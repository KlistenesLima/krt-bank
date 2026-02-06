namespace KRT.Payments.Domain.Enums;

public enum PixTransactionStatus
{
    Pending = 0,
    SourceDebited = 1,
    Completed = 2,
    Failed = 3,
    Compensated = 4
}
