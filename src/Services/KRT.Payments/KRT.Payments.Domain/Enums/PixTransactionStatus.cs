namespace KRT.Payments.Domain.Enums;

public enum PixTransactionStatus
{
    // Saga states
    Pending = 0,
    SourceDebited = 1,
    Completed = 2,
    Failed = 3,
    Compensated = 4,

    // Fraud analysis states
    PendingAnalysis = 10,
    Approved = 11,
    Rejected = 12,
    UnderReview = 13
}
