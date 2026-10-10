namespace Template.Domain.Common;

public abstract class EntityBase
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();

    // Preenchidas pelo AppDbContext ao salvar (ADR 0004)
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Exclusão lógica (ADR 0008)
    public bool Ativo { get; private set; } = true;

    public void Ativar() => Ativo = true;

    public void Inativar() => Ativo = false;
}
