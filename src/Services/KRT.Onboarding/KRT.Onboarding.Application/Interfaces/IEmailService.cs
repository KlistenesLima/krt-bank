namespace KRT.Onboarding.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationCode);
    Task SendRegistrationPendingAsync(string toEmail, string userName);
    Task SendApprovalNotificationAsync(string toEmail, string userName, string email, string document);
    Task SendRejectionNotificationAsync(string toEmail, string userName);
    Task SendPasswordResetAsync(string toEmail, string userName, string resetCode);
}
