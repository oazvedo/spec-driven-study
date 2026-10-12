using Template.Domain.Common;
using Template.Domain.Entities.Enums;

namespace Template.Domain.Entities;

public class Endereco : EntityBase
{
    // Limites usados pela EnderecoConfig (banco) e pelos validators (API)
    public const int LogradouroMaxLength = 100;
    public const int NumeroMaxLength = 10;
    public const int ComplementoMaxLength = 100;
    public const int BairroMaxLength = 100;
    public const int CidadeMaxLength = 100;
    public const int EstadoLength = 2; // UF
    public const int CepMaxLength = 9; // 00000-000

    public Guid UsuarioId { get; private set; }
    public string Logradouro { get; private set; } = null!;
    public string Numero { get; private set; } = null!;
    public string? Complemento { get; private set; }
    public string Bairro { get; private set; } = null!;
    public string Cidade { get; private set; } = null!;
    public string Estado { get; private set; } = null!;
    public string Cep { get; private set; } = null!;
    public EnderecoTipo Tipo { get; private set; }

    private Endereco() { } // EF Core

    public Endereco(Guid usuarioId, string logradouro, string numero, string? complemento, string bairro, string cidade, string estado, string cep, EnderecoTipo tipo)
        => Atualizar(usuarioId, logradouro, numero, complemento, bairro, cidade, estado, cep, tipo);

    public void Atualizar(Guid usuarioId, string logradouro, string numero, string? complemento, string bairro, string cidade, string estado, string cep, EnderecoTipo tipo)
    {
        UsuarioId = usuarioId;
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
        Cep = cep;
        Tipo = tipo;
    }
}
