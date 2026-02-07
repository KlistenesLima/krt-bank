using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KRT.Payments.Application.Commands;
using KRT.Payments.Domain.Interfaces;
using MediatR;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/pix")]
[Authorize]
public class PixController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPixTransactionRepository _repository;

    public PixController(IMediator mediator, IPixTransactionRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    /// <summary>
    /// Inicia uma transferência Pix. A transação é criada e entra na fila
    /// de análise anti-fraude assíncrona.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ProcessPix([FromBody] PixTransferRequest request)
    {
        var command = new ProcessPixCommand
        {
            SourceAccountId = request.SourceAccountId,
            DestinationAccountId = request.DestinationAccountId,
            Amount = request.Amount,
            PixKey = request.PixKey,
            Description = request.Description ?? "",
            IdempotencyKey = request.IdempotencyKey
        };

        var result = await _mediator.Send(command);

        if (!result.IsValid)
            return BadRequest(new { success = false, error = result.Errors.FirstOrDefault() });

        // Retorna 202 Accepted (processamento assíncrono)
        return Accepted(new
        {
            success = true,
            transactionId = result.Id,
            status = "PendingAnalysis",
            message = "Transação recebida. Análise anti-fraude em andamento. Consulte GET /api/v1/pix/{id} para acompanhar."
        });
    }

    /// <summary>
    /// Consulta o status de uma transação Pix (inclui resultado da análise de fraude).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        var tx = await _repository.GetByIdAsync(id);
        if (tx == null) return NotFound(new { error = "Transação não encontrada" });

        return Ok(new
        {
            transactionId = tx.Id,
            sourceAccountId = tx.SourceAccountId,
            destinationAccountId = tx.DestinationAccountId,
            amount = tx.Amount,
            currency = tx.Currency,
            pixKey = tx.PixKey,
            status = tx.Status.ToString(),
            description = tx.Description,
            failureReason = tx.FailureReason,
            createdAt = tx.CreatedAt,
            completedAt = tx.CompletedAt,
            fraud = new
            {
                score = tx.FraudScore,
                details = tx.FraudDetails,
                analyzedAt = tx.FraudAnalyzedAt
            }
        });
    }

    /// <summary>
    /// Lista transações de uma conta (extrato Pix).
    /// </summary>
    [HttpGet("account/{accountId:guid}")]
    public async Task<IActionResult> GetByAccount(Guid accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var txs = await _repository.GetByAccountIdAsync(accountId, page, pageSize);
        return Ok(txs.Select(tx => new
        {
            transactionId = tx.Id,
            sourceAccountId = tx.SourceAccountId,
            destinationAccountId = tx.DestinationAccountId,
            amount = tx.Amount,
            status = tx.Status.ToString(),
            fraudScore = tx.FraudScore,
            description = tx.Description,
            createdAt = tx.CreatedAt,
            completedAt = tx.CompletedAt
        }));
    }
}

public record PixTransferRequest(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    string PixKey,
    decimal Amount,
    string? Description,
    Guid IdempotencyKey
);

