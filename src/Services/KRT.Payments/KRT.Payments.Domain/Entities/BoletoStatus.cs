namespace KRT.Payments.Domain.Entities;

public enum BoletoStatus
{
    Pending,
    Processing,
    Paid,
    Overdue,
    Cancelled
}