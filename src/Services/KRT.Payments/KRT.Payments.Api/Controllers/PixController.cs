using Microsoft.AspNetCore.Mvc;
using KRT.Payments.Application.UseCases;
using KRT.Payments.Application.DTOs;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PixController : ControllerBase
{
    private readonly PixUseCase _useCase;

    public PixController(PixUseCase useCase)
    {
        _useCase = useCase;
    }

    [HttpPost]
    public async Task<IActionResult> SendPix([FromBody] PixRequest request)
    {
        var result = await _useCase.Handle(request);
        return Ok(result);
    }
}
