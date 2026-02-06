using MediatR;
using KRT.BuildingBlocks.Domain;

namespace KRT.Onboarding.Application.Commands;

public record DebitAccountCommand(Guid AccountId, decimal Amount, string Reason) : IRequest<CommandResult>;
