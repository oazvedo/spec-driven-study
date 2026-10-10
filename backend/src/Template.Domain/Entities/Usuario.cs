using Template.Domain.Common;

namespace Template.Domain.Entities;

public class Usuario : EntityBase
{
    public string Nome { get; private set; } = null!;
    public string Email { get; private set; } = null!;

    private Usuario() { } // EF Core

    public Usuario(string nome, string email) => Atualizar(nome, email);

    public void Atualizar(string nome, string email)
    {
        Nome = nome;
        Email = NormalizarEmail(email);
    }

    // E-mail único sem diferenciar maiúsculas de minúsculas (spec 001, RN-02)
    public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();
}
