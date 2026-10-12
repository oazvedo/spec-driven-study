using Template.Domain.Entities;
using Template.Domain.Entities.Interfaces;
using Template.Infrastructure.Data;

namespace Template.Infrastructure.Repositories;

public class EnderecoRepository(AppDbContext context) : RepositoryBase<Endereco>(context), IEnderecoRepository;
