using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Domain.Entities;

namespace Template.Infrastructure.Data.Configurations;

public class EnderecoConfig : IEntityTypeConfiguration<Endereco>
{
    public void Configure(EntityTypeBuilder<Endereco> builder)
    {
        builder.ToTable("enderecos");
        builder.HasKey(x => x.Id);
        builder.HasOne<Usuario>().WithMany().HasForeignKey(x => x.UsuarioId);
        builder.Property(x => x.Logradouro).HasMaxLength(Endereco.LogradouroMaxLength).IsRequired();
        builder.Property(x => x.Numero).HasMaxLength(Endereco.NumeroMaxLength).IsRequired();
        builder.Property(x => x.Complemento).HasMaxLength(Endereco.ComplementoMaxLength);
        builder.Property(x => x.Bairro).HasMaxLength(Endereco.BairroMaxLength).IsRequired();
        builder.Property(x => x.Cidade).HasMaxLength(Endereco.CidadeMaxLength).IsRequired();
        builder.Property(x => x.Estado).HasMaxLength(Endereco.EstadoLength).IsFixedLength().IsRequired();
        builder.Property(x => x.Cep).HasMaxLength(Endereco.CepMaxLength).IsRequired();
    }
}
