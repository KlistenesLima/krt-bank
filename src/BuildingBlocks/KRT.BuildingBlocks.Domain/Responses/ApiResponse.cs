namespace KRT.BuildingBlocks.Domain.Responses;

/// <summary>
/// Envelope padrão para todas as respostas da API.
/// Garante consistência entre Onboarding e Payments.
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? CorrelationId { get; set; }

    public static ApiResponse<T> Ok(T data, string? correlationId = null)
        => new() { Success = true, Data = data, CorrelationId = correlationId };

    public static ApiResponse<T> Fail(string error, string? correlationId = null)
        => new() { Success = false, Errors = new List<string> { error }, CorrelationId = correlationId };

    public static ApiResponse<T> Fail(List<string> errors, string? correlationId = null)
        => new() { Success = false, Errors = errors, CorrelationId = correlationId };
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string? correlationId = null)
        => new() { Success = true, CorrelationId = correlationId };

    public new static ApiResponse Fail(string error, string? correlationId = null)
        => new() { Success = false, Errors = new List<string> { error }, CorrelationId = correlationId };
}

/// <summary>
/// Envelope paginado para listagens.
/// </summary>
public class PagedResponse<T>
{
    public bool Success { get; set; } = true;
    public List<T> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
