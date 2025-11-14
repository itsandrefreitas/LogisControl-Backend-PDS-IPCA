using Microsoft.AspNetCore.Mvc;
using LogisControlAPI.Models;
using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System;
using LogisControlAPI.Services;

namespace LogisControlAPI.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão dos registos de manutenção.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RegistoManutencaoController : ControllerBase
    {
        private readonly LogisControlContext _context;
        private readonly ManutencaoService _manutencaoService;


        /// <summary>
        /// Construtor do controlador que injeta o contexto da base de dados.
        /// </summary>
        /// <param name="context">Instância do contexto da base de dados.</param>
        public RegistoManutencaoController(LogisControlContext context, ManutencaoService manutencaoService)
        {
            _context = context;
            _manutencaoService = manutencaoService;
        }

        #region ObterRegistos
        /// <summary>
        /// Obtém a lista de todos os registos de manutenção.
        /// </summary>
        /// <returns>Lista de registos de manutenção.</returns>
        /// <response code="200">Retorna a lista de registos com sucesso.</response>
        /// <response code="500">Erro interno ao tentar obter os registos.</response>
        [HttpGet("ObterRegistos")]
        [Authorize(Roles = "Tecnico")]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<RegistoManutencaoDetalheDTO>>> GetRegistos()
        {
            try
            {
                var registos = await _context.RegistosManutencao
                    .Select(r => new RegistoManutencaoDetalheDTO
                    {
                        RegistoManutencaoId = r.RegistoManutencaoId,
                        Descricao = r.Descricao,
                        Estado = r.Estado,
                        PedidoManutencaoPedidoManutId = r.PedidoManutencaoPedidoManutId,
                        UtilizadorUtilizadorId = r.UtilizadorUtilizadorId,
                        AssistenciaExternaAssistenteId = r.AssistenciaExternaAssistenteId
                    })
                    .ToListAsync();

                return Ok(registos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao obter registos de manutenção: {ex.Message}");
            }
        }
        #endregion

        #region ObterRegistoPorId
        /// <summary>
        /// Obtém um registo de manutenção pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único do registo.</param>
        /// <returns>Dados do registo correspondente.</returns>
        /// <response code="200">Registo encontrado com sucesso.</response>
        /// <response code="404">Registo não encontrado.</response>
        /// <response code="500">Erro interno ao procurar o registo.</response>
        [HttpGet("ObterRegisto/{id}")]
        [Authorize(Roles = "Tecnico")]
        [Produces("application/json")]
        public async Task<ActionResult<RegistoManutencaoDetalheDTO>> GetRegistoPorId(int id)
        {
            try
            {
                var registo = await _context.RegistosManutencao
                    .Where(r => r.RegistoManutencaoId == id)
                    .Select(r => new RegistoManutencaoDetalheDTO
                    {
                        RegistoManutencaoId = r.RegistoManutencaoId,
                        Descricao = r.Descricao,
                        Estado = r.Estado,
                        PedidoManutencaoPedidoManutId = r.PedidoManutencaoPedidoManutId,
                        UtilizadorUtilizadorId = r.UtilizadorUtilizadorId,
                        AssistenciaExternaAssistenteId = r.AssistenciaExternaAssistenteId
                    })
                    .FirstOrDefaultAsync();

                if (registo == null)
                    return NotFound("Registo de manutenção não encontrado.");

                return Ok(registo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter registo: {ex.Message}");
            }
        }
        #endregion

        #region CriarRegisto
        /// <summary>
        /// Cria um novo registo de manutenção.
        /// </summary>
        /// <param name="novoRegistoDto">Dados para criação do registo.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="201">Registo criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="500">Erro interno ao criar o registo.</response>
        [HttpPost("CriarRegisto")]
        [Authorize(Roles = "Tecnico")]
        [Produces("application/json")]
        public async Task<ActionResult> CriarRegisto([FromBody] RegistoManutencaoDTO novoRegistoDto)
        {
            try
            {

                var idClaim = User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int utilizadorId))
                    return Unauthorized("Não foi possível identificar o utilizador.");

                var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
                if (utilizador == null)
                    return BadRequest("Utilizador não encontrado.");

                var novoRegisto = new RegistoManutencao
                {
                    Descricao = novoRegistoDto.Descricao,
                    Estado = novoRegistoDto.Estado,
                    PedidoManutencaoPedidoManutId = novoRegistoDto.PedidoManutencaoPedidoManutId,
                    UtilizadorUtilizadorId = utilizadorId,
                    AssistenciaExternaAssistenteId = novoRegistoDto.AssistenciaExternaAssistenteId 
                };

                _context.RegistosManutencao.Add(novoRegisto);
                await _context.SaveChangesAsync();
                await _manutencaoService.AtualizarEstadoPedidoSeRegistoResolvido(novoRegisto.RegistoManutencaoId);

                return StatusCode(201, "Registo de manutenção criado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao criar registo de manutenção: {ex.Message}");
            }
        }
        #endregion

        #region AtualizarRegisto
        /// <summary>
        /// Atualiza os dados de um registo de manutenção existente.
        /// </summary>
        /// <param name="registoId">ID do registo a atualizar.</param>
        /// <param name="registoAtualizado">Dados atualizados do registo.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Registo atualizado com sucesso.</response>
        /// <response code="404">Registo não encontrado.</response>
        /// <response code="500">Erro interno ao tentar atualizar o registo.</response>
        [HttpPut("AtualizarRegisto/{registoId}")]
        [Authorize(Roles = "Tecnico")]
        [Produces("application/json")]
        public async Task<IActionResult> AtualizarRegisto(int registoId, [FromBody] RegistoManutencaoDTO registoAtualizado)
        {
            try
            {
                var registo = await _context.RegistosManutencao.FindAsync(registoId);

                if (registo == null)
                    return NotFound("Registo de manutenção não encontrado.");

                // Obter ID do utilizador autenticado
                var idClaim = User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int utilizadorId))
                    return Unauthorized("Não foi possível identificar o utilizador.");

                var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
                if (utilizador == null)
                    return BadRequest("Utilizador não encontrado.");

                // Atualizar os campos
                registo.Descricao = registoAtualizado.Descricao;
                registo.Estado = registoAtualizado.Estado;
                registo.PedidoManutencaoPedidoManutId = registoAtualizado.PedidoManutencaoPedidoManutId;
                registo.UtilizadorUtilizadorId = utilizadorId;
                registo.AssistenciaExternaAssistenteId = registoAtualizado.AssistenciaExternaAssistenteId;

                await _context.SaveChangesAsync();

                return Ok("Registo de manutenção atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar registo de manutenção: {ex.Message}");
            }
        }
        #endregion

        #region ObterRegistosPorPedido
        /// <summary>
        /// Obtém todos os registos de manutenção associados a um pedido específico.
        /// </summary>
        /// <param name="pedidoId">ID do pedido de manutenção.</param>
        /// <returns>Lista de registos (descrição e estado).</returns>
        /// <response code="200">Registos obtidos com sucesso.</response>
        /// <response code="404">Nenhum registo encontrado para o pedido.</response>
        /// <response code="500">Erro interno ao obter os registos.</response>
        [HttpGet("ObterRegistosPorPedido/{pedidoId}")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<RegistoManutencaoVisualDTO>>> ObterRegistosPorPedido(int pedidoId)
        {
            try
            {
                var registos = await _context.RegistosManutencao
                    .Where(r => r.PedidoManutencaoPedidoManutId == pedidoId)
                    .Select(r => new RegistoManutencaoVisualDTO
                    {
                        RegistoManutencaoId = r.RegistoManutencaoId,
                        Descricao = r.Descricao,
                        Estado = r.Estado
                    })
                    .ToListAsync();
                return Ok(registos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter registos: {ex.Message}");
            }
        }
        #endregion
    }
}
