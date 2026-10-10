using AwesomeAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Template.Api.ErrorHandling;
using Template.Domain.Common.Exceptions;

namespace Template.UnitTests.Api.ErrorHandling;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public void ParaProblemDetails_NotFoundException_Retorna404ComMensagem()
    {
        var problem = GlobalExceptionHandler.ParaProblemDetails(new NotFoundException("Usuário x não encontrado."));

        problem!.Status.Should().Be(404);
        problem.Detail.Should().Be("Usuário x não encontrado.");
    }

    [Fact]
    public void ParaProblemDetails_ConflictException_Retorna409()
    {
        var problem = GlobalExceptionHandler.ParaProblemDetails(new ConflictException("E-mail em uso."));

        problem!.Status.Should().Be(409);
    }

    [Fact]
    public void ParaProblemDetails_ValidationException_Retorna400ComErrosPorCampoEmCamelCase()
    {
        var exception = new ValidationException(
        [
            new ValidationFailure("Nome", "O nome é obrigatório."),
            new ValidationFailure("Email", "O e-mail é obrigatório."),
            new ValidationFailure("Email", "E-mail inválido.")
        ]);

        var problem = GlobalExceptionHandler.ParaProblemDetails(exception);

        problem!.Status.Should().Be(400);
        problem.Should().BeOfType<ValidationProblemDetails>()
            .Which.Errors.Should().BeEquivalentTo(new Dictionary<string, string[]>
            {
                ["nome"] = ["O nome é obrigatório."],
                ["email"] = ["O e-mail é obrigatório.", "E-mail inválido."]
            });
    }

    // Falha: exceção desconhecida não é tratada aqui e cai no 500 padrão, sem expor detalhes
    [Fact]
    public void ParaProblemDetails_ExcecaoDesconhecida_RetornaNull()
    {
        GlobalExceptionHandler.ParaProblemDetails(new InvalidOperationException("boom")).Should().BeNull();
    }
}
