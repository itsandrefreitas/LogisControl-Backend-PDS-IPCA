using Microsoft.AspNetCore.Mvc;
using LogisControlAPI.Models;
using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LogisControlAPI.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão das assistências externas.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AssistenciaExternaController : ControllerBase
    {
        private readonly LogisControlContext _context;

        /// <summary>
        /// Construtor do controlador que injeta o contexto da base de dados.
        /// </summary>
        /// <param name="context">Instância do contexto da base de dados.</param>
        public AssistenciaExternaController(LogisControlContext context)
        {
            _context = context;
        }

        #region ObterAssistencias
        /// <summary>
        /// Obtém a lista de todas as assistências externas registadas.
        /// </summary>
        /// <returns>Lista de assistências externas.</returns>
        /// <response code="200">Retorna a lista com sucesso.</response>
        /// <response code="500">Erro interno ao tentar obter as assistências.</response>
        [HttpGet("ObterAssistencias")]
        [Authorize(Roles = "Tecnico")]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<AssistenciaExternaDTO>>> GetAssistencias()
        {
            try
            {
                var assistencias = await _context.AssistenciasExternas
                    .Select(a => new AssistenciaExternaDTO
                    {
                        AssistenteId = a.AssistenteId,
                        Nome = a.Nome,
                        Nif = a.Nif,
                        Morada = a.Morada,
                        Telefone = a.Telefone
                    })
                    .ToListAsync();

                return Ok(assistencias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao obter assistências externas: {ex.Message}");
            }
        }
        #endregion

        #region ObterAssistenciaPorId
        /// <summary>
        /// Obtém uma assistência externa pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único da assistência.</param>
        /// <returns>Dados da assistência correspondente.</returns>
        /// <response code="200">Assistência encontrada com sucesso.</response>
        /// <response code="404">Assistência não encontrada.</response>
        /// <response code="500">Erro interno ao procurar a assistência.</response>
        [HttpGet("ObterAssistencia/{id}")]
        [Authorize(Roles = "Tecnico")]
        [Produces("application/json")]
        public async Task<ActionResult<AssistenciaExternaDTO>> GetAssistenciaPorId(int id)
        {
            try
            {
                var assistencia = await _context.AssistenciasExternas
                    .Where(a => a.AssistenteId == id)
                    .Select(a => new AssistenciaExternaDTO
                    {
                        AssistenteId = a.AssistenteId,
                        Nome = a.Nome,
                        Nif = a.Nif,
                        Morada = a.Morada,
                        Telefone = a.Telefone
                    })
                    .FirstOrDefaultAsync();

                if (assistencia == null)
                    return NotFound("Assistência externa não encontrada.");

                return Ok(assistencia);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter assistência externa: {ex.Message}");
            }
        }
        #endregion

        #region CriarAssistencia
        /// <summary>
        /// Cria uma nova assistência externa.
        /// </summary>
        /// <param name="novaAssistenciaDto">Dados para criação da assistência.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="201">Assistência criada com sucesso.</response>
        /// <response code="400">Dados inválidos ou duplicados.</response>
        /// <response code="500">Erro interno ao criar a assistência.</response>
        [HttpPost("CriarAssistencia")]
        [Authorize(Roles = "Tecnico")]
        [Produces("application/json")]
        public async Task<ActionResult> CriarAssistencia([FromBody] AssistenciaExternaDTO novaAssistenciaDto)
        {
            try
            {
                // Verifica se já existe assistência com o mesmo NIF
                var existente = await _context.AssistenciasExternas
                    .AnyAsync(a => a.Nif == novaAssistenciaDto.Nif);

                if (existente)
                    return BadRequest("Já existe uma assistência externa com o mesmo NIF.");

                var novaAssistencia = new AssistenciaExterna
                {
                    Nome = novaAssistenciaDto.Nome,
                    Nif = novaAssistenciaDto.Nif,
                    Morada = novaAssistenciaDto.Morada,
                    Telefone = novaAssistenciaDto.Telefone
                };

                _context.AssistenciasExternas.Add(novaAssistencia);
                await _context.SaveChangesAsync();

                return StatusCode(201, "Assistência externa criada com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao criar assistência externa: {ex.Message}");
            }
        }
        #endregion

        #region AtualizarAssistencia
        /// <summary>
        /// Atualiza os dados de uma assistência externa existente.
        /// </summary>
        /// <param name="assistenteId">ID da assistência a atualizar.</param>
        /// <param name="assistenciaAtualizada">Dados atualizados da assistência.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Assistência atualizada com sucesso.</response>
        /// <response code="404">Assistência não encontrada.</response>
        /// <response code="400">NIF duplicado ou dados inválidos.</response>
        /// <response code="500">Erro interno ao tentar atualizar a assistência.</response>
        [HttpPut("AtualizarAssistencia/{assistenteId}")]
        [Authorize(Roles = "Tecnico")]
        [Produces("application/json")]
        public async Task<IActionResult> AtualizarAssistencia(int assistenteId, [FromBody] AssistenciaExternaDTO assistenciaAtualizada)
        {
            try
            {
                var assistencia = await _context.AssistenciasExternas.FindAsync(assistenteId);

                if (assistencia == null)
                    return NotFound("Assistência externa não encontrada.");

                // Verificar duplicação de NIF (em outro registo)
                bool nifDuplicado = await _context.AssistenciasExternas
                    .AnyAsync(a => a.Nif == assistenciaAtualizada.Nif && a.AssistenteId != assistenteId);

                if (nifDuplicado)
                    return BadRequest("Já existe outra assistência externa com o mesmo NIF.");

                // Atualizar os campos
                assistencia.Nome = assistenciaAtualizada.Nome;
                assistencia.Nif = assistenciaAtualizada.Nif;
                assistencia.Morada = assistenciaAtualizada.Morada;
                assistencia.Telefone = assistenciaAtualizada.Telefone;

                await _context.SaveChangesAsync();

                return Ok("Assistência externa atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar assistência externa: {ex.Message}");
            }
        }
        #endregion

        #region ObterAssistenciaPorPedido
        /// <summary>
        /// Obtém a assistência externa associada à máquina de um pedido de manutenção.
        /// </summary>
        /// <param name="pedidoId">ID do pedido de manutenção.</param>
        /// <returns>Informação da assistência externa (ID e Nome), ou null se não existir.</returns>
        /// <response code="200">Assistência obtida com sucesso ou inexistente.</response>
        /// <response code="404">Pedido de manutenção não encontrado.</response>
        /// <response code="500">Erro interno ao obter a assistência.</response>
        [HttpGet("ObterAssistenciaPorPedido/{pedidoId}")]
        [Authorize(Roles = "Tecnico")]
        [Produces("application/json")]
        public async Task<ActionResult<AssistenciaExternaPedidoDTO>> ObterAssistenciaPorPedido(int pedidoId)
        {
            try
            {
                var pedido = await _context.PedidosManutencao
                    .Include(p => p.MaquinaMaquina)
                        .ThenInclude(m => m.AssistenciaExternaAssistente)
                    .FirstOrDefaultAsync(p => p.PedidoManutId == pedidoId);

                if (pedido == null)
                    return NotFound("Pedido de manutenção não encontrado.");

                var assistencia = pedido.MaquinaMaquina?.AssistenciaExternaAssistente;

                if (assistencia == null)
                    return Ok(null); // máquina sem assistência associada

                var dto = new AssistenciaExternaPedidoDTO
                {
                    AssistenteId = assistencia.AssistenteId,
                    Nome = assistencia.Nome
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter assistência externa: {ex.Message}");
            }
        }
        #endregion

    }
}
