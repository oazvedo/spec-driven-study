using FluentValidation.TestHelper;
using Template.Application.Dtos.Enderecos;
using Template.Application.Validators;
using Template.Domain.Entities;
using Template.Domain.Entities.Enums;

namespace Template.UnitTests.Application.Validators;

public class CriarEnderecoRequestValidatorTests
{
    private readonly CriarEnderecoRequestValidator _validator = new();

    // Sem complemento: o caso válido já cobre o RN-02 (complemento opcional)
    private static readonly CriarEnderecoRequest Valido =
        new(Guid.NewGuid(), "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP", "01310-100", EnderecoTipo.Comercial);

    [Fact]
    public void Validate_RequestValido_NaoTemErros()
        => _validator.TestValidate(Valido).ShouldNotHaveAnyValidationErrors();

    // RN-05: com ou sem hífen
    [Theory]
    [InlineData("01310-100")]
    [InlineData("01310100")]
    public void Validate_CepComOuSemHifen_Passa(string cep)
        => _validator.TestValidate(Valido with { Cep = cep }).ShouldNotHaveValidationErrorFor(x => x.Cep);

    // CA-04 (falha)
    [Fact]
    public void Validate_CamposObrigatoriosVazios_FalhaComMensagemPorCampo()
    {
        var resultado = _validator.TestValidate(Valido with
        {
            UsuarioId = Guid.Empty, Logradouro = "", Numero = " ", Bairro = "", Cidade = "", Estado = "", Cep = ""
        });

        resultado.ShouldHaveValidationErrorFor(x => x.UsuarioId).WithErrorMessage("O usuário é obrigatório.");
        resultado.ShouldHaveValidationErrorFor(x => x.Logradouro).WithErrorMessage("O campo logradouro é obrigatório.");
        resultado.ShouldHaveValidationErrorFor(x => x.Numero).WithErrorMessage("O campo número é obrigatório.");
        resultado.ShouldHaveValidationErrorFor(x => x.Bairro).WithErrorMessage("O campo bairro é obrigatório.");
        resultado.ShouldHaveValidationErrorFor(x => x.Cidade).WithErrorMessage("O campo cidade é obrigatório.");
        resultado.ShouldHaveValidationErrorFor(x => x.Estado).WithErrorMessage("O estado é obrigatório.");
        resultado.ShouldHaveValidationErrorFor(x => x.Cep).WithErrorMessage("O CEP é obrigatório.");
    }

    // CA-04 / RN-03 (falha)
    [Fact]
    public void Validate_AcimaDoLimite_FalhaComMensagem()
    {
        var resultado = _validator.TestValidate(Valido with
        {
            Logradouro = new string('a', Endereco.LogradouroMaxLength + 1),
            Numero = new string('1', Endereco.NumeroMaxLength + 1),
            Complemento = new string('a', Endereco.ComplementoMaxLength + 1),
        });

        resultado.ShouldHaveValidationErrorFor(x => x.Logradouro)
            .WithErrorMessage($"O campo logradouro deve ter no máximo {Endereco.LogradouroMaxLength} caracteres.");
        resultado.ShouldHaveValidationErrorFor(x => x.Numero)
            .WithErrorMessage($"O campo número deve ter no máximo {Endereco.NumeroMaxLength} caracteres.");
        resultado.ShouldHaveValidationErrorFor(x => x.Complemento)
            .WithErrorMessage($"O complemento deve ter no máximo {Endereco.ComplementoMaxLength} caracteres.");
    }

    [Fact]
    public void Validate_NoLimite_Passa()
        => _validator.TestValidate(Valido with { Logradouro = new string('a', Endereco.LogradouroMaxLength) })
            .ShouldNotHaveValidationErrorFor(x => x.Logradouro);

    // CA-05 / RN-04 (falha)
    [Theory]
    [InlineData("sp")]
    [InlineData("SPX")]
    [InlineData("S")]
    public void Validate_UfInvalida_FalhaComMensagem(string estado)
        => _validator.TestValidate(Valido with { Estado = estado })
            .ShouldHaveValidationErrorFor(x => x.Estado).WithErrorMessage("Informe a UF com 2 letras maiúsculas, por exemplo SP.");

    // CA-05 / RN-05 (falha)
    [Theory]
    [InlineData("1234-567")]
    [InlineData("01310-1000")]
    [InlineData("abcde-fgh")]
    public void Validate_CepInvalido_FalhaComMensagem(string cep)
        => _validator.TestValidate(Valido with { Cep = cep })
            .ShouldHaveValidationErrorFor(x => x.Cep).WithErrorMessage("CEP inválido. Use 00000-000 ou 00000000.");

    // CA-05 / RN-06 (falha)
    [Fact]
    public void Validate_TipoForaDoEnum_FalhaComMensagem()
        => _validator.TestValidate(Valido with { Tipo = (EnderecoTipo)99 })
            .ShouldHaveValidationErrorFor(x => x.Tipo).WithErrorMessage("Tipo de endereço inválido.");
}

// As regras são as mesmas do Criar; aqui basta garantir que o Atualizar as aplica
public class AtualizarEnderecoRequestValidatorTests
{
    private readonly AtualizarEnderecoRequestValidator _validator = new();

    private static readonly AtualizarEnderecoRequest Valido =
        new(Guid.NewGuid(), "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP", "01310-100", EnderecoTipo.Residencial);

    [Fact]
    public void Validate_RequestValido_NaoTemErros()
        => _validator.TestValidate(Valido).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void Validate_CampoVazioOuCepInvalido_Falha()
    {
        _validator.TestValidate(Valido with { Logradouro = "" }).ShouldHaveValidationErrorFor(x => x.Logradouro);
        _validator.TestValidate(Valido with { Cep = "123" }).ShouldHaveValidationErrorFor(x => x.Cep);
    }
}
