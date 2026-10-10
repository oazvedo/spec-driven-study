using FluentValidation.TestHelper;
using Template.Application.Dtos.Usuarios;
using Template.Application.Validators;
using Template.Domain.Entities;

namespace Template.UnitTests.Application.Validators;

public class CriarUsuarioRequestValidatorTests
{
    private readonly CriarUsuarioRequestValidator _validator = new();

    private static string EmailComTamanho(int tamanho) => new string('a', tamanho - "@x.com".Length) + "@x.com";

    [Fact]
    public void Validate_RequestValido_NaoTemErros()
        => _validator.TestValidate(new CriarUsuarioRequest("Ana", "ana@x.com")).ShouldNotHaveAnyValidationErrors();

    // CA-04 (falha)
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_NomeVazio_FalhaComMensagem(string nome)
        => _validator.TestValidate(new CriarUsuarioRequest(nome, "ana@x.com"))
            .ShouldHaveValidationErrorFor(x => x.Nome).WithErrorMessage("O nome é obrigatório.");

    // RN-01 (falha)
    [Fact]
    public void Validate_NomeAcimaDoLimite_FalhaComMensagem()
        => _validator.TestValidate(new CriarUsuarioRequest(new string('a', Usuario.NomeMaxLength + 1), "ana@x.com"))
            .ShouldHaveValidationErrorFor(x => x.Nome)
            .WithErrorMessage($"O nome deve ter no máximo {Usuario.NomeMaxLength} caracteres.");

    [Fact]
    public void Validate_NomeNoLimite_Passa()
        => _validator.TestValidate(new CriarUsuarioRequest(new string('a', Usuario.NomeMaxLength), "ana@x.com"))
            .ShouldNotHaveValidationErrorFor(x => x.Nome);

    // CA-04 (falha)
    [Fact]
    public void Validate_EmailVazio_FalhaComMensagem()
        => _validator.TestValidate(new CriarUsuarioRequest("Ana", ""))
            .ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("O e-mail é obrigatório.");

    // CA-05 (falha)
    [Fact]
    public void Validate_EmailInvalido_FalhaComMensagem()
        => _validator.TestValidate(new CriarUsuarioRequest("Ana", "nao-e-email"))
            .ShouldHaveValidationErrorFor(x => x.Email).WithErrorMessage("E-mail inválido.");

    // RN-02 (falha)
    [Fact]
    public void Validate_EmailAcimaDoLimite_Falha()
        => _validator.TestValidate(new CriarUsuarioRequest("Ana", EmailComTamanho(Usuario.EmailMaxLength + 1)))
            .ShouldHaveValidationErrorFor(x => x.Email);

    [Fact]
    public void Validate_EmailNoLimite_Passa()
        => _validator.TestValidate(new CriarUsuarioRequest("Ana", EmailComTamanho(Usuario.EmailMaxLength)))
            .ShouldNotHaveValidationErrorFor(x => x.Email);
}

// As regras são as mesmas do Criar; aqui basta garantir que o Atualizar as aplica
public class AtualizarUsuarioRequestValidatorTests
{
    private readonly AtualizarUsuarioRequestValidator _validator = new();

    [Fact]
    public void Validate_RequestValido_NaoTemErros()
        => _validator.TestValidate(new AtualizarUsuarioRequest("Ana", "ana@x.com")).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_CamposVaziosOuEmailInvalido_Falha()
    {
        _validator.TestValidate(new AtualizarUsuarioRequest("", "")).ShouldHaveValidationErrorFor(x => x.Nome);
        _validator.TestValidate(new AtualizarUsuarioRequest("Ana", "nao-e-email")).ShouldHaveValidationErrorFor(x => x.Email);
    }
}
