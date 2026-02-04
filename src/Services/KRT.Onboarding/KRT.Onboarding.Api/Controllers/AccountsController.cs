using KRT.Onboarding.Application.Accounts.DTOs.Requests;
using KRT.Onboarding.Application.Accounts.DTOs.Responses;
using KRT.Onboarding.Application.Accounts.Services;
using Microsoft.AspNetCore.Mvc;
using KRT.BuildingBlocks.Domain;

namespace KRT.Onboarding.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(IAccountService accountService, ILogger<AccountsController> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _accountService.GetByIdAsync(id, ct);
        
        // Verifica se falhou ou se o valor é nulo
        if (result.IsFailure || result.Value == null)
            return NotFound(new { message = "Conta não encontrada" });

        return Ok(result.Value);
    }

    [HttpGet("by-number/{accountNumber}")]
    public async Task<IActionResult> GetByAccountNumber(string accountNumber, CancellationToken ct)
    {
        var result = await _accountService.GetByAccountNumberAsync(accountNumber, ct);
        if (result.IsFailure) return NotFound();
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/balance")]
    public async Task<IActionResult> GetBalance(Guid id, CancellationToken ct)
    {
        var result = await _accountService.GetBalanceAsync(id, ct);
        if (result.IsFailure) return NotFound();
        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/statement")]
    public async Task<IActionResult> GetStatement(Guid id, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, CancellationToken ct)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;
        var result = await _accountService.GetStatementAsync(id, start, end, ct);
        if (result.IsFailure) return NotFound();
        return Ok(result.Value);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Creating account for document {Document}", request.CustomerDocument);

        var result = await _accountService.CreateAsync(request, ct);

        if (result.IsFailure)
            return BadRequest(new ProblemDetails
            {
                Title = "Erro ao criar conta",
                Detail = result.Error,
                Status = StatusCodes.Status400BadRequest,
                Extensions = { { "errorCode", result.ErrorCode } }
            });

        // O Value do result é o GUID da nova conta
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        var result = await _accountService.ActivateAsync(id, ct);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result);
    }

    [HttpPost("{id:guid}/block")]
    public async Task<IActionResult> Block(Guid id, [FromBody] BlockAccountRequest request, CancellationToken ct)
    {
        var result = await _accountService.BlockAsync(id, request.Reason, ct);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [HttpPost("{id:guid}/unblock")]
    public async Task<IActionResult> Unblock(Guid id, CancellationToken ct)
    {
        var result = await _accountService.UnblockAsync(id, ct);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return NoContent();
    }

    [HttpPost("{id:guid}/debit")]
    public async Task<IActionResult> Debit(Guid id, [FromBody] DebitAccountRequest request, CancellationToken ct)
    {
        var debitRequest = request with { AccountId = id };
        var result = await _accountService.DebitAsync(debitRequest, ct);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/credit")]
    public async Task<IActionResult> Credit(Guid id, [FromBody] CreditAccountRequest request, CancellationToken ct)
    {
        var creditRequest = request with { AccountId = id };
        var result = await _accountService.CreditAsync(creditRequest, ct);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest request, CancellationToken ct)
    {
        var result = await _accountService.TransferAsync(request, ct);
        if (result.IsFailure) return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }
}
