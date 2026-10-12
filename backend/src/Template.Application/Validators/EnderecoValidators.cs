using FluentValidation;
using Template.Application.Dtos.Enderecos;
using Template.Domain.Entities;

namespace Template.Application.Validators;

public class CriarEnderecoRequestValidator : EnderecoRequestValidator<CriarEnderecoRequest>;

public class AtualizarEnderecoRequestValidator : EnderecoRequestValidator<AtualizarEnderecoRequest>;

// Regras compartilhadas pelos dois requests
public abstract class EnderecoRequestValidator<T> : AbstractValidator<T> where T : IEnderecoRequest
{
    protected EnderecoRequestValidator()
    {
        RuleFor(x => x.UsuarioId).NotEmpty().WithMessage("O usuário é obrigatório.");
        Obrigatorio(RuleFor(x => x.Logradouro), "logradouro", Endereco.LogradouroMaxLength);
        Obrigatorio(RuleFor(x => x.Numero), "número", Endereco.NumeroMaxLength);
        RuleFor(x => x.Complemento).MaximumLength(Endereco.ComplementoMaxLength).WithMessage("O complemento deve ter no máximo {MaxLength} caracteres.");
        Obrigatorio(RuleFor(x => x.Bairro), "bairro", Endereco.BairroMaxLength);
        Obrigatorio(RuleFor(x => x.Cidade), "cidade", Endereco.CidadeMaxLength);
        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("O estado é obrigatório.")
            .Matches($"^[A-Z]{{{Endereco.EstadoLength}}}$").WithMessage("Informe a UF com 2 letras maiúsculas, por exemplo SP.");
        RuleFor(x => x.Cep)
            .NotEmpty().WithMessage("O CEP é obrigatório.")
            .Matches(@"^\d{5}-?\d{3}$").WithMessage("CEP inválido. Use 00000-000 ou 00000000.");
        RuleFor(x => x.Tipo).IsInEnum().WithMessage("Tipo de endereço inválido.");
    }

    private static void Obrigatorio(IRuleBuilder<T, string> regra, string campo, int maxLength)
        => regra
            .NotEmpty().WithMessage($"O campo {campo} é obrigatório.")
            .MaximumLength(maxLength).WithMessage($"O campo {campo} deve ter no máximo {{MaxLength}} caracteres.");
}
