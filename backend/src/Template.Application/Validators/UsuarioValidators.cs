using FluentValidation;
using Template.Application.Dtos.Usuarios;
using Template.Domain.Entities;

namespace Template.Application.Validators;

public class CriarUsuarioRequestValidator : AbstractValidator<CriarUsuarioRequest>
{
    public CriarUsuarioRequestValidator()
    {
        RuleFor(x => x.Nome).NomeDeUsuario();
        RuleFor(x => x.Email).EmailDeUsuario();
    }
}

public class AtualizarUsuarioRequestValidator : AbstractValidator<AtualizarUsuarioRequest>
{
    public AtualizarUsuarioRequestValidator()
    {
        RuleFor(x => x.Nome).NomeDeUsuario();
        RuleFor(x => x.Email).EmailDeUsuario();
    }
}

// Regras compartilhadas pelos dois requests (RN-01 e RN-02)
internal static class UsuarioRegras
{
    public static IRuleBuilderOptions<T, string> NomeDeUsuario<T>(this IRuleBuilder<T, string> regra)
        => regra
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(Usuario.NomeMaxLength).WithMessage("O nome deve ter no máximo {MaxLength} caracteres.");

    public static IRuleBuilderOptions<T, string> EmailDeUsuario<T>(this IRuleBuilder<T, string> regra)
        => regra
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(Usuario.EmailMaxLength).WithMessage("O e-mail deve ter no máximo {MaxLength} caracteres.");
}
