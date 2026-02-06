using Microsoft.AspNetCore.Mvc;
using KRT.Payments.Application.Commands;
using KRT.Payments.Domain.Interfaces;
using MediatR;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/pix")]
public class PixController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPixTransactionRepository _pixRepo;

    public PixController(IMediator mediator, IPixTransactionRepository pixRepo)
    {
        _mediator = mediator;
        _pixRepo = pixRepo;
    }

    /// <summary>
    /// Executa transferência Pix via Saga Orchestrator.
    /// POST /api/v1/pix/transfer
    /// </summary>
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] ProcessPixCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsValid)
        {
            var statusCode = result.Errors.Any(e =>
                e.Contains("insuficiente", StringComparison.OrdinalIgnoreCase) ||
                e.Contains("Saldo", StringComparison.OrdinalIgnoreCase))
                ? 422 : 400;

            return StatusCode(statusCode, new { success = false, errors = result.Errors });
        }

        return Ok(new { success = true, transactionId = result.Id });
    }

    /// <summary>
    /// Histórico de transações por conta.
    /// GET /api/v1/pix/history/{accountId}
    /// </summary>
    [HttpGet("history/{accountId}")]
    public async Task<IActionResult> History(Guid accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var transactions = await _pixRepo.GetByAccountIdAsync(accountId, page, pageSize);

        var response = transactions.Select(t => new
        {
            t.Id,
            t.SourceAccountId,
            t.DestinationAccountId,
            t.Amount,
            t.PixKey,
            Status = t.Status.ToString(),
            t.Description,
            t.CreatedAt,
            t.CompletedAt
        });

        return Ok(response);
    }

    /// <summary>
    /// Detalhe de uma transação.
    /// GET /api/v1/pix/{id}
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tx = await _pixRepo.GetByIdAsync(id);
        if (tx == null) return NotFound();

        return Ok(new
        {
            tx.Id,
            tx.SourceAccountId,
            tx.DestinationAccountId,
            tx.Amount,
            tx.Currency,
            tx.PixKey,
            Status = tx.Status.ToString(),
            tx.Description,
            tx.FailureReason,
            tx.SourceDebited,
            tx.DestinationCredited,
            tx.CreatedAt,
            tx.CompletedAt
        });
    }
}
