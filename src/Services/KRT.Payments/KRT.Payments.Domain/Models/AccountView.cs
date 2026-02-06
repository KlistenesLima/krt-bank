namespace KRT.Payments.Domain.Models;

public class AccountView
{
    public Guid Id { get; set; }
    public decimal Balance { get; set; }

    public AccountView(Guid id, decimal balance)
    {
        Id = id;
        Balance = balance;
    }
}
