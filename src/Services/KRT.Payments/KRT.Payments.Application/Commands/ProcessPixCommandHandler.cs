using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Interfaces;
using KRT.Payments.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KRT.Payments.Application.Commands;

/// <summary>
/// SAGA ORCHESTRATOR — Processa Pix com compensação automática.
/// Fluxo: Validate → Create TX → Debit Source → Credit Dest → Complete
/// Se Credit falha: Compensa (re-credita Source)
/// </summary>
public class ProcessPixCommandHandler : IRequestHandler<ProcessPixCommand, CommandResult>
{
    private readonly IPixTransactionRepository _pixRepo;
    private readonly IOnboardingServiceClient _onboardingClient;
    private readonly ILogger<ProcessPixCommandHandler> _logger;

    public ProcessPixCommandHandler(
        IPixTransactionRepository pixRepo,
        IOnboardingServiceClient onboardingClient,
        ILogger<ProcessPixCommandHandler> logger)
    {
        _pixRepo = pixRepo;
        _onboardingClient = onboardingClient;
        _logger = logger;
    }

    public async Task<CommandResult> Handle(ProcessPixCommand request, CancellationToken ct)
    {
        // ── 1. VALIDAÇÃO ──
        if (request.Amount <= 0)
            return CommandResult.Failure("O valor deve ser positivo.");

        if (request.SourceAccountId == request.DestinationAccountId)
            return CommandResult.Failure("Contas de origem e destino devem ser diferentes.");

        // ── 2. IDEMPOTÊNCIA ──
        var existing = await _pixRepo.GetByIdempotencyKeyAsync(request.IdempotencyKey);
        if (existing != null)
        {
            _logger.LogInformation("Transação duplicada detectada: {Key}", request.IdempotencyKey);
            return CommandResult.Success(existing.Id);
        }

        // ── 3. CRIAR TRANSAÇÃO (Pending) ──
        var tx = new PixTransaction(
            request.SourceAccountId,
            request.DestinationAccountId,
            request.Amount,
            request.PixKey,
            request.Description,
            request.IdempotencyKey);

        await _pixRepo.AddAsync(tx);
        await _pixRepo.UnitOfWork.CommitAsync(ct);

        _logger.LogInformation("Pix TX criada: {TxId} | {Amount} BRL", tx.Id, tx.Amount);

        // ── 4. STEP 1: DEBITAR ORIGEM ──
        var debitResult = await _onboardingClient.DebitAccountAsync(
            request.SourceAccountId, request.Amount, $"Pix enviado - TX:{tx.Id}");

        if (!debitResult.Success)
        {
            tx.MarkFailed(debitResult.Error ?? "Falha ao debitar conta origem");
            _pixRepo.Update(tx);
            await _pixRepo.UnitOfWork.CommitAsync(ct);

            _logger.LogWarning("Pix FALHOU no débito: {TxId} - {Error}", tx.Id, debitResult.Error);
            return CommandResult.Failure(debitResult.Error ?? "Saldo insuficiente para realizar a transação.");
        }

        tx.MarkSourceDebited();
        _pixRepo.Update(tx);
        await _pixRepo.UnitOfWork.CommitAsync(ct);

        // ── 5. STEP 2: CREDITAR DESTINO ──
        var creditResult = await _onboardingClient.CreditAccountAsync(
            request.DestinationAccountId, request.Amount, $"Pix recebido - TX:{tx.Id}");

        if (!creditResult.Success)
        {
            _logger.LogWarning("Pix crédito FALHOU, iniciando COMPENSAÇÃO: {TxId}", tx.Id);

            // ── 6. COMPENSAÇÃO: RE-CREDITAR ORIGEM ──
            var compensateResult = await _onboardingClient.CreditAccountAsync(
                request.SourceAccountId, request.Amount, $"Compensação Pix - TX:{tx.Id}");

            if (compensateResult.Success)
            {
                tx.MarkCompensated();
                _logger.LogInformation("Pix COMPENSADO com sucesso: {TxId}", tx.Id);
            }
            else
            {
                tx.MarkFailed($"Compensação falhou: {compensateResult.Error}");
                _logger.LogError("ALERTA: Compensação FALHOU para TX:{TxId}. Intervenção manual necessária!", tx.Id);
            }

            _pixRepo.Update(tx);
            await _pixRepo.UnitOfWork.CommitAsync(ct);
            return CommandResult.Failure(creditResult.Error ?? "Conta destino não encontrada.");
        }

        // ── 7. SUCESSO COMPLETO ──
        tx.MarkDestinationCredited();
        _pixRepo.Update(tx);
        await _pixRepo.UnitOfWork.CommitAsync(ct);

        _logger.LogInformation("Pix CONCLUÍDO: {TxId} | {Amount} BRL", tx.Id, tx.Amount);
        return CommandResult.Success(tx.Id);
    }
}
