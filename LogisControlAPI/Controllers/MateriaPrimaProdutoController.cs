using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using LogisControlAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogisControlAPI.Controllers
{
    /// <summary>
    /// Controlador responsável pela relação entre matéria-prima e produtos.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MateriaPrimaProdutoController : ControllerBase
    {
        private readonly LogisControlContext _context;

        /// <summary>
        /// Construtor do controlador que injeta o contexto da base de dados.
        /// </summary>
        /// <param name="context">Instância do contexto da base de dados.</param>
        public MateriaPrimaProdutoController(LogisControlContext context)
        {
            _context = context;
        }

        #region ListarMateriaPrimaProduto
        /// <summary>
        /// Lista todas as relações entre matéria-prima e produtos.
        /// </summary>
        /// <returns>Lista das relações.</returns>
        /// <response code="200">Lista obtida com sucesso.</response>
        /// <response code="500">Erro interno ao obter os registros.</response>
        [HttpGet("ListarMateriaPrimaProduto")]
        public async Task<ActionResult<IEnumerable<MateriaPrimaProdutoDTO>>> GetAll()
        {
            try
            {
                var items = await _context.MateriaPrimaProdutos
                    .Select(mpp => new MateriaPrimaProdutoDTO
                    {
                        MateriaPrimaProdutoId = mpp.MateriaPrimaProdutoId,
                        QuantidadeNec = mpp.QuantidadeNec,
                        MateriaPrimaMateriaPrimaId = mpp.MateriaPrimaMateriaPrimaId,
                        ProdutoProdutoId = mpp.ProdutoProdutoId
                    })
                    .ToListAsync();

                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao obter os registros: {ex.Message}");
            }
        }
        #endregion

        #region ObterMateriaPrimaProdutoPorId
        /// <summary>
        /// Obtém uma relação entre matéria-prima e produto pelo ID.
        /// </summary>
        /// <param name="id">ID da relação.</param>
        /// <returns>Dados da relação.</returns>
        /// <response code="200">Relação encontrada.</response>
        /// <response code="404">Relação não encontrada.</response>
        [HttpGet("ObterMateriaPrimaProdutoPorId/{id}")]
        public async Task<ActionResult<MateriaPrimaProdutoDTO>> GetById(int id)
        {
            var item = await _context.MateriaPrimaProdutos.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }
        #endregion

        #region CriarMateriaPrimaProduto
        /// <summary>
        /// Cria uma nova relação entre matéria-prima e produto.
        /// </summary>
        /// <param name="dto">Dados da nova relação.</param>
        /// <returns>Relação criada.</returns>
        /// <response code="201">Relação criada com sucesso.</response>
        [HttpPost("CriarMateriaPrimaProduto")]
        public async Task<ActionResult<MateriaPrimaProdutoDTO>> Create([FromBody] MateriaPrimaProdutoDTO dto)
        {
            var entity = new MateriaPrimaProduto
            {
                QuantidadeNec = dto.QuantidadeNec,
                MateriaPrimaMateriaPrimaId = dto.MateriaPrimaMateriaPrimaId,
                ProdutoProdutoId = dto.ProdutoProdutoId
            };

            _context.MateriaPrimaProdutos.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.MateriaPrimaProdutoId }, entity);
        }
        #endregion

        #region AtualizarMateriaPrimaProduto
        /// <summary>
        /// Atualiza os dados de uma relação entre matéria-prima e produto existente.
        /// </summary>
        /// <param name="id">ID da relação a ser atualizada.</param>
        /// <param name="dto">Novos dados da relação.</param>
        /// <returns>Resultado da atualização.</returns>
        /// <response code="204">Relação atualizada com sucesso.</response>
        /// <response code="404">Relação não encontrada.</response>
        [HttpPut("AtualizarMateriaPrimaProduto/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MateriaPrimaProdutoDTO dto)
        {
            var entity = await _context.MateriaPrimaProdutos.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            entity.QuantidadeNec = dto.QuantidadeNec;
            entity.MateriaPrimaMateriaPrimaId = dto.MateriaPrimaMateriaPrimaId;
            entity.ProdutoProdutoId = dto.ProdutoProdutoId;

            await _context.SaveChangesAsync();
            return NoContent();
        }
        #endregion

        #region DeletarMateriaPrimaProduto
        /// <summary>
        /// Exclui uma relação entre matéria-prima e produto pelo ID.
        /// </summary>
        /// <param name="id">ID da relação a ser excluída.</param>
        /// <returns>Resultado da exclusão.</returns>
        /// <response code="204">Relação excluída com sucesso.</response>
        /// <response code="404">Relação não encontrada.</response>
        [HttpDelete("ApagarMateriaPrimaProduto/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _context.MateriaPrimaProdutos.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            _context.MateriaPrimaProdutos.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
    }
}
