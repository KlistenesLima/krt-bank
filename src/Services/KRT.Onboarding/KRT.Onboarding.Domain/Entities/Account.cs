using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Domain.Exceptions;

namespace KRT.Onboarding.Domain.Entities;

public class Account : Entity, IAggregateRoot
{
    public string CustomerName { get; private set; }
    public string Cpf { get; private set; }
    public string Email { get; private set; }
    public string AccountNumber { get; private set; } // Propriedade Restaurada
    public decimal Balance { get; private set; }

    // Propriedades Computadas para compatibilidade
    public string CustomerDocument => Cpf;
    public string CustomerEmail => Email;

    protected Account() { }

    public Account(string name, string cpf, string email)
    {
        Id = Guid.NewGuid();
        CustomerName = name;
        Cpf = cpf;
        Email = email;
        
        // Gera um número de conta aleatório (Simulação de Agência/Conta)
        AccountNumber = new Random().Next(10000, 99999).ToString() + "-9";
        
        Balance = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void Debit(decimal amount)
    {
        if (amount <= 0) throw new DomainException("Valor deve ser maior que zero");
        if (Balance < amount) throw new DomainException("Saldo insuficiente");
        Balance -= amount;
    }

    public void Credit(decimal amount)
    {
        if (amount <= 0) throw new DomainException("Valor deve ser maior que zero");
        Balance += amount;
    }
}
