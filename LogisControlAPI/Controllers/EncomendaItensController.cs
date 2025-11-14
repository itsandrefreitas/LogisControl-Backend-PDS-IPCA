using Microsoft.AspNetCore.Mvc;
using LogisControlAPI.Models;
using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Services;

namespace LogisControlAPI.Controllers
{

    /// <summary>
    /// Controlador responsável pela gestão dos itens de encomenda.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EncomendaItensController : ControllerBase
    {
        private readonly LogisControlContext _context;
        private readonly VerificacaoStockEncomendaService _verificacaoStockService;

        /// <summary>
        /// Construtor do controlador que injeta o contexto da base de dados.
        /// </summary>
        /// <param name="context">Instância do contexto da base de dados.</param>
        public EncomendaItensController(LogisControlContext context, VerificacaoStockEncomendaService verificacaoStockService)
        {
            _context = context;
            _verificacaoStockService = verificacaoStockService;
        }

        #region ObterEncomendaItens
        /// <summary>
        /// Obtém a lista de todos os itens de encomenda registados.
        /// </summary>
        /// <returns>Lista de itens de encomenda.</returns>
        /// <response code="200">Retorna a lista com sucesso.</response>
        /// <response code="500">Erro interno ao tentar obter os itens.</response>
        [HttpGet("ObterEncomendaItens")]
        public async Task<ActionResult<IEnumerable<EncomendaItensDTO>>> GetEncomendaItens()
        {
            try
            {
                var itens = await _context.EncomendasItem
                    .Select(e => new EncomendaItensDTO
                    {
                        EncomendaItensId = e.EncomendaItensId,
                        Quantidade = e.Quantidade,
                        ProdutoId = e.ProdutoId,
                        EncomendaClienteEncomendaClienteId = e.EncomendaClienteEncomendaClienteId
                    })
                    .ToListAsync();

                return Ok(itens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao obter itens de encomenda: {ex.Message}");
            }
        }
        #endregion

        #region ObterEncomendaItemPorId
        /// <summary>
        /// Obtém um item de encomenda pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único do item.</param>
        /// <returns>Dados do item de encomenda correspondente.</returns>
        /// <response code="200">Item encontrado com sucesso.</response>
        /// <response code="404">Item não encontrado.</response>
        /// <response code="500">Erro interno ao procurar o item.</response>
        [HttpGet("ObterEncomendaItem/{id}")]
        public async Task<ActionResult<EncomendaItensDTO>> GetEncomendaItemPorId(int id)
        {
            try
            {
                var item = await _context.EncomendasItem
                    .Where(e => e.EncomendaItensId == id)
                    .Select(e => new EncomendaItensDTO
                    {
                        EncomendaItensId = e.EncomendaItensId,
                        Quantidade = e.Quantidade,
                        EncomendaClienteEncomendaClienteId = e.EncomendaClienteEncomendaClienteId
                    })
                    .FirstOrDefaultAsync();

                if (item == null)
                    return NotFound("Item de encomenda não encontrado.");

                return Ok(item);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter item de encomenda: {ex.Message}");
            }
        }
        #endregion

        #region CriarEncomendaItem
        /// <summary>
        /// Cria um novo item de encomenda.
        /// </summary>
        /// <param name="novoItemDto">Dados para criação do item.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="201">Item de encomenda criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="500">Erro interno ao criar o item.</response>
        [HttpPost("CriarEncomendaItem")]
        public async Task<IActionResult> CriarEncomendaItem([FromBody] EncomendaItensDTO novoItemDto)
        {
            try
            {
                var novoItem = new EncomendaItens
                {
                    Quantidade = novoItemDto.Quantidade,
                    ProdutoId = novoItemDto.ProdutoId,
                    EncomendaClienteEncomendaClienteId = novoItemDto.EncomendaClienteEncomendaClienteId
                };

                _context.EncomendasItem.Add(novoItem);
                await _context.SaveChangesAsync();

                await _verificacaoStockService.VerificarStockParaEncomenda(novoItem.EncomendaClienteEncomendaClienteId);

                return StatusCode(201, "Item criado e stock verificado.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro: {ex.Message}");
            }
        }
        #endregion

        #region AtualizarEncomendaItem
        /// <summary>
        /// Atualiza os dados de um item de encomenda existente.
        /// </summary>
        /// <param name="itemId">ID do item a atualizar.</param>
        /// <param name="itemAtualizado">Dados atualizados do item.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Item atualizado com sucesso.</response>
        /// <response code="404">Item não encontrado.</response>
        /// <response code="500">Erro interno ao tentar atualizar o item.</response>
        [HttpPut("AtualizarEncomendaItem/{itemId}")]
        public async Task<IActionResult> AtualizarEncomendaItem(int itemId, [FromBody] EncomendaItensDTO itemAtualizado)
        {
            try
            {
                var item = await _context.EncomendasItem.FindAsync(itemId);

                if (item == null)
                    return NotFound("Item de encomenda não encontrado.");

                // Atualizar os campos
                item.Quantidade = itemAtualizado.Quantidade;
                item.EncomendaClienteEncomendaClienteId = itemAtualizado.EncomendaClienteEncomendaClienteId;

                await _context.SaveChangesAsync();

                return Ok("Item de encomenda atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar item de encomenda: {ex.Message}");
            }
        }
        #endregion
    }
}
