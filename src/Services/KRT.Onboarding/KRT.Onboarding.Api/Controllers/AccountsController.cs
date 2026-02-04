using Microsoft.AspNetCore.Mvc;
using KRT.Onboarding.Application.DTOs;
using KRT.Onboarding.Infra.Data.Context;
using KRT.Onboarding.Application.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KRT.Onboarding.Api.Controllers;

[ApiController]
[Route("api/v1/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ApplicationDbContext _context;

    public AccountsController(IMediator mediator, ApplicationDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsValid) return BadRequest(new { errors = result.Errors });
        return Ok(new { id = result.Id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var account = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        if (account == null) return NotFound();
        return Ok(account);
    }

    [HttpGet("{id}/balance")]
    public async Task<IActionResult> GetBalance(Guid id)
    {
        var acc = await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        if (acc == null) return NotFound();
        return Ok(new BalanceDto { AccountId = id, AvailableAmount = acc.Balance });
    }

    [HttpGet("{id}/statement")]
    public async Task<IActionResult> GetStatement(Guid id)
    {
        var txs = await _context.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == id)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new StatementDto 
            { 
                Id = t.Id, 
                Amount = t.Amount, 
                Type = t.Description, 
                CreatedAt = t.CreatedAt 
            })
            .ToListAsync();

        return Ok(txs);
    }

    [HttpPost("{id}/pix")]
    public async Task<IActionResult> PerformPix(Guid id, [FromBody] PerformPixCommand command)
    {
        command.AccountId = id; 
        var result = await _mediator.Send(command);
        if (!result.IsValid) return BadRequest(new { errors = result.Errors });
        return Ok(new { transactionId = result.Id });
    }
}
