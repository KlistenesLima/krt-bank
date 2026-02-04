namespace KRT.BuildingBlocks.Domain.Exceptions;

public abstract class DomainException : Exception
{
    public string Code { get; }
    protected DomainException(string message, string code = "DomainError") : base(message)
    {
        Code = code;
    }
}

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string message, string code = "NotFound") : base(message, code) { }
}

public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message, string code = "BusinessRule") : base(message, code) { }
}

public class ConcurrencyException : DomainException
{
    public ConcurrencyException(string message, string code = "Concurrency") : base(message, code) { }
}
