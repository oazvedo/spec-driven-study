using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Template.Domain.Entities;

namespace Template.Infrastructure.Data.Configurations;

public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(254).IsRequired();
        builder.HasIndex(x => x.Email).IsUnique();
    }
}
