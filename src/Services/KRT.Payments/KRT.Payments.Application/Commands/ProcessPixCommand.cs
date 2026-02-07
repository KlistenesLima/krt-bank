using MediatR;
using KRT.BuildingBlocks.Domain;

namespace KRT.Payments.Application.Commands;

public class ProcessPixCommand : IRequest<CommandResult>
{
    public Guid SourceAccountId { get; set; }
    public Guid DestinationAccountId { get; set; }
    public decimal Amount { get; set; }
    public string PixKey { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid IdempotencyKey { get; set; } = Guid.NewGuid();
}
