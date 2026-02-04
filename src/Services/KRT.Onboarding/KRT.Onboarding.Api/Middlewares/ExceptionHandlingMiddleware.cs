using System.Net;
using System.Text.Json;
using KRT.BuildingBlocks.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace KRT.Onboarding.Api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();

        _logger.LogError(exception, "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);

        // AQUI ESTA A CORREÇÃO DA LÓGICA DO SWITCH
        // Classes mais específicas (filhas) devem vir ANTES da classe genérica (pai)
        var (statusCode, problemDetails) = exception switch
        {
            // 1. Filhas de DomainException
            EntityNotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                CreateProblemDetails("Not Found", notFoundEx.Message, (int)HttpStatusCode.NotFound, notFoundEx.Code, correlationId)),

            BusinessRuleException businessEx => (
                HttpStatusCode.UnprocessableEntity,
                CreateProblemDetails("Business Rule Violation", businessEx.Message, (int)HttpStatusCode.UnprocessableEntity, businessEx.Code, correlationId)),

            ConcurrencyException concurrencyEx => (
                HttpStatusCode.Conflict,
                CreateProblemDetails("Concurrency Conflict", concurrencyEx.Message, (int)HttpStatusCode.Conflict, concurrencyEx.Code, correlationId)),

            // 2. Pai (DomainException) - Captura qualquer outra falha de domínio não listada acima
            DomainException domainEx => (
                HttpStatusCode.BadRequest,
                CreateProblemDetails("Domain Error", domainEx.Message, (int)HttpStatusCode.BadRequest, domainEx.Code, correlationId)),

            // 3. Genérico (Exception)
            _ => (
                HttpStatusCode.InternalServerError,
                CreateProblemDetails("Internal Server Error", "An unexpected error occurred.", (int)HttpStatusCode.InternalServerError, "INTERNAL_ERROR", correlationId))
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static ProblemDetails CreateProblemDetails(string title, string detail, int status, string errorCode, string correlationId)
    {
        var problem = new ProblemDetails
        {
            Title = title,
            Detail = detail,
            Status = status
        };
        
        if (!problem.Extensions.ContainsKey("errorCode"))
            problem.Extensions.Add("errorCode", errorCode);
            
        if (!problem.Extensions.ContainsKey("correlationId"))
            problem.Extensions.Add("correlationId", correlationId);
        
        return problem;
    }
}
