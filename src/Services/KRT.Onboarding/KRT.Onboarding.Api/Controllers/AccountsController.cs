using Microsoft.AspNetCore.Mvc;
using KRT.Onboarding.Application.Commands;
using KRT.Onboarding.Domain.Interfaces;
using MediatR;

namespace KRT.Onboarding.Api.Controllers;

[ApiController]
[Route("api/v1/accounts")]
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
    public async Task<IActionResult> Create([FromBody] CreateAccountCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsValid) return BadRequest(new { errors = result.Errors });
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { id = result.Id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (account == null) return NotFound();

        return Ok(new
        {
            account.Id,
            account.CustomerName,
            account.Document,
            account.Email,
            account.Balance,
            Status = account.Status.ToString(),
            Type = account.Type.ToString()
        });
    }

    [HttpGet("{id}/balance")]
    public async Task<IActionResult> GetBalance(Guid id)
    {
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (account == null) return NotFound();
        return Ok(new { AccountId = id, AvailableAmount = account.Balance });
    }

    // === ENDPOINTS PARA A SAGA ===

    public record DebitRequest(decimal Amount, string Reason);
    public record CreditRequest(decimal Amount, string Reason);

    /// <summary>
    /// Debita valor da conta. Usado pela Saga do Pix.
    /// </summary>
    [HttpPost("{id}/debit")]
    public async Task<IActionResult> Debit(Guid id, [FromBody] DebitRequest request)
    {
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (account == null)
            return NotFound(new { Success = false, Error = "Conta nao encontrada", NewBalance = 0m });

        try
        {
            account.Debit(request.Amount);

            // Commit via UnitOfWork (ApplicationDbContext)
            var uow = _repository.UnitOfWork;
            await uow.CommitAsync(CancellationToken.None);

            return Ok(new { Success = true, Error = (string?)null, NewBalance = account.Balance });
        }
        catch (Exception ex)
        {
            return UnprocessableEntity(new { Success = false, Error = ex.Message, NewBalance = account.Balance });
        }
    }

    /// <summary>
    /// Credita valor na conta. Usado pela Saga do Pix.
    /// </summary>
    [HttpPost("{id}/credit")]
    public async Task<IActionResult> Credit(Guid id, [FromBody] CreditRequest request)
    {
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (account == null)
            return NotFound(new { Success = false, Error = "Conta nao encontrada", NewBalance = 0m });

        try
        {
            account.Credit(request.Amount);

            var uow = _repository.UnitOfWork;
            await uow.CommitAsync(CancellationToken.None);

            return Ok(new { Success = true, Error = (string?)null, NewBalance = account.Balance });
        }
        catch (Exception ex)
        {
            return UnprocessableEntity(new { Success = false, Error = ex.Message, NewBalance = account.Balance });
        }
    }
}
