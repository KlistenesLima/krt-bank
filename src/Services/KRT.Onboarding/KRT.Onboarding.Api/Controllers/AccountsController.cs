using Microsoft.AspNetCore.Mvc;
using KRT.Onboarding.Application.Commands;
using KRT.Onboarding.Domain.Interfaces; // Para IAccountRepository
using KRT.Onboarding.Application.Accounts.DTOs.Responses; // Se houver DTOs
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
        
        // Retorna 201 Created
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new { id = result.Id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Query direta no Repositório (CQRS Simplificado para leitura)
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        
        if (account == null) return NotFound();
        
        return Ok(new 
        { 
            account.Id, 
            account.CustomerName, 
            account.Document, 
            account.Email, 
            account.Status,
            AccountNumber = account.Id // Usando ID como número por enquanto
        });
    }

    [HttpGet("{id}/balance")]
    public async Task<IActionResult> GetBalance(Guid id)
    {
        var account = await _repository.GetByIdAsync(id, CancellationToken.None);
        
        if (account == null) return NotFound();
        
        // Retorna apenas o saldo
        return Ok(new { AccountId = id, AvailableAmount = account.Balance });
    }

    // NOTA ARQUITETURAL:
    // "GetStatement" e "PerformPix" foram removidos.
    // Essas funcionalidades devem ser chamadas na API de Payments:
    // GET http://localhost:5002/api/v1/transactions/{accountId}
    // POST http://localhost:5002/api/v1/transactions/pix
}
