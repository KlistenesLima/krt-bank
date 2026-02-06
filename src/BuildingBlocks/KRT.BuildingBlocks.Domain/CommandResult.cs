namespace KRT.BuildingBlocks.Domain;

public class CommandResult
{
    public bool IsValid { get; private set; }
    public Guid Id { get; private set; }
    public List<string> Errors { get; private set; }

    // Construtor privado
    private CommandResult() 
    { 
        Errors = new List<string>(); 
    }

    // 1. Sucesso com ID (Create)
    public static CommandResult Success(Guid id) 
    {
        return new CommandResult 
        { 
            IsValid = true, 
            Id = id 
        };
    }

    // 2. Sucesso Void (Update/Delete/Transactions)
    public static CommandResult Success() 
    {
        return new CommandResult 
        { 
            IsValid = true 
        };
    }

    // 3. Falha Simples
    public static CommandResult Failure(string error) 
    {
        return new CommandResult 
        { 
            IsValid = false, 
            Errors = new List<string> { error } 
        };
    }

    // 4. Falha Múltipla
    public static CommandResult Failure(List<string> errors) 
    {
        return new CommandResult 
        { 
            IsValid = false, 
            Errors = errors 
        };
    }
}
