using System.Security.Claims;
using KRT.Onboarding.Application.Commands.Users;
using KRT.Onboarding.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KRT.Onboarding.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
[Authorize(Roles = "Administrador")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetAdminId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }

    /// <summary>
    /// Lista todos os usuários.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _mediator.Send(new GetAllUsersQuery());
        return Ok(new { success = true, data = users });
    }

    /// <summary>
    /// Lista usuários pendentes de aprovação.
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending()
    {
        var users = await _mediator.Send(new GetPendingUsersQuery());
        return Ok(new { success = true, data = users });
    }

    /// <summary>
    /// Busca usuário por ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _mediator.Send(new GetUserByIdQuery(id));
        if (user == null)
            return NotFound(new { success = false, message = "Usuário não encontrado" });

        return Ok(new { success = true, data = user });
    }

    /// <summary>
    /// Aprovar usuário pendente.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var result = await _mediator.Send(new ApproveUserCommand(id, GetAdminId()));

        if (!result.Success)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Rejeitar usuário pendente.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var result = await _mediator.Send(new RejectUserCommand(id, GetAdminId()));

        if (!result.Success)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Alterar role do usuário.
    /// </summary>
    [HttpPut("{id:guid}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeRoleRequest request)
    {
        var result = await _mediator.Send(new ChangeUserRoleCommand(id, request.NewRole, GetAdminId()));

        if (!result.Success)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Ativar ou desativar usuário.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request)
    {
        var result = await _mediator.Send(new ChangeUserStatusCommand(id, request.Activate, GetAdminId()));

        if (!result.Success)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }
}

public record ChangeRoleRequest(UserRole NewRole);
public record ChangeStatusRequest(bool Activate);
