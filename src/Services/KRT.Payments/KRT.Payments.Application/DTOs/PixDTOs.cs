namespace KRT.Payments.Application.DTOs;
public record PixRequest(Guid AccountId, string Key, decimal Amount);
public record PixResponse(Guid PaymentId, string Status);
