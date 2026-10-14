using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Template.Application.Dtos.Enderecos;
using Template.Application.Services.Interfaces;
using Template.Domain.Common;

namespace Template.Api.Controllers
{
    [ApiController]
    [Route("api/enderecos")]
    public class EnderecosController(IEnderecoService enderecoService) : ControllerBase
    {
        [HttpGet]
        public Task<PagedResult<EnderecoResponse>> Listar(
            [Range(1, int.MaxValue, ErrorMessage = "A página deve ser maior ou igual a 1.")] int page = 1,
            [Range(1, 100, ErrorMessage = "O tamanho da página deve estar entre 1 e 100.")] int pageSize = 20,
            CancellationToken cancellationToken = default) => enderecoService.ListarAsync(page, pageSize, cancellationToken);

        [HttpGet("{id:guid}")]
        public Task<EnderecoResponse> Obter(Guid id, CancellationToken cancellationToken)
            => enderecoService.ObterAsync(id, cancellationToken);

        [HttpPost]
        [ProducesResponseType<EnderecoResponse>(StatusCodes.Status201Created)]
        public async Task<IActionResult> Criar(CriarEnderecoRequest request, CancellationToken cancellationToken)
        {
            var endereco = await enderecoService.CriarAsync(request, cancellationToken);
            return CreatedAtAction(nameof(Obter), new { id = endereco.Id }, endereco);
        }

        [HttpPut("{id:guid}")]
        public Task<EnderecoResponse> Atualizar(Guid id, AtualizarEnderecoRequest request, CancellationToken cancellationToken)
            => enderecoService.AtualizarAsync(id, request, cancellationToken);

        [HttpPatch("{id:guid}/ativar")]
        public async Task<IActionResult> Ativar(Guid id, CancellationToken cancellationToken)
        {
            await enderecoService.AtivarAsync(id, cancellationToken);
            return NoContent();
        }

        [HttpPatch("{id:guid}/inativar")]
        public async Task<IActionResult> Inativar(Guid id, CancellationToken cancellationToken)
        {
            await enderecoService.InativarAsync(id, cancellationToken);
            return NoContent();
        }

        // Exceção ao "sem DELETE": endereço não tem valor histórico (ADR 0016)
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Excluir(Guid id, CancellationToken cancellationToken)
        {
            await enderecoService.ExcluirAsync(id, cancellationToken);
            return NoContent();
        }
    }
}