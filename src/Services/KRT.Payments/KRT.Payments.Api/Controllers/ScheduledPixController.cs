using KRT.Payments.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/pix/scheduled")]
public class ScheduledPixController : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, ScheduledPix> _store = new();

    /// <summary>
    /// Lista agendamentos de uma conta.
    /// </summary>
    [HttpGet("account/{accountId}")]
    [AllowAnonymous]
    public IActionResult GetByAccount(Guid accountId, [FromQuery] string? status = null)
    {
        var items = _store.Values
            .Where(s => s.AccountId == accountId)
            .AsEnumerable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<ScheduledPixStatus>(status, true, out var st))
            items = items.Where(s => s.Status == st);

        return Ok(items.OrderByDescending(s => s.ScheduledDate).Select(s => new
        {
            s.Id, s.AccountId, s.DestinationAccountId, s.PixKey, s.DestinationName,
            s.Amount, s.Description, s.ScheduledDate,
            frequency = s.GetFrequencyLabel(),
            frequencyCode = s.Frequency.ToString(),
            s.IsRecurring, s.EndDate, s.MaxExecutions, s.ExecutionCount,
            status = s.GetStatusLabel(),
            statusCode = s.Status.ToString(),
            s.NextExecutionDate, s.LastExecutedAt, s.LastError, s.CreatedAt
        }));
    }

    /// <summary>
    /// Cria agendamento Pix (unico ou recorrente).
    /// </summary>
    [HttpPost]
    [AllowAnonymous]
    public IActionResult Create([FromBody] CreateScheduledPixRequest request)
    {
        try
        {
            var freq = Enum.TryParse<ScheduledPixFrequency>(request.Frequency, true, out var f) ? f : ScheduledPixFrequency.Once;

            var scheduled = ScheduledPix.Create(
                request.AccountId,
                request.DestinationAccountId,
                request.PixKey,
                request.DestinationName ?? "Favorecido",
                request.Amount,
                request.Description ?? "",
                request.ScheduledDate,
                freq,
                request.EndDate,
                request.MaxExecutions);

            _store[scheduled.Id] = scheduled;

            return Created($"/api/v1/pix/scheduled/{scheduled.Id}", new
            {
                scheduled.Id,
                message = scheduled.IsRecurring
                    ? $"Pix recorrente ({scheduled.GetFrequencyLabel()}) agendado com sucesso"
                    : "Pix agendado com sucesso",
                scheduled.ScheduledDate,
                scheduled.NextExecutionDate,
                frequency = scheduled.GetFrequencyLabel(),
                scheduled.IsRecurring
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Consulta um agendamento.
    /// </summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public IActionResult GetById(Guid id)
    {
        if (!_store.TryGetValue(id, out var s))
            return NotFound(new { error = "Agendamento nao encontrado" });

        return Ok(new
        {
            s.Id, s.AccountId, s.DestinationAccountId, s.PixKey, s.DestinationName,
            s.Amount, s.Description, s.ScheduledDate,
            frequency = s.GetFrequencyLabel(),
            s.IsRecurring, s.EndDate, s.MaxExecutions, s.ExecutionCount,
            status = s.GetStatusLabel(),
            s.NextExecutionDate, s.LastExecutedAt, s.LastError, s.CreatedAt, s.CancelledAt
        });
    }

    /// <summary>
    /// Executa manualmente um agendamento (simula o job).
    /// </summary>
    [HttpPost("{id}/execute")]
    [AllowAnonymous]
    public IActionResult Execute(Guid id)
    {
        if (!_store.TryGetValue(id, out var s))
            return NotFound(new { error = "Agendamento nao encontrado" });

        var (success, message) = s.Execute();
        return success
            ? Ok(new { message, s.ExecutionCount, s.NextExecutionDate, status = s.GetStatusLabel() })
            : BadRequest(new { error = message });
    }

    /// <summary>
    /// Cancela um agendamento.
    /// </summary>
    [HttpPost("{id}/cancel")]
    [AllowAnonymous]
    public IActionResult Cancel(Guid id)
    {
        if (!_store.TryGetValue(id, out var s))
            return NotFound(new { error = "Agendamento nao encontrado" });

        s.Cancel();
        return Ok(new { message = "Agendamento cancelado", status = s.GetStatusLabel() });
    }

    /// <summary>
    /// Pausa um agendamento recorrente.
    /// </summary>
    [HttpPost("{id}/pause")]
    [AllowAnonymous]
    public IActionResult Pause(Guid id)
    {
        if (!_store.TryGetValue(id, out var s))
            return NotFound(new { error = "Agendamento nao encontrado" });

        try
        {
            s.Pause();
            return Ok(new { message = "Agendamento pausado", status = s.GetStatusLabel() });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Retoma um agendamento pausado.
    /// </summary>
    [HttpPost("{id}/resume")]
    [AllowAnonymous]
    public IActionResult Resume(Guid id)
    {
        if (!_store.TryGetValue(id, out var s))
            return NotFound(new { error = "Agendamento nao encontrado" });

        try
        {
            s.Resume();
            return Ok(new { message = "Agendamento retomado", s.NextExecutionDate, status = s.GetStatusLabel() });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza valor de um agendamento.
    /// </summary>
    [HttpPut("{id}/amount")]
    [AllowAnonymous]
    public IActionResult UpdateAmount(Guid id, [FromBody] UpdateAmountRequest request)
    {
        if (!_store.TryGetValue(id, out var s))
            return NotFound(new { error = "Agendamento nao encontrado" });

        try
        {
            s.UpdateAmount(request.Amount);
            return Ok(new { message = "Valor atualizado", s.Amount });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public record CreateScheduledPixRequest(
    Guid AccountId,
    Guid DestinationAccountId,
    string PixKey,
    string? DestinationName,
    decimal Amount,
    string? Description,
    DateTime ScheduledDate,
    string Frequency = "Once",
    DateTime? EndDate = null,
    int? MaxExecutions = null);

public record UpdateAmountRequest(decimal Amount);