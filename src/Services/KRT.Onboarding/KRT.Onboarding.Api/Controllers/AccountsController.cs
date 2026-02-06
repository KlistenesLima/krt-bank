using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KRT.BuildingBlocks.Domain.Responses;
using KRT.Onboarding.Application.Commands;
using KRT.Onboarding.Domain.Interfaces;
using MediatR;

namespace KRT.Onboarding.Api.Controllers;

[ApiController]
[Route("api/v1/accounts")]
[Authorize]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAccountRepository _repository;

    public AccountsController(IMediator mediator, IAccountRepository repository)
    {
        _mediator = mediator;
        _repository = repository;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Create([FromBody] CreateAccountCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsValid) return BadRequest(ApiResponse.Fail(result.Errors));
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<object>.Ok(new { id = result.Id }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (account == null) return NotFound(ApiResponse.Fail("Conta não encontrada."));
        return Ok(ApiResponse<object>.Ok(new
        {
            account.Id, account.CustomerName, Document = account.Document,
            account.Email, account.Balance, Status = account.Status.ToString(),
            Type = account.Type.ToString(), Currency = "BRL"
        }));
    }

    [HttpGet("{id}/balance")]
    public async Task<IActionResult> GetBalance(Guid id)
    {
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (account == null) return NotFound(ApiResponse.Fail("Conta não encontrada."));
        return Ok(ApiResponse<object>.Ok(new
        {
            AccountId = id, AvailableAmount = account.Balance,
            Currency = "BRL", UpdatedAt = account.UpdatedAt ?? account.CreatedAt
        }));
    }

    [HttpGet("{id}/statement")]
    public async Task<IActionResult> GetStatement(Guid id)
    {
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (account == null) return NotFound(ApiResponse.Fail("Conta não encontrada."));
        return Ok(ApiResponse<object>.Ok(Array.Empty<object>()));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var accounts = await _repository.GetAllAsync(CancellationToken.None);
        return Ok(ApiResponse<object>.Ok(accounts.Select(a => new
        {
            a.Id, a.CustomerName, a.Document, a.Email,
            a.Balance, Status = a.Status.ToString(), Type = a.Type.ToString()
        }).ToList()));
    }

    [HttpPost("{id}/activate")]
    public async Task<IActionResult> Activate(Guid id)
    {
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (account == null) return NotFound(ApiResponse.Fail("Conta não encontrada."));
        try { account.Activate(); await _repository.UnitOfWork.CommitAsync(CancellationToken.None); return Ok(ApiResponse.Ok()); }
        catch (Exception ex) { return UnprocessableEntity(ApiResponse.Fail(ex.Message)); }
    }

    [HttpPost("{id}/block")]
    public async Task<IActionResult> Block(Guid id, [FromBody] BlockRequest request)
    {
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (account == null) return NotFound(ApiResponse.Fail("Conta não encontrada."));
        try { account.Block(request.Reason); await _repository.UnitOfWork.CommitAsync(CancellationToken.None); return Ok(ApiResponse.Ok()); }
        catch (Exception ex) { return UnprocessableEntity(ApiResponse.Fail(ex.Message)); }
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> Close(Guid id, [FromBody] CloseRequest request)
    {
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (account == null) return NotFound(ApiResponse.Fail("Conta não encontrada."));
        try { account.Close(request.Reason); await _repository.UnitOfWork.CommitAsync(CancellationToken.None); return Ok(ApiResponse.Ok()); }
        catch (Exception ex) { return UnprocessableEntity(ApiResponse.Fail(ex.Message)); }
    }

    // ===== SAGA (Service-to-Service) =====
    public record DebitRequest(decimal Amount, string Reason);
    public record CreditRequest(decimal Amount, string Reason);
    public record BlockRequest(string Reason);
    public record CloseRequest(string Reason);

    [HttpPost("{id}/debit")]
    [AllowAnonymous]
    public async Task<IActionResult> Debit(Guid id, [FromBody] DebitRequest request)
    {
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (account == null) return NotFound(new { Success = false, Error = "Conta não encontrada", NewBalance = 0m });
        try
        {
            account.Debit(request.Amount);
            await _repository.UnitOfWork.CommitAsync(CancellationToken.None);
            return Ok(new { Success = true, Error = (string?)null, NewBalance = account.Balance });
        }
        catch (Exception ex) { return UnprocessableEntity(new { Success = false, Error = ex.Message, NewBalance = account.Balance }); }
    }

    [HttpPost("{id}/credit")]
    [AllowAnonymous]
    public async Task<IActionResult> Credit(Guid id, [FromBody] CreditRequest request)
    {
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (account == null) return NotFound(new { Success = false, Error = "Conta não encontrada", NewBalance = 0m });
        try
        {
            account.Credit(request.Amount);
            await _repository.UnitOfWork.CommitAsync(CancellationToken.None);
            return Ok(new { Success = true, Error = (string?)null, NewBalance = account.Balance });
        }
        catch (Exception ex) { return UnprocessableEntity(new { Success = false, Error = ex.Message, NewBalance = account.Balance }); }
    }
}
