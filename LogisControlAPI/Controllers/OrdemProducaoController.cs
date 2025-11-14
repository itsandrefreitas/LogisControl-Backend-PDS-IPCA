using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using LogisControlAPI.Models;
using LogisControlAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogisControlAPI.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão das ordens de produção.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrdemProducaoController : ControllerBase
    {
        private readonly LogisControlContext _context;

        /// <summary>
        /// Construtor do controlador que injeta o contexto da base de dados.
        /// </summary>
        /// <param name="context">Instância do contexto da base de dados.</param>
        public OrdemProducaoController(LogisControlContext context)
        {
            _context = context;
        }
        [HttpGet("{ordemProducaoId}/produtos")]

        public async Task<IActionResult> ObterProdutosPorOrdemProducao(int ordemProducaoId)
        {
            var produtos = await _context.Produtos
                .Where(p => p.OrdemProducaoOrdemProdId == ordemProducaoId)
                .Select(p => new {
                    p.ProdutoId,
                })
                .ToListAsync();

            return Ok(produtos);
        }

        #region ListarOrdens
        /// <summary>
        /// Obtém todas as ordens de produção.
        /// </summary>
        /// <returns>Lista de ordens de produção.</returns>
        /// <response code="200">Lista obtida com sucesso.</response>
        /// <response code="500">Erro ao obter as ordens de produção.</response>
        [HttpGet("ListarOrdensProducao")]
        public async Task<ActionResult<IEnumerable<OrdemProducaoDTO>>> GetAll()
        {
            try
            {
                var ordens = await _context.OrdensProducao
                    .Select(o => new OrdemProducaoDTO
                    {
                        OrdemProdId = o.OrdemProdId,
                        Estado = o.Estado,
                        Quantidade = o.Quantidade,
                        DataAbertura = o.DataAbertura,
                        DataConclusao = o.DataConclusao,
                        MaquinaMaquinaId = o.MaquinaMaquinaId,
                        EncomendaClienteEncomendaClienteId = o.EncomendaClienteEncomendaClienteId
                    })
                    .ToListAsync();

                return Ok(ordens);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao obter as ordens de produção: {ex.Message}");
            }
        }
        #endregion

        #region ObterPorId
        /// <summary>
        /// Obtém uma ordem de produção pelo ID.
        /// </summary>
        /// <param name="id">ID da ordem de produção.</param>
        /// <returns>Dados da ordem de produção.</returns>
        /// <response code="200">Ordem encontrada.</response>
        /// <response code="404">Ordem não encontrada.</response>
        [HttpGet("ObterOrdemProducaoPorId/{id}")]
        public async Task<ActionResult<OrdemProducaoDTO>> GetById(int id)
        {
            var ordem = await _context.OrdensProducao.FindAsync(id);
            if (ordem == null)
            {
                return NotFound();
            }
            return Ok(ordem);
        }
        #endregion

        #region CriarOrdem
        /// <summary>
        /// Cria uma nova ordem de produção.
        /// </summary>
        /// <param name="dto">Dados da nova ordem.</param>
        /// <returns>Ordem criada.</returns>
        /// <response code="201">Ordem criada com sucesso.</response>
        [HttpPost("CriarOrdemProducao")]
        public async Task<ActionResult<OrdemProducaoDTO>> Create([FromBody] OrdemProducaoDTO dto)
        {
            var ordem = new OrdemProducao
            {
                Estado = dto.Estado,
                Quantidade = dto.Quantidade,
                DataAbertura = DateTime.Now,
                DataConclusao = dto.DataConclusao,
                MaquinaMaquinaId = dto.MaquinaMaquinaId,
                EncomendaClienteEncomendaClienteId = dto.EncomendaClienteEncomendaClienteId
            };

            _context.OrdensProducao.Add(ordem);
            await _context.SaveChangesAsync();

            // Atualiza o estado da encomenda para "Em produção"
            var encomenda = await _context.EncomendasCliente
                .FindAsync(dto.EncomendaClienteEncomendaClienteId);

            if (encomenda != null)
            {
                encomenda.Estado = "Em produção";
                await _context.SaveChangesAsync();
            }

            var ordemDto = new OrdemProducaoDTO
            {
                OrdemProdId = ordem.OrdemProdId,
                Estado = ordem.Estado,
                Quantidade = ordem.Quantidade,
                DataAbertura = ordem.DataAbertura,
                DataConclusao = ordem.DataConclusao,
                MaquinaMaquinaId = ordem.MaquinaMaquinaId,
                EncomendaClienteEncomendaClienteId = ordem.EncomendaClienteEncomendaClienteId
            };

            return CreatedAtAction(nameof(GetById), new { id = ordem.OrdemProdId }, ordemDto);
        }
        #endregion

        #region AtualizarOrdem
        /// <summary>
        /// Atualiza os dados de uma ordem de produção.
        /// </summary>
        /// <param name="id">ID da ordem a ser atualizada.</param>
        /// <param name="dto">Novos dados da ordem.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        /// <response code="204">Ordem atualizada com sucesso.</response>
        /// <response code="404">Ordem não encontrada.</response>
        [HttpPut("AtualizarOrdemProducao/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] OrdemProducaoDTO dto)
        {
            var ordem = await _context.OrdensProducao.FindAsync(id);
            if (ordem == null)
            {
                return NotFound();
            }

            ordem.Estado = dto.Estado;
            ordem.Quantidade = dto.Quantidade;
            ordem.DataAbertura = dto.DataAbertura;
            ordem.DataConclusao = dto.DataConclusao;
            ordem.MaquinaMaquinaId = dto.MaquinaMaquinaId;
            ordem.EncomendaClienteEncomendaClienteId = dto.EncomendaClienteEncomendaClienteId;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        #region DeletarOrdem
        /// <summary>
        /// Exclui uma ordem de produção pelo ID.
        /// </summary>
        /// <param name="id">ID da ordem a ser excluída.</param>
        /// <returns>Sem conteúdo em caso de sucesso.</returns>
        /// <response code="204">Ordem excluída com sucesso.</response>
        /// <response code="404">Ordem não encontrada.</response>
        [HttpDelete("ApagarOrdemProducao/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var ordem = await _context.OrdensProducao.FindAsync(id);
            if (ordem == null)
            {
                return NotFound();
            }

            _context.OrdensProducao.Remove(ordem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
    }
}
