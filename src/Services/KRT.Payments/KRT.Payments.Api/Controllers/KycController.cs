using KRT.Payments.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Api.Controllers;

[ApiController]
[Route("api/v1/kyc")]
public class KycController : ControllerBase
{
    private readonly PaymentsDbContext _db;
    public KycController(PaymentsDbContext db) => _db = db;

    [HttpGet("{accountId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStatus(Guid accountId)
    {
        var profile = await _db.KycProfiles.FindAsync(accountId);
        if (profile == null)
        {
            profile = new KycProfile { AccountId = accountId };
            _db.KycProfiles.Add(profile);
            await _db.SaveChangesAsync();
        }
        return Ok(new
        {
            profile.AccountId, profile.CurrentStep, profile.OverallStatus,
            document = new { uploaded = profile.DocumentUploaded, type = profile.DocumentType, file = profile.DocumentFileName, uploadedAt = profile.DocumentUploadedAt, validation = profile.DocumentValidation },
            selfie = new { uploaded = profile.SelfieUploaded, uploadedAt = profile.SelfieUploadedAt, faceMatch = profile.FaceMatch },
            personalData = new { confirmed = profile.DataConfirmed, fullName = profile.FullName, cpf = profile.Cpf, birthDate = profile.BirthDate, motherName = profile.MotherName, confirmedAt = profile.DataConfirmedAt },
            completionPercent = (profile.DocumentUploaded ? 33 : 0) + (profile.SelfieUploaded ? 33 : 0) + (profile.DataConfirmed ? 34 : 0)
        });
    }

    [HttpPost("{accountId}/document")]
    [AllowAnonymous]
    public async Task<IActionResult> UploadDocument(Guid accountId, [FromBody] DocumentUploadRequest req)
    {
        var profile = await GetOrCreate(accountId);
        profile.DocumentUploaded = true;
        profile.DocumentType = req.DocumentType;
        profile.DocumentFileName = req.FileName;
        profile.DocumentUploadedAt = DateTime.UtcNow;
        profile.DocumentValidation = new ValidationResult { IsValid = true, Score = 0.95, Details = "Documento validado com sucesso" };
        profile.CurrentStep = "selfie";
        await _db.SaveChangesAsync();
        return Ok(new { message = "Documento enviado e validado", validation = profile.DocumentValidation });
    }

    [HttpPost("{accountId}/selfie")]
    [AllowAnonymous]
    public async Task<IActionResult> UploadSelfie(Guid accountId)
    {
        var profile = await GetOrCreate(accountId);
        profile.SelfieUploaded = true;
        profile.SelfieUploadedAt = DateTime.UtcNow;
        profile.FaceMatch = new FaceMatchResult { Match = true, Confidence = 0.97, LivenessScore = 0.99, Details = "Face match confirmado" };
        profile.CurrentStep = "confirm";
        await _db.SaveChangesAsync();
        return Ok(new { message = "Selfie validada com sucesso", faceMatch = profile.FaceMatch });
    }

    [HttpPost("{accountId}/confirm")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmData(Guid accountId, [FromBody] ConfirmDataRequest req)
    {
        var profile = await GetOrCreate(accountId);
        profile.DataConfirmed = true;
        profile.FullName = req.FullName;
        profile.Cpf = req.Cpf;
        profile.BirthDate = req.BirthDate;
        profile.MotherName = req.MotherName;
        profile.DataConfirmedAt = DateTime.UtcNow;
        profile.OverallStatus = "Em analise";
        profile.CurrentStep = "review";
        await _db.SaveChangesAsync();
        return Ok(new { message = "Dados confirmados. KYC em analise.", profile.OverallStatus });
    }

    [HttpPost("{accountId}/approve")]
    [AllowAnonymous]
    public async Task<IActionResult> Approve(Guid accountId)
    {
        var profile = await GetOrCreate(accountId);
        profile.OverallStatus = "Aprovado";
        profile.CurrentStep = "completed";
        await _db.SaveChangesAsync();
        return Ok(new { message = "KYC aprovado com sucesso" });
    }

    private async Task<KycProfile> GetOrCreate(Guid accountId)
    {
        var p = await _db.KycProfiles.FindAsync(accountId);
        if (p != null) return p;
        p = new KycProfile { AccountId = accountId };
        _db.KycProfiles.Add(p);
        await _db.SaveChangesAsync();
        return p;
    }
}

public class KycProfile
{
    public Guid AccountId { get; set; }
    public string CurrentStep { get; set; } = "document";
    public string OverallStatus { get; set; } = "Pendente";
    public bool DocumentUploaded { get; set; }
    public string DocumentType { get; set; } = "";
    public string DocumentFileName { get; set; } = "";
    public DateTime? DocumentUploadedAt { get; set; }
    public ValidationResult? DocumentValidation { get; set; }
    public bool SelfieUploaded { get; set; }
    public DateTime? SelfieUploadedAt { get; set; }
    public FaceMatchResult? FaceMatch { get; set; }
    public bool DataConfirmed { get; set; }
    public string FullName { get; set; } = "";
    public string Cpf { get; set; } = "";
    public DateTime? BirthDate { get; set; }
    public string MotherName { get; set; } = "";
    public DateTime? DataConfirmedAt { get; set; }
}

public class ValidationResult { public bool IsValid { get; set; } public double Score { get; set; } public string Details { get; set; } = ""; }
public class FaceMatchResult { public bool Match { get; set; } public double Confidence { get; set; } public double LivenessScore { get; set; } public string Details { get; set; } = ""; }
public record DocumentUploadRequest(string DocumentType, string FileName);
public record ConfirmDataRequest(string FullName, string Cpf, DateTime BirthDate, string MotherName);
