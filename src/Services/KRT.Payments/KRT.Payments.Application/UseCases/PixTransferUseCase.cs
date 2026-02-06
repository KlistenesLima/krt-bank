using KRT.Payments.Application.DTOs;
using KRT.Payments.Application.Services;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace KRT.Payments.Application.UseCases;

/// <summary>
/// SAGA ORCHESTRATOR - Coordena a transferencia Pix entre duas contas.
/// 
/// Fluxo:
///   1. Validar + checar idempotencia
///   2. Criar PixTransaction (Pending)
///   3. Debitar conta origem (HTTP -> Onboarding)
///   4. Creditar conta destino (HTTP -> Onboarding)
///   5. Se falhar no passo 4 -> COMPENSAR (reverter debito)
///   6. Marcar transacao como Completed ou Compensated
///
/// Patterns: Saga Orchestrator, Idempotency, Compensation
/// </summary>
public class PixTransferUseCase
{
    private readonly IPixTransactionRepository _pixRepo;
    private readonly IOnboardingServiceClient _accountClient;
    private readonly ILogger<PixTransferUseCase> _logger;

    public PixTransferUseCase(
        IPixTransactionRepository pixRepo,
        IOnboardingServiceClient accountClient,
        ILogger<PixTransferUseCase> logger)
    {
        _pixRepo = pixRepo;
        _accountClient = accountClient;
        _logger = logger;
    }

    public async Task<PixTransferResponse> ExecuteAsync(PixTransferRequest request)
    {
        // === STEP 0: Idempotencia ===
        var existing = await _pixRepo.GetByIdempotencyKeyAsync(request.IdempotencyKey);
        if (existing != null)
        {
            _logger.LogInformation("Transacao duplicada detectada. IdempotencyKey: {Key}", request.IdempotencyKey);
            return MapToResponse(existing);
        }

        // === STEP 1: Validacao ===
        if (request.Amount <= 0)
            throw new ArgumentException("Valor deve ser positivo");
        if (request.SourceAccountId == request.DestinationAccountId)
            throw new ArgumentException("Conta origem e destino devem ser diferentes");

        // === STEP 2: Criar transacao (Pending) ===
        var transaction = new PixTransaction(
            request.SourceAccountId,
            request.DestinationAccountId,
            request.PixKey,
            request.Amount,
            request.Description,
            request.IdempotencyKey);

        await _pixRepo.AddAsync(transaction);
        await _pixRepo.UnitOfWork.CommitAsync();

        _logger.LogInformation("Pix {TxId} criado. Iniciando saga...", transaction.Id);

        try
        {
            // === STEP 3: Debitar conta origem ===
            var debitResult = await _accountClient.DebitAccountAsync(
                request.SourceAccountId,
                request.Amount,
                "Pix enviado - " + transaction.Id);

            if (!debitResult.Success)
            {
                transaction.MarkFailed("Falha no debito: " + debitResult.Error);
                _pixRepo.Update(transaction);
                await _pixRepo.UnitOfWork.CommitAsync();

                _logger.LogWarning("Pix {TxId} falhou no debito: {Error}", transaction.Id, debitResult.Error);
                return MapToResponse(transaction);
            }

            transaction.MarkSourceDebited();
            _pixRepo.Update(transaction);
            await _pixRepo.UnitOfWork.CommitAsync();

            _logger.LogInformation("Pix {TxId} - Conta origem debitada", transaction.Id);

            // === STEP 4: Creditar conta destino ===
            var creditResult = await _accountClient.CreditAccountAsync(
                request.DestinationAccountId,
                request.Amount,
                "Pix recebido - " + transaction.Id);

            if (!creditResult.Success)
            {
                _logger.LogWarning("Pix {TxId} falhou no credito. Iniciando compensacao...", transaction.Id);

                // === COMPENSACAO: Reverter debito ===
                await CompensateAsync(transaction);
                return MapToResponse(transaction);
            }

            // === STEP 5: Sucesso total ===
            transaction.MarkDestinationCredited();
            _pixRepo.Update(transaction);
            await _pixRepo.UnitOfWork.CommitAsync();

            _logger.LogInformation("Pix {TxId} COMPLETADO com sucesso!", transaction.Id);

            // TODO: Publicar PixCompletedIntegrationEvent no Kafka

            return MapToResponse(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pix {TxId} - Erro inesperado na saga", transaction.Id);

            if (transaction.SourceDebited)
            {
                await CompensateAsync(transaction);
            }
            else
            {
                transaction.MarkFailed("Erro inesperado: " + ex.Message);
                _pixRepo.Update(transaction);
                await _pixRepo.UnitOfWork.CommitAsync();
            }

            return MapToResponse(transaction);
        }
    }

    /// <summary>
    /// COMPENSACAO: Reverte o debito da conta origem quando o credito falha.
    /// </summary>
    private async Task CompensateAsync(PixTransaction transaction)
    {
        try
        {
            var compensationResult = await _accountClient.CreditAccountAsync(
                transaction.SourceAccountId,
                transaction.Amount,
                "Compensacao Pix - " + transaction.Id);

            if (compensationResult.Success)
            {
                transaction.MarkCompensated();
                _logger.LogInformation("Pix {TxId} - Compensacao realizada com sucesso", transaction.Id);
            }
            else
            {
                transaction.MarkFailed("Falha na compensacao: " + compensationResult.Error);
                _logger.LogError("Pix {TxId} - FALHA NA COMPENSACAO! Requer intervencao manual.", transaction.Id);
            }
        }
        catch (Exception ex)
        {
            transaction.MarkFailed("Erro na compensacao: " + ex.Message);
            _logger.LogError(ex, "Pix {TxId} - ERRO CRITICO NA COMPENSACAO!", transaction.Id);
        }

        _pixRepo.Update(transaction);
        await _pixRepo.UnitOfWork.CommitAsync();
    }

    public async Task<List<TransactionHistoryResponse>> GetHistoryAsync(Guid accountId, int page, int pageSize)
    {
        var transactions = await _pixRepo.GetByAccountIdAsync(accountId, page, pageSize);
        return transactions.Select(t => new TransactionHistoryResponse(
            t.Id,
            t.SourceAccountId,
            t.DestinationAccountId,
            t.Amount,
            t.Status.ToString(),
            t.Description,
            t.CreatedAt
        )).ToList();
    }

    private static PixTransferResponse MapToResponse(PixTransaction tx)
    {
        return new PixTransferResponse(
            tx.Id,
            tx.Status.ToString(),
            tx.Amount,
            tx.Currency,
            tx.CreatedAt,
            tx.CompletedAt);
    }
}
