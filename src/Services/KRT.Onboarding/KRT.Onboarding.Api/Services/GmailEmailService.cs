using KRT.Onboarding.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace KRT.Onboarding.Api.Services;

public class GmailEmailService : IEmailService
{
    private readonly string _smtpHost = "smtp.gmail.com";
    private readonly int _smtpPort = 587;
    private readonly string _fromEmail;
    private readonly string _fromName;
    private readonly string _password;
    private readonly ILogger<GmailEmailService> _logger;

    public GmailEmailService(IConfiguration configuration, ILogger<GmailEmailService> logger)
    {
        _fromEmail = configuration["Email:FromAddress"] ?? "klisteneswar3@gmail.com";
        _fromName = configuration["Email:FromName"] ?? "KRT Bank";
        _password = configuration["Email:SmtpPassword"] ?? "";
        _logger = logger;
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationCode)
    {
        var subject = "KRT Bank — Confirme seu email";
        var body = BuildTemplate(userName,
            $@"<p style='font-size:16px;color:#333;'>Olá <strong>{userName}</strong>,</p>
               <p style='font-size:16px;color:#333;'>Seu código de verificação é:</p>
               <div style='text-align:center;margin:30px 0;'>
                 <span style='font-size:36px;font-weight:bold;letter-spacing:8px;color:#0047BB;background:#f0f4ff;padding:15px 30px;border-radius:8px;display:inline-block;'>{confirmationCode}</span>
               </div>
               <p style='font-size:14px;color:#666;text-align:center;'>Este código expira em <strong>30 minutos</strong>.</p>");
        await SendAsync(toEmail, subject, body);
    }

    public async Task SendRegistrationPendingAsync(string toEmail, string userName)
    {
        var subject = "KRT Bank — Cadastro solicitado";
        var body = BuildTemplate(userName,
            $@"<p style='font-size:16px;color:#333;'>Olá <strong>{userName}</strong>,</p>
               <p style='font-size:16px;color:#333;'>Seu cadastro foi recebido com sucesso!</p>
               <p style='font-size:16px;color:#333;'>Após aprovação pelo administrador, você poderá acessar o sistema com seu email/CPF e a senha cadastrada.</p>
               <p style='font-size:14px;color:#666;margin-top:20px;'>Você receberá um email assim que seu acesso for liberado.</p>");
        await SendAsync(toEmail, subject, body);
    }

    public async Task SendApprovalNotificationAsync(string toEmail, string userName, string email, string document)
    {
        var subject = "KRT Bank — Acesso aprovado!";
        var maskedDoc = document.Length >= 11
            ? $"{document[..3]}.***.**{document[^2..]}"
            : document;
        var body = BuildTemplate(userName,
            $@"<p style='font-size:16px;color:#333;'>Olá <strong>{userName}</strong>,</p>
               <p style='font-size:16px;color:#333;'>Seu acesso foi <span style='color:#28a745;font-weight:bold;'>aprovado</span>!</p>
               <p style='font-size:16px;color:#333;'>Agora você pode acessar o KRT Bank com:</p>
               <div style='background:#f0f4ff;padding:15px 20px;border-radius:8px;margin:15px 0;'>
                 <p style='margin:5px 0;color:#333;'><strong>Email:</strong> {email}</p>
                 <p style='margin:5px 0;color:#333;'><strong>CPF:</strong> {maskedDoc}</p>
                 <p style='margin:5px 0;color:#333;'>Use a senha cadastrada no registro.</p>
               </div>");
        await SendAsync(toEmail, subject, body);
    }

    public async Task SendRejectionNotificationAsync(string toEmail, string userName)
    {
        var subject = "KRT Bank — Cadastro não aprovado";
        var body = BuildTemplate(userName,
            $@"<p style='font-size:16px;color:#333;'>Olá <strong>{userName}</strong>,</p>
               <p style='font-size:16px;color:#333;'>Infelizmente seu cadastro <span style='color:#dc3545;font-weight:bold;'>não foi aprovado</span> neste momento.</p>
               <p style='font-size:14px;color:#666;'>Se você acredita que houve um engano, entre em contato com o suporte.</p>");
        await SendAsync(toEmail, subject, body);
    }

    public async Task SendPasswordResetAsync(string toEmail, string userName, string resetCode)
    {
        var subject = "KRT Bank — Recuperação de senha";
        var body = BuildTemplate(userName,
            $@"<p style='font-size:16px;color:#333;'>Olá <strong>{userName}</strong>,</p>
               <p style='font-size:16px;color:#333;'>Seu código de recuperação de senha é:</p>
               <div style='text-align:center;margin:30px 0;'>
                 <span style='font-size:36px;font-weight:bold;letter-spacing:8px;color:#0047BB;background:#f0f4ff;padding:15px 30px;border-radius:8px;display:inline-block;'>{resetCode}</span>
               </div>
               <p style='font-size:14px;color:#666;text-align:center;'>Este código expira em <strong>30 minutos</strong>.</p>
               <p style='font-size:14px;color:#666;text-align:center;'>Se você não solicitou a recuperação de senha, ignore este email.</p>");
        await SendAsync(toEmail, subject, body);
    }

    private static string BuildTemplate(string userName, string content)
    {
        return $@"<!DOCTYPE html>
<html>
<head><meta charset='UTF-8'></head>
<body style='margin:0;padding:0;font-family:Arial,Helvetica,sans-serif;background-color:#f5f5f5;'>
  <div style='max-width:600px;margin:0 auto;background:#ffffff;'>
    <div style='background:#0047BB;padding:30px;text-align:center;'>
      <h1 style='color:#ffffff;margin:0;font-size:28px;'>KRT Bank</h1>
      <p style='color:#ccd9f0;margin:5px 0 0;font-size:14px;'>Banco Digital Seguro</p>
    </div>
    <div style='padding:30px 40px;'>
      {content}
    </div>
    <div style='background:#f8f9fa;padding:20px 40px;text-align:center;border-top:1px solid #e9ecef;'>
      <p style='font-size:12px;color:#999;margin:0;'>KRT Bank — Este é um email automático, não responda.</p>
      <p style='font-size:12px;color:#999;margin:5px 0 0;'>&copy; 2026 KRT Bank. Todos os direitos reservados.</p>
    </div>
  </div>
</body>
</html>";
    }

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        if (string.IsNullOrEmpty(_password))
        {
            _logger.LogWarning("[Email] SMTP password not configured. Skipping email to {Email}: {Subject}", toEmail, subject);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_fromEmail, _password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("[Email] Sent '{Subject}' to {Email}", subject, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Email] Failed to send '{Subject}' to {Email}", subject, toEmail);
        }
    }
}
