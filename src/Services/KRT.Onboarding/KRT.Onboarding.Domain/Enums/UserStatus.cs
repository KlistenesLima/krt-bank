namespace KRT.Onboarding.Domain.Enums;

public enum UserStatus
{
    PendingEmailConfirmation = 0,
    PendingApproval = 1,
    Active = 2,
    Inactive = 3,
    Rejected = 4
}
