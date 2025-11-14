using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using LogisControlAPI.Models;

namespace LogisControlAPI.Controllers
{
    /// <summary>
    /// Controlador responsável por atualizar a quantidade de matérias-primas utilizadas numa ordem de produção.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProdMateriaisController : ControllerBase
    {
        private readonly LogisControlContext _context;

        public ProdMateriaisController(LogisControlContext context)
        {
            _context = context;
        }

        #region CriarProdMaterial
        /// <summary>
        /// Regista um novo material utilizado numa ordem de produção.
        /// </summary>
        /// <param name="dto">Dados do material a registar.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="201">Material registado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="500">Erro interno ao registar o material.</response>
        [HttpPost("CriarProdMaterial")]
        public async Task<IActionResult> CriarProdMaterial([FromBody] ProdMaterialDTO dto)
        {
            try
            {
                // Validação básica
                if (dto.QuantidadeUtilizada <= 0 || dto.OrdemProducaoOrdemProdId <= 0 || dto.MateriaPrimaMateriaPrimaId <= 0)
                {
                    return BadRequest("Todos os campos devem ter valores válidos.");
                }

                var novaEntrada = new ProdMateriais
                {
                    QuantidadeUtilizada = dto.QuantidadeUtilizada,
                    OrdemProducaoOrdemProdId = dto.OrdemProducaoOrdemProdId,
                    MateriaPrimaMateriaPrimaId = dto.MateriaPrimaMateriaPrimaId
                };

                await _context.ProdMateriais.AddAsync(novaEntrada);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(AtualizarQuantidade), new { id = novaEntrada.ProdMateriaisId }, "Material registado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao registar o material: {ex.Message}");
            }
        }
        #endregion

        #region AtualizarQuantidade
        /// <summary>
        /// Atualiza a quantidade utilizada de uma matéria-prima numa ordem de produção.
        /// </summary>
        /// <param name="id">ID do registo ProdMateriais.</param>
        /// <param name="dto">Nova quantidade a atualizar.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Quantidade atualizada com sucesso.</response>
        /// <response code="404">Registo não encontrado.</response>
        /// <response code="500">Erro interno ao atualizar o registo.</response>
        [HttpPut("AtualizarQuantidade/{id}")]
        public async Task<IActionResult> AtualizarQuantidade(int id, [FromBody] ProdMaterialDTO dto)
        {
            try
            {
                var registo = await _context.ProdMateriais.FindAsync(id);
                if (registo == null)
                    return NotFound("Registo de material não encontrado.");

                registo.QuantidadeUtilizada = dto.QuantidadeUtilizada;

                await _context.SaveChangesAsync();
                return Ok("Quantidade atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar a quantidade: {ex.Message}");
            }
        }
        #endregion

    }
}