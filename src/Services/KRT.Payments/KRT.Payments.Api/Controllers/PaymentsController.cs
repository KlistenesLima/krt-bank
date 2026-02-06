using Microsoft.AspNetCore.Mvc;
using KRT.Payments.Domain.Entities;

namespace KRT.Payments.Api.Controllers;

public record CreatePaymentRequest(Guid AccountId, decimal Amount, string ReceiverKey);

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(ILogger<PaymentsController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
    {
        _logger.LogInformation(">>> [CONTROLLER] Recebendo requisicao de pagamento. Processando... (Se ver isso 2x para a mesma chave, a Idempotencia FALHOU)");
        
        // Simula processamento pesado (Banco de dados, Gateway, etc)
        await Task.Delay(100);

        var payment = new Payment(request.AccountId, request.ReceiverKey, request.Amount);
        
        _logger.LogInformation(">>> [CONTROLLER] Pagamento processado: {PaymentId}", payment.Id);

        return Ok(new { 
            PaymentId = payment.Id, 
            Status = "Processed", 
            Timestamp = DateTime.UtcNow 
        });
    }
}
