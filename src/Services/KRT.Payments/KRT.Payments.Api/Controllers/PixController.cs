using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KRT.BuildingBlocks.Domain.Responses;
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
    private readonly IPixTransactionRepository _pixRepo;

    public PixController(IMediator mediator, IPixTransactionRepository pixRepo)
    {
        _mediator = mediator;
        _pixRepo = pixRepo;
    }

    /// <summary>POST /api/v1/pix/transfer — Saga Orchestrator</summary>
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] ProcessPixCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsValid)
        {
            var code = result.Errors.Any(e =>
                e.Contains("insuficiente", StringComparison.OrdinalIgnoreCase)) ? 422 : 400;

            return StatusCode(code, ApiResponse.Fail(result.Errors));
        }

        return Ok(ApiResponse<object>.Ok(new { transactionId = result.Id }));
    }

    /// <summary>GET /api/v1/pix/history/{accountId}</summary>
    [HttpGet("history/{accountId}")]
    public async Task<IActionResult> History(Guid accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var transactions = await _pixRepo.GetByAccountIdAsync(accountId, page, pageSize);

        return Ok(new PagedResponse<object>
        {
            Data = transactions.Select(t => new
            {
                t.Id, t.SourceAccountId, t.DestinationAccountId,
                t.Amount, t.PixKey, Status = t.Status.ToString(),
                t.Description, t.CreatedAt, t.CompletedAt
            }).Cast<object>().ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = transactions.Count
        });
    }

    /// <summary>GET /api/v1/pix/{id}</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var tx = await _pixRepo.GetByIdAsync(id);
        if (tx == null) return NotFound(ApiResponse.Fail("Transação não encontrada."));

        return Ok(ApiResponse<object>.Ok(new
        {
            tx.Id, tx.SourceAccountId, tx.DestinationAccountId,
            tx.Amount, tx.Currency, tx.PixKey,
            Status = tx.Status.ToString(), tx.Description,
            tx.FailureReason, tx.SourceDebited, tx.DestinationCredited,
            tx.CreatedAt, tx.CompletedAt
        }));
    }
}
