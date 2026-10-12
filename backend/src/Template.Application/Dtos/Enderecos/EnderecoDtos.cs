using Template.Domain.Entities;
using Template.Domain.Entities.Enums;

namespace Template.Application.Dtos.Enderecos;

// Campos comuns aos dois requests; permite um único conjunto de regras de validação
public interface IEnderecoRequest
{
    Guid UsuarioId { get; }
    string Logradouro { get; }
    string Numero { get; }
    string? Complemento { get; }
    string Bairro { get; }
    string Cidade { get; }
    string Estado { get; }
    string Cep { get; }
    EnderecoTipo Tipo { get; }
}

public record CriarEnderecoRequest(Guid UsuarioId, string Logradouro, string Numero, string? Complemento, string Bairro, string Cidade, string Estado, string Cep, EnderecoTipo Tipo) : IEnderecoRequest;

public record AtualizarEnderecoRequest(Guid UsuarioId, string Logradouro, string Numero, string? Complemento, string Bairro, string Cidade, string Estado, string Cep, EnderecoTipo Tipo) : IEnderecoRequest;

public record EnderecoResponse(Guid Id, Guid UsuarioId, string Logradouro, string Numero, string? Complemento, string Bairro, string Cidade, string Estado, string Cep, EnderecoTipo Tipo);

// Mapeamento manual (ADR 0007)
public static class EnderecoMappings
{
    public static EnderecoResponse ToResponse(this Endereco e)
        => new(e.Id, e.UsuarioId, e.Logradouro, e.Numero, e.Complemento, e.Bairro, e.Cidade, e.Estado, e.Cep, e.Tipo);
}
