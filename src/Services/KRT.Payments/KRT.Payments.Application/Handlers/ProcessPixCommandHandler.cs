using KRT.BuildingBlocks.Domain;
using KRT.BuildingBlocks.Infrastructure.Outbox;
using KRT.Payments.Application.Commands;
using KRT.Payments.Application.Events;
using KRT.Payments.Application.Interfaces;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KRT.Payments.Application.Handlers;

public class ProcessPixCommandHandler : IRequestHandler<ProcessPixCommand, CommandResult>
{
    private readonly IPixTransactionRepository _repository;
    private readonly IOnboardingServiceClient _onboardingClient;
    private readonly IOutboxWriter _outbox;
    private readonly ILogger<ProcessPixCommandHandler> _logger;

    public ProcessPixCommandHandler(
        IPixTransactionRepository repository,
        IOnboardingServiceClient onboardingClient,
        IOutboxWriter outbox,
        ILogger<ProcessPixCommandHandler> logger)
    {
        _repository = repository;
        _onboardingClient = onboardingClient;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task<CommandResult> Handle(ProcessPixCommand command, CancellationToken ct)
    {
        var existing = await _repository.GetByIdempotencyKeyAsync(command.IdempotencyKey);
        if (existing != null) return CommandResult.Success(existing.Id);

        var tx = new PixTransaction(
            command.SourceAccountId, command.DestinationAccountId,
            command.Amount, command.PixKey, command.Description, command.IdempotencyKey);

        await _repository.AddAsync(tx);
        await _repository.UnitOfWork.CommitAsync(ct);

        _outbox.Add(OutboxMessage.Create(new PixTransferInitiatedEvent(
            tx.Id, tx.SourceAccountId, tx.DestinationAccountId, tx.Amount, tx.PixKey, DateTime.UtcNow), tx.Id.ToString()));
        await _outbox.SaveAsync(ct);

        var debitResult = await _onboardingClient.DebitAccountAsync(
            command.SourceAccountId, command.Amount, "Pix para " + command.PixKey, ct);

        if (!debitResult.Success)
        {
            tx.MarkFailed(debitResult.Error ?? "Falha no debito");
            _repository.Update(tx);
            await _repository.UnitOfWork.CommitAsync(ct);
            _outbox.Add(OutboxMessage.Create(new PixTransferFailedEvent(
                tx.Id, tx.SourceAccountId, tx.DestinationAccountId, tx.Amount, tx.PixKey,
                debitResult.Error ?? "Debit failed", false, DateTime.UtcNow), tx.Id.ToString()));
            await _outbox.SaveAsync(ct);
            return CommandResult.Failure(debitResult.Error ?? "Saldo insuficiente.");
        }

        tx.MarkSourceDebited();
        _repository.Update(tx);
        await _repository.UnitOfWork.CommitAsync(ct);

        var creditResult = await _onboardingClient.CreditAccountAsync(
            command.DestinationAccountId, command.Amount, "Pix de " + command.PixKey, ct);

        if (!creditResult.Success)
        {
            var comp = await _onboardingClient.CreditAccountAsync(
                command.SourceAccountId, command.Amount, "Estorno Pix " + tx.Id, ct);
            tx.MarkCompensated();
            _repository.Update(tx);
            await _repository.UnitOfWork.CommitAsync(ct);
            _outbox.Add(OutboxMessage.Create(new PixTransferFailedEvent(
                tx.Id, tx.SourceAccountId, tx.DestinationAccountId, tx.Amount, tx.PixKey,
                creditResult.Error ?? "Credit failed", comp.Success, DateTime.UtcNow), tx.Id.ToString()));
            await _outbox.SaveAsync(ct);
            return CommandResult.Failure("Falha ao creditar destino. Valor estornado.");
        }

        tx.MarkDestinationCredited();
        _repository.Update(tx);
        await _repository.UnitOfWork.CommitAsync(ct);
        _outbox.Add(OutboxMessage.Create(new PixTransferCompletedEvent(
            tx.Id, tx.SourceAccountId, tx.DestinationAccountId, tx.Amount, tx.PixKey, "BRL", DateTime.UtcNow), tx.Id.ToString()));
        await _outbox.SaveAsync(ct);
        return CommandResult.Success(tx.Id);
    }
}
