using Microsoft.AspNetCore.Mvc;
using KRT.Payments.Application.UseCases;
using KRT.Payments.Application.DTOs;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/pix")]
public class PixController : ControllerBase
{
    private readonly PixTransferUseCase _useCase;

    public PixController(PixTransferUseCase useCase)
    {
        _useCase = useCase;
    }

    /// <summary>
    /// Executa uma transferencia Pix (Saga Orchestrator)
    /// </summary>
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] PixTransferRequest request)
    {
        var result = await _useCase.ExecuteAsync(request);

        return result.Status switch
        {
            "Completed" => Ok(result),
            "Failed" or "Compensated" => UnprocessableEntity(result),
            _ => Accepted(result)
        };
    }

    /// <summary>
    /// Consulta historico de transacoes Pix de uma conta
    /// </summary>
    [HttpGet("history/{accountId}")]
    public async Task<IActionResult> History(Guid accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _useCase.GetHistoryAsync(accountId, page, pageSize);
        return Ok(result);
    }
}
