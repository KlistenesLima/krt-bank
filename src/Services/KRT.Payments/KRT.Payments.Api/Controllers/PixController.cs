using KRT.Payments.Api.Services;
using KRT.Payments.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KRT.Payments.Application.Commands;
using KRT.Payments.Domain.Interfaces;
using MediatR;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/pix")]
[Authorize]
public class PixController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPixTransactionRepository _repository;
    private readonly QrCodeService _qrCodeService;
    private readonly PdfReceiptService _pdfService;

    public PixController(
        IMediator mediator,
        IPixTransactionRepository repository,
        QrCodeService qrCodeService,
        PdfReceiptService pdfService)
    {
        _mediator = mediator;
        _repository = repository;
        _qrCodeService = qrCodeService;
        _pdfService = pdfService;
    }

    /// <summary>
    /// Inicia uma transferencia Pix. A transacao e criada e entra na fila
    /// de analise anti-fraude assincrona.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> ProcessPix([FromBody] PixTransferRequest request)
    {
        var command = new ProcessPixCommand
        {
            SourceAccountId = request.SourceAccountId,
            DestinationAccountId = request.DestinationAccountId,
            Amount = request.Amount,
            PixKey = request.PixKey,
            Description = request.Description ?? "",
            IdempotencyKey = request.IdempotencyKey
        };

        var result = await _mediator.Send(command);

        if (!result.IsValid)
            return BadRequest(new { success = false, error = result.Errors.FirstOrDefault() });

        return Accepted(new
        {
            success = true,
            transactionId = result.Id,
            status = "PendingAnalysis",
            message = "Transacao recebida. Analise anti-fraude em andamento. Consulte GET /api/v1/pix/{id} para acompanhar."
        });
    }

    /// <summary>
    /// Consulta o status de uma transacao Pix (inclui resultado da analise de fraude).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetStatus(Guid id)
    {
        var tx = await _repository.GetByIdAsync(id);
        if (tx == null) return NotFound(new { error = "Transacao nao encontrada" });

        return Ok(new
        {
            transactionId = tx.Id,
            sourceAccountId = tx.SourceAccountId,
            destinationAccountId = tx.DestinationAccountId,
            amount = tx.Amount,
            currency = tx.Currency,
            pixKey = tx.PixKey,
            status = tx.Status.ToString(),
            description = tx.Description,
            failureReason = tx.FailureReason,
            createdAt = tx.CreatedAt,
            completedAt = tx.CompletedAt,
            fraud = new
            {
                score = tx.FraudScore,
                details = tx.FraudDetails,
                analyzedAt = tx.FraudAnalyzedAt
            }
        });
    }

    /// <summary>
    /// Lista transacoes de uma conta (extrato Pix).
    /// </summary>
    [HttpGet("account/{accountId:guid}")]
    public async Task<IActionResult> GetByAccount(Guid accountId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var txs = await _repository.GetByAccountIdAsync(accountId, page, pageSize);
        return Ok(txs.Select(tx => new
        {
            transactionId = tx.Id,
            sourceAccountId = tx.SourceAccountId,
            destinationAccountId = tx.DestinationAccountId,
            amount = tx.Amount,
            status = tx.Status.ToString(),
            fraudScore = tx.FraudScore,
            description = tx.Description,
            createdAt = tx.CreatedAt,
            completedAt = tx.CompletedAt
        }));
    }

    // ===== QR CODE PIX =====

    /// <summary>
    /// Gera QR Code Pix para receber pagamento.
    /// </summary>
    [HttpPost("qrcode/generate")]
    [AllowAnonymous]
    public IActionResult GenerateQrCode([FromBody] GenerateQrCodeRequest request)
    {
        var payload = _qrCodeService.GeneratePixPayload(
            request.PixKey,
            request.MerchantName ?? "KRT Bank",
            request.City ?? "Sao Paulo",
            request.Amount,
            request.TxId ?? Guid.NewGuid().ToString("N")[..25]);

        var base64 = _qrCodeService.GenerateQrCodeBase64(payload);

        return Ok(new
        {
            payload,
            qrCodeBase64 = base64,
            qrCodeDataUrl = $"data:image/png;base64,{base64}"
        });
    }

    /// <summary>
    /// Gera QR Code como imagem PNG.
    /// </summary>
    [HttpPost("qrcode/image")]
    [AllowAnonymous]
    public IActionResult GenerateQrCodeImage([FromBody] GenerateQrCodeRequest request)
    {
        var payload = _qrCodeService.GeneratePixPayload(
            request.PixKey,
            request.MerchantName ?? "KRT Bank",
            request.City ?? "Sao Paulo",
            request.Amount,
            request.TxId ?? Guid.NewGuid().ToString("N")[..25]);

        var bytes = _qrCodeService.GenerateQrCodeBytes(payload);
        return File(bytes, "image/png", "pix-qrcode.png");
    }

    // ===== COMPROVANTE PDF =====

    /// <summary>
    /// Gera comprovante PDF de uma transacao Pix.
    /// </summary>
    [HttpGet("receipt/{transactionId}")]
    [Authorize]
    public async Task<IActionResult> GenerateReceipt(Guid transactionId, CancellationToken ct)
    {
        var tx = await _repository.GetByIdAsync(transactionId, ct);
        if (tx is null)
            return NotFound(new { error = "Transacao nao encontrada." });

        var qrPayload = $"PIX-RECEIPT-{transactionId}";
        var qrBytes = _qrCodeService.GenerateQrCodeBytes(qrPayload);

        // Extrair nomes da descricao (formato: "Pix de X para Y" ou "PIX de X para Y")
        var sourceName = tx.SourceAccountId.ToString()[..8];
        var destName = tx.DestinationAccountId.ToString()[..8];
        if (!string.IsNullOrEmpty(tx.Description))
        {
            var parts = tx.Description.Split(" para ", StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                sourceName = parts[0].Replace("Pix de ", "").Replace("PIX de ", "").Replace("Pix ", "").Trim();
                destName = parts[1].Trim();
            }
        }

        var statusText = tx.Status.ToString() switch
        {
            "Completed" => "Concluido",
            "Failed" => "Falhou",
            "UnderReview" => "Em analise",
            "Pending" => "Pendente",
            "Reversed" => "Estornado",
            _ => tx.Status.ToString()
        };

        var data = new PixReceiptData
        {
            TransactionId = transactionId.ToString(),
            SourceName = sourceName,
            SourceDocument = "",
            DestinationName = destName,
            DestinationDocument = tx.PixKey,
            Amount = tx.Amount,
            Status = statusText,
            Timestamp = tx.CompletedAt ?? tx.CreatedAt,
            QrCodeBytes = qrBytes
        };

        var pdf = _pdfService.GeneratePixReceipt(data);
        return File(pdf, "application/pdf", $"comprovante-pix-{transactionId:N}.pdf");
    }
        }

        var statusText = tx.Status.ToString() switch
        {
            "Completed" => "Concluido",
            "Failed" => "Falhou",
            "UnderReview" => "Em analise",
            "Pending" => "Pendente",
            "Reversed" => "Estornado",
            _ => tx.Status.ToString()
        };

        var data = new PixReceiptData
        {
            TransactionId = transactionId.ToString(),
            SourceName = sourceName,
            SourceDocument = "",
            DestinationName = destName,
            DestinationDocument = tx.PixKey,
            Amount = tx.Amount,
            Status = statusText,
            Timestamp = tx.CompletedAt ?? tx.CreatedAt,
            QrCodeBytes = qrBytes
        };

        var pdf = _pdfService.GeneratePixReceipt(data);
        return File(pdf, "application/pdf", $"comprovante-pix-{transactionId:N}.pdf");
    }

    /// <summary>
    /// Gera comprovante PDF com dados fornecidos.
    /// </summary>
    [HttpPost("receipt")]
    [AllowAnonymous]
    public IActionResult GenerateReceiptFromData([FromBody] PixReceiptRequest request)
    {
        var qrPayload = $"PIX-{request.TransactionId}";
        var qrBytes = _qrCodeService.GenerateQrCodeBytes(qrPayload);

        var data = new PixReceiptData
        {
            TransactionId = request.TransactionId,
            SourceName = request.SourceName,
            SourceDocument = request.SourceDocument,
            DestinationName = request.DestinationName,
            DestinationDocument = request.DestinationDocument,
            Amount = request.Amount,
            Status = request.Status ?? "Completed",
            Timestamp = request.Timestamp ?? DateTime.UtcNow,
            QrCodeBytes = qrBytes
        };

        var pdf = _pdfService.GeneratePixReceipt(data);
        return File(pdf, "application/pdf", $"comprovante-pix-{request.TransactionId}.pdf");
    }

    // ===== LIMITES PIX =====

    /// <summary>
    /// Consulta limites Pix de uma conta.
    /// </summary>
    [HttpGet("limits/{accountId}")]
    [AllowAnonymous]
    public IActionResult GetLimits(Guid accountId)
    {
        var limits = PixLimit.CreateDefault(accountId);
        var now = DateTime.UtcNow;
        var period = now.Hour >= 6 && now.Hour < 20 ? "Diurno" : "Noturno";

        return Ok(new
        {
            accountId,
            currentPeriod = period,
            daytime = new
            {
                perTransaction = limits.DaytimePerTransaction,
                daily = limits.DaytimeDaily,
                usedToday = limits.DaytimeUsedToday,
                remaining = limits.DaytimeDaily - limits.DaytimeUsedToday
            },
            nighttime = new
            {
                perTransaction = limits.NighttimePerTransaction,
                daily = limits.NighttimeDaily,
                usedToday = limits.NighttimeUsedToday,
                remaining = limits.NighttimeDaily - limits.NighttimeUsedToday
            }
        });
    }

    /// <summary>
    /// Atualiza limites Pix de uma conta.
    /// </summary>
    [HttpPut("limits/{accountId}")]
    [AllowAnonymous]
    public IActionResult UpdateLimits(Guid accountId, [FromBody] UpdateLimitsRequest request)
    {
        var limits = PixLimit.CreateDefault(accountId);
        limits.UpdateLimits(
            request.DaytimePerTransaction,
            request.DaytimeDaily,
            request.NighttimePerTransaction,
            request.NighttimeDaily);

        return Ok(new
        {
            accountId,
            message = "Limites atualizados com sucesso",
            daytime = new { perTransaction = limits.DaytimePerTransaction, daily = limits.DaytimeDaily },
            nighttime = new { perTransaction = limits.NighttimePerTransaction, daily = limits.NighttimeDaily }
        });
    }
}

// === Request DTOs ===

public record PixTransferRequest(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    string PixKey,
    decimal Amount,
    string? Description,
    Guid IdempotencyKey);

public record GenerateQrCodeRequest(
    string PixKey,
    decimal Amount,
    string? MerchantName = null,
    string? City = null,
    string? TxId = null);

public record PixReceiptRequest(
    string TransactionId,
    string SourceName,
    string SourceDocument,
    string DestinationName,
    string DestinationDocument,
    decimal Amount,
    string? Status = null,
    DateTime? Timestamp = null);

public record UpdateLimitsRequest(
    decimal? DaytimePerTransaction,
    decimal? DaytimeDaily,
    decimal? NighttimePerTransaction,
    decimal? NighttimeDaily);
