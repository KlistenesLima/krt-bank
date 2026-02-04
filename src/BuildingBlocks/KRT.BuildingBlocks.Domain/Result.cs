namespace KRT.BuildingBlocks.Domain;

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public string ErrorCode { get; } // Adicionado
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess, string error, string errorCode = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode ?? "Error";
    }

    public static Result Fail(string message, string errorCode = "GeneralError") => new Result(false, message, errorCode);
    public static Result<T> Fail<T>(string message, string errorCode = "GeneralError") => new Result<T>(default, false, message, errorCode);
    
    public static Result Ok() => new Result(true, string.Empty);
    public static Result<T> Ok<T>(T value) => new Result<T>(value, true, string.Empty);
}

public class Result<T> : Result
{
    public T Value { get; }
    
    protected internal Result(T value, bool isSuccess, string error, string errorCode = null) 
        : base(isSuccess, error, errorCode)
    {
        Value = value;
    }
}
