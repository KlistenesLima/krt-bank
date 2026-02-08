using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KRT.Onboarding.Application.Commands;
using KRT.Onboarding.Domain.Interfaces;
using MediatR;

namespace KRT.Onboarding.Api.Controllers;

[ApiController]
[Route("api/v1/accounts")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class AccountsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IAccountRepository _repository;
    private readonly ICacheService _cache;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(
        IMediator mediator,
        IAccountRepository repository,
        ICacheService cache,
        ILogger<AccountsController> logger)
    {
        _mediator = mediator;
        _repository = repository;
        _cache = cache;
        _logger = logger;
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
        // 1. Tenta buscar no cache Redis
        var cacheKey = $"account:{id}";
        var cached = await _cache.GetAsync<AccountDto>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache HIT for account {AccountId}", id);
            return Ok(cached);
        }

        // 2. Cache miss - busca no banco
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (account == null) return NotFound();

        var dto = new AccountDto(
            account.Id,
            account.CustomerName,
            account.Document,
            account.Email,
            account.Balance,
            account.Status.ToString(),
            account.Type.ToString()
        );

        // 3. Salva no cache (5 minutos)
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
        _logger.LogDebug("Cached account {AccountId} for 5 min", id);

        return Ok(dto);
    }

    /// <summary>
    /// Busca conta pelo CPF (documento). Usado pelo dashboard.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("by-document/{document}")]
    public async Task<IActionResult> GetByDocument(string document)
    {
        var cleanDoc = document.Replace(".", "").Replace("-", "").Trim();
        var cacheKey = $"account:doc:{cleanDoc}";
        var cached = await _cache.GetAsync<AccountDto>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache HIT for document {Document}", cleanDoc);
            return Ok(cached);
        }

        var account = await _repository.GetByCpfAsync(cleanDoc, CancellationToken.None);
        if (account == null) return NotFound();

        var dto = new AccountDto(
            account.Id,
            account.CustomerName,
            account.Document,
            account.Email,
            account.Balance,
            account.Status.ToString(),
            account.Type.ToString()
        );

        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5));
        return Ok(dto);
    }

    [HttpGet("{id}/balance")]
    public async Task<IActionResult> GetBalance(Guid id)
    {
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (account == null) return NotFound();
        return Ok(new { accountId = id, availableAmount = account.Balance });
    }

    // === ENDPOINTS PARA A SAGA ===

    public record DebitRequest(decimal Amount, string Reason);
    public record CreditRequest(decimal Amount, string Reason);

    /// <summary>
    /// Debita valor da conta. Usado pela Saga do Pix.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("{id}/debit")]
    public async Task<IActionResult> Debit(Guid id, [FromBody] DebitRequest request)
    {
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        if (account == null)
            return NotFound(new { Success = false, Error = "Conta nao encontrada", NewBalance = 0m });

        try
        {
            account.Debit(request.Amount);

            var uow = _repository.UnitOfWork;
            await uow.CommitAsync(CancellationToken.None);

            // Invalida cache apos alteracao de saldo
            await _cache.RemoveAsync($"account:{id}");
            await _cache.RemoveAsync($"account:doc:{account.Document}");

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
    [AllowAnonymous]
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

            // Invalida cache apos alteracao de saldo
            await _cache.RemoveAsync($"account:{id}");
            await _cache.RemoveAsync($"account:doc:{account.Document}");

            return Ok(new { Success = true, Error = (string?)null, NewBalance = account.Balance });
        }
        catch (Exception ex)
        {
            return UnprocessableEntity(new { Success = false, Error = ex.Message, NewBalance = account.Balance });
        }
    }
}

// DTO interno para serializacao do cache (records sao serializaveis)
public record AccountDto(
    Guid Id,
    string CustomerName,
    string Document,
    string Email,
    decimal Balance,
    string Status,
    string Type
);