using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using LogisControlAPI.Interfaces;
using LogisControlAPI.Models;
using LogisControlAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogisControlAPI.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão dos produtos.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutoController : ControllerBase
    {
        private readonly LogisControlContext _context;
        private readonly ProdutoService _produtoService;

        /// <summary>
        /// Construtor do controlador que injeta o contexto da base de dados.
        /// </summary>
        /// <param name="context">Instância do contexto da base de dados.</param>
        public ProdutoController(LogisControlContext context, ProdutoService produtoService)
        {
            _context = context;
            _produtoService = produtoService;
        }

        #region ListarProdutos
        /// <summary>
        /// Lista todos os produtos.
        /// </summary>
        /// <returns>Lista de produtos.</returns>
        /// <response code="200">Lista obtida com sucesso.</response>
        /// <response code="500">Erro interno ao obter produtos.</response>
        [HttpGet("ListarProdutos")]
        [Authorize ("Gestor")]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<ProdutoDTO>>> GetAll()
        {
            try
            {
                var produtos = await _context.Produtos
                    .Select(p => new ProdutoDTO
                    {
                        ProdutoId = p.ProdutoId,
                        Nome = p.Nome,
                        Quantidade = p.Quantidade,
                        Descricao = p.Descricao,
                        CodInterno = p.CodInterno,
                        Preco = p.Preco,
                        OrdemProducaoOrdemProdId = p.OrdemProducaoOrdemProdId
                    })
                    .ToListAsync();

                return Ok(produtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao obter os produtos: {ex.Message}");
            }
        }
        #endregion

        #region ObterProdutoParaEdicao
        /// <summary>
        /// Obtém os dados de um produto e as matérias-primas associadas para edição.
        /// </summary>
        /// <param name="id">ID do produto.</param>
        /// <returns>DTO com os dados preenchidos para edição.</returns>
        /// <response code="200">Produto encontrado.</response>
        /// <response code="404">Produto não encontrado.</response>
        [HttpGet("ObterProdutoParaEdicao/{id}")]
        [Authorize("Gestor")]
        [Produces("application/json")]
        public async Task<ActionResult<CriarProdutoDTO>> ObterProdutoParaEdicao(int id)
        {
            var dto = await _produtoService.ObterProdutoParaEdicaoAsync(id);

            if (dto == null)
                return NotFound();

            return Ok(dto);
        }
        #endregion
        #region CriarProduto
        /// <summary>
        /// Cria um novo produto com matérias-primas associadas.
        /// </summary>
        /// <param name="dto">Dados do produto e das matérias-primas.</param>
        /// <returns>Resposta de sucesso ou erro.</returns>
        /// <response code="201">Produto criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="500">Erro interno ao criar o produto.</response>
        [HttpPost("CriarProduto")]
        [Authorize("Gestor")]
        [Produces("application/json")]
        public async Task<IActionResult> Create([FromBody] CriarProdutoDTO dto)
        {
            try
            {
                await _produtoService.CriarProdutoAsync(dto);
                return StatusCode(201, "Produto criado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao criar o produto: {ex.Message}");
            }
        }
        #endregion

        #region AtualizarProduto
        /// <summary>
        /// Atualiza um produto existente e as suas matérias-primas.
        /// </summary>
        /// <param name="id">ID do produto a ser atualizado.</param>
        /// <param name="dto">Novos dados do produto.</param>
        /// <returns>Status da atualização.</returns>
        /// <response code="204">Produto atualizado com sucesso.</response>
        /// <response code="404">Produto não encontrado.</response>
        [HttpPut("AtualizarProduto/{id}")]
        [Authorize("Gestor")]
        [Produces("application/json")]
        public async Task<IActionResult> Update(int id, [FromBody] CriarProdutoDTO dto)
        {
            try
            {
                await _produtoService.AtualizarProdutoAsync(id, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound($"Erro ao atualizar produto: {ex.Message}");
            }
        }
        #endregion

        #region DeletarProduto
        /// <summary>
        /// Exclui um produto pelo ID.
        /// </summary>
        /// <param name="id">ID do produto a ser excluído.</param>
        /// <returns>Status da exclusão.</returns>
        /// <response code="204">Produto excluído com sucesso.</response>
        /// <response code="404">Produto não encontrado.</response>
        [HttpDelete("ApagarProduto/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
    }
}
