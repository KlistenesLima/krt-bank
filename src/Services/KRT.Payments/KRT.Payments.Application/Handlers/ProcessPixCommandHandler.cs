using KRT.BuildingBlocks.Domain;
using KRT.Payments.Application.Commands;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KRT.Payments.Application.Handlers;

/// <summary>
/// Handler agora cria a transação e retorna imediatamente.
/// O FraudAnalysisWorker processa assincronamente:
/// Fraud Analysis → Saga (Debit → Credit) → Notificação
/// </summary>
public class ProcessPixCommandHandler : IRequestHandler<ProcessPixCommand, CommandResult>
{
    private readonly IPixTransactionRepository _repository;
    private readonly ILogger<ProcessPixCommandHandler> _logger;

    public ProcessPixCommandHandler(
        IPixTransactionRepository repository,
        ILogger<ProcessPixCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CommandResult> Handle(ProcessPixCommand command, CancellationToken ct)
    {
        // Idempotência
        var existing = await _repository.GetByIdempotencyKeyAsync(command.IdempotencyKey, ct);
        if (existing != null) return CommandResult.Success(existing.Id);

        // Cria transação em PendingAnalysis
        var tx = new PixTransaction(
            command.SourceAccountId,
            command.DestinationAccountId,
            command.Amount,
            command.PixKey,
            command.Description,
            command.IdempotencyKey);

        await _repository.AddAsync(tx, ct);
        await _repository.UnitOfWork.CommitAsync(ct);

        _logger.LogInformation(
            "Pix {TxId} criado (PendingAnalysis). FraudWorker processará assincronamente.",
            tx.Id);

        return CommandResult.Success(tx.Id);
    }
}
