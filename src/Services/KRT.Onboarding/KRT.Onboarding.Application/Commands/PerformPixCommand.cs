using MediatR;
namespace KRT.Onboarding.Application.Commands;
public class PerformPixCommand : IRequest<CommandResult>
{
    public Guid AccountId { get; set; }
    public string PixKey { get; set; }
    public decimal Amount { get; set; }
}
