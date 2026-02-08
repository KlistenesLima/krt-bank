using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/kyc")]
public class KycController : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, KycProfile> _store = new();

    [HttpGet("{accountId}")]
    [AllowAnonymous]
    public IActionResult GetStatus(Guid accountId)
    {
        var kyc = _store.GetOrAdd(accountId, _ => new KycProfile { AccountId = accountId });
        return Ok(kyc);
    }

    /// <summary>
    /// Upload de documento (RG, CNH, Passaporte) — aceita base64.
    /// </summary>
    [HttpPost("{accountId}/document")]
    [AllowAnonymous]
    public IActionResult UploadDocument(Guid accountId, [FromBody] DocumentUploadRequest req)
    {
        var kyc = _store.GetOrAdd(accountId, _ => new KycProfile { AccountId = accountId });

        if (string.IsNullOrEmpty(req.DocumentType) || string.IsNullOrEmpty(req.Base64Data))
            return BadRequest(new { error = "Tipo de documento e imagem sao obrigatorios" });

        kyc.DocumentType = req.DocumentType;
        kyc.DocumentUploaded = true;
        kyc.DocumentUploadedAt = DateTime.UtcNow;
        kyc.DocumentFileName = req.FileName ?? $"doc_{accountId:N}.jpg";

        // Simulacao de validacao automatica
        var rng = new Random(accountId.GetHashCode());
        kyc.DocumentValidation = new ValidationResult
        {
            IsValid = rng.Next(100) > 10,  // 90% chance valido
            Confidence = Math.Round(85 + rng.NextDouble() * 15, 1),
            Details = "Documento legivel, dados conferem",
            ValidatedAt = DateTime.UtcNow
        };

        kyc.UpdateStep();
        return Ok(new { message = "Documento enviado e validado", kyc.DocumentValidation, kyc.CurrentStep });
    }

    /// <summary>
    /// Upload de selfie para prova de vida — aceita base64.
    /// </summary>
    [HttpPost("{accountId}/selfie")]
    [AllowAnonymous]
    public IActionResult UploadSelfie(Guid accountId, [FromBody] SelfieUploadRequest req)
    {
        var kyc = _store.GetOrAdd(accountId, _ => new KycProfile { AccountId = accountId });

        if (string.IsNullOrEmpty(req.Base64Data))
            return BadRequest(new { error = "Imagem da selfie e obrigatoria" });

        kyc.SelfieUploaded = true;
        kyc.SelfieUploadedAt = DateTime.UtcNow;

        // Simulacao face match
        var rng = new Random(accountId.GetHashCode() + 1);
        kyc.FaceMatch = new FaceMatchResult
        {
            IsMatch = rng.Next(100) > 5, // 95% chance match
            Confidence = Math.Round(88 + rng.NextDouble() * 12, 1),
            LivenessScore = Math.Round(90 + rng.NextDouble() * 10, 1),
            Details = "Face detectada, liveness confirmado",
            ValidatedAt = DateTime.UtcNow
        };

        kyc.UpdateStep();
        return Ok(new { message = "Selfie enviada e validada", kyc.FaceMatch, kyc.CurrentStep });
    }

    /// <summary>
    /// Confirma dados pessoais (etapa final).
    /// </summary>
    [HttpPost("{accountId}/confirm")]
    [AllowAnonymous]
    public IActionResult ConfirmData(Guid accountId, [FromBody] ConfirmDataRequest req)
    {
        var kyc = _store.GetOrAdd(accountId, _ => new KycProfile { AccountId = accountId });

        kyc.FullName = req.FullName;
        kyc.Cpf = req.Cpf;
        kyc.BirthDate = req.BirthDate;
        kyc.MotherName = req.MotherName;
        kyc.DataConfirmed = true;
        kyc.DataConfirmedAt = DateTime.UtcNow;
        kyc.UpdateStep();

        return Ok(new { message = "Dados confirmados. Conta em analise.", kyc.CurrentStep, kyc.OverallStatus });
    }

    /// <summary>
    /// Aprova/rejeita conta (simulacao admin).
    /// </summary>
    [HttpPost("{accountId}/approve")]
    [AllowAnonymous]
    public IActionResult Approve(Guid accountId, [FromBody] ApproveRequest req)
    {
        var kyc = _store.GetOrAdd(accountId, _ => new KycProfile { AccountId = accountId });
        kyc.OverallStatus = req.Approved ? "Aprovado" : "Rejeitado";
        kyc.ReviewedAt = DateTime.UtcNow;
        kyc.ReviewNotes = req.Notes;
        return Ok(new { message = $"Conta {kyc.OverallStatus.ToLower()}", kyc.OverallStatus });
    }
}

public class KycProfile
{
    public Guid AccountId { get; set; }
    public string CurrentStep { get; set; } = "document";
    public string OverallStatus { get; set; } = "Pendente";

    // Documento
    public bool DocumentUploaded { get; set; }
    public string DocumentType { get; set; } = "";
    public string DocumentFileName { get; set; } = "";
    public DateTime? DocumentUploadedAt { get; set; }
    public ValidationResult? DocumentValidation { get; set; }

    // Selfie
    public bool SelfieUploaded { get; set; }
    public DateTime? SelfieUploadedAt { get; set; }
    public FaceMatchResult? FaceMatch { get; set; }

    // Dados
    public bool DataConfirmed { get; set; }
    public string FullName { get; set; } = "";
    public string Cpf { get; set; } = "";
    public DateTime? BirthDate { get; set; }
    public string MotherName { get; set; } = "";
    public DateTime? DataConfirmedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }

    public void UpdateStep()
    {
        if (!DocumentUploaded) CurrentStep = "document";
        else if (!SelfieUploaded) CurrentStep = "selfie";
        else if (!DataConfirmed) CurrentStep = "confirm";
        else CurrentStep = "review";

        if (DataConfirmed && DocumentValidation?.IsValid == true && FaceMatch?.IsMatch == true)
            OverallStatus = "Em analise";
    }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public double Confidence { get; set; }
    public string Details { get; set; } = "";
    public DateTime ValidatedAt { get; set; }
}

public class FaceMatchResult
{
    public bool IsMatch { get; set; }
    public double Confidence { get; set; }
    public double LivenessScore { get; set; }
    public string Details { get; set; } = "";
    public DateTime ValidatedAt { get; set; }
}

public record DocumentUploadRequest(string DocumentType, string Base64Data, string? FileName);
public record SelfieUploadRequest(string Base64Data);
public record ConfirmDataRequest(string FullName, string Cpf, DateTime BirthDate, string MotherName);
public record ApproveRequest(bool Approved, string? Notes);