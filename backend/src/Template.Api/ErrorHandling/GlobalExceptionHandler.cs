using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Template.Domain.Common.Exceptions;

namespace Template.Api.ErrorHandling;

// Converte as exceções conhecidas em ProblemDetails (ADR 0006).
// As demais seguem para o handler padrão, que responde 500 sem expor detalhes.
// Requisição abortada pelo cliente não chega aqui: o UseExceptionHandler já responde 499 e só loga em Debug.
public class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = ParaProblemDetails(exception);
        if (problem is null)
            return false;

        httpContext.Response.StatusCode = problem.Status!.Value;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }

    public static ProblemDetails? ParaProblemDetails(Exception exception) => exception switch
    {
        ValidationException e => new ValidationProblemDetails(ErrosPorCampo(e))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Dados inválidos."
        },
        NotFoundException e => new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Recurso não encontrado.",
            Detail = e.Message
        },
        ConflictException e => new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflito.",
            Detail = e.Message
        },
        _ => null
    };

    // Campos em camelCase, iguais aos do JSON, para o frontend associar cada erro ao seu campo
    private static Dictionary<string, string[]> ErrosPorCampo(ValidationException exception)
        => exception.Errors
            .GroupBy(x => JsonNamingPolicy.CamelCase.ConvertName(x.PropertyName))
            .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
}
