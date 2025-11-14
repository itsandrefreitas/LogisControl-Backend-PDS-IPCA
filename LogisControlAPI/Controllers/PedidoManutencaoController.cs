using Microsoft.AspNetCore.Mvc;
using LogisControlAPI.Models;
using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using LogisControlAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LogisControlAPI.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão dos pedidos de manutenção.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoManutencaoController : ControllerBase
    {
        private readonly LogisControlContext _context;
        private readonly ManutencaoService _manutencaoService;

        /// <summary>
        /// Construtor do controlador que injeta o contexto da base de dados.
        /// </summary>
        /// <param name="context">Instância do contexto da base de dados.</param>
        public PedidoManutencaoController(LogisControlContext context, ManutencaoService manutencaoService)
        {
            _context = context;
            _manutencaoService = manutencaoService;
        }

        #region ObterPedidos
        /// <summary>
        /// Obtém a lista de todos os pedidos de manutenção registados.
        /// </summary>
        /// <returns>Lista de pedidos de manutenção.</returns>
        /// <response code="200">Retorna a lista de pedidos com sucesso.</response>
        /// <response code="500">Erro interno ao tentar obter os pedidos.</response>
        [HttpGet("ObterPedidos")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<PedidoManutençãoDTO>>> GetPedidos()
        {
            try
            {
                var pedidos = await _context.PedidosManutencao
                    .Include(p => p.MaquinaMaquina)
                    .Include(p => p.UtilizadorUtilizador)
                    .Select(p => new PedidoManutençãoDTO
                    {
                        PedidoManutId = p.PedidoManutId,
                        Descricao = p.Descricao,
                        Estado = p.Estado,
                        DataAbertura = p.DataAbertura,
                        DataConclusao = p.DataConclusao,
                        MaquinaMaquinaId = p.MaquinaMaquinaId,
                        UtilizadorUtilizadorId = p.UtilizadorUtilizadorId,
                        MaquinaNome = p.MaquinaMaquina.Nome,
                        UtilizadorNome = p.UtilizadorUtilizador.PrimeiroNome + " " + p.UtilizadorUtilizador.Sobrenome
                    })
                    .ToListAsync();

                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao obter pedidos de manutenção: {ex.Message}");
            }
        }
        #endregion

        #region ListarPedidosManutencaoPorUtilizador
        /// <summary>
        /// Lista os pedidos de manutenção do utilizador autenticado.
        /// </summary>
        /// <returns>Lista de pedidos de manutenção do utilizador.</returns>
        /// <response code="200">Lista obtida com sucesso.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        /// <response code="500">Erro ao obter os pedidos.</response>
        [HttpGet("ListarPedidosManutencaoPorUtilizador")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<PedidoManutençãoDTO>>> ListarPedidosManutencaoPorUtilizador()
        {
            try
            {
                // Obter o ID do token JWT
                var idClaim = User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int utilizadorId))
                    return Unauthorized("Não foi possível identificar o utilizador.");

                var pedidos = await _context.PedidosManutencao
                    .Include(p => p.UtilizadorUtilizador)
                    .Include(p => p.MaquinaMaquina)
                    .Where(p => p.UtilizadorUtilizadorId == utilizadorId)
                    .Select(p => new PedidoManutençãoDTO
                    {
                        PedidoManutId = p.PedidoManutId,
                        Descricao = p.Descricao,
                        Estado = p.Estado,
                        DataAbertura = p.DataAbertura,
                        DataConclusao = p.DataConclusao,
                        MaquinaMaquinaId = p.MaquinaMaquinaId,
                        UtilizadorUtilizadorId = p.UtilizadorUtilizadorId,
                        MaquinaNome = p.MaquinaMaquina.Nome,
                        UtilizadorNome = $"{p.UtilizadorUtilizador.PrimeiroNome} {p.UtilizadorUtilizador.Sobrenome}"
                    })
                    .ToListAsync();

                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter pedidos de manutenção: {ex.Message}");
            }
        }
        #endregion

        #region ObterPedidoPorId
        /// <summary>
        /// Obtém um pedido de manutenção pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único do pedido.</param>
        /// <returns>Dados do pedido de manutenção correspondente.</returns>
        /// <response code="200">Pedido encontrado com sucesso.</response>
        /// <response code="404">Pedido não encontrado.</response>
        /// <response code="500">Erro interno ao procurar o pedido.</response>
        [HttpGet("ObterPedido/{id}")]
        [Authorize]
        [Produces("application/json")]
        public async Task<ActionResult<PedidoManutençãoDTO>> GetPedidoPorId(int id)
        {
            try
            {
                var pedido = await _context.PedidosManutencao
                    .Where(p => p.PedidoManutId == id)
                    .Select(p => new PedidoManutençãoDTO
                    {
                        PedidoManutId = p.PedidoManutId,
                        Descricao = p.Descricao,
                        Estado = p.Estado,
                        DataAbertura = p.DataAbertura,
                        DataConclusao = p.DataConclusao,
                        MaquinaMaquinaId = p.MaquinaMaquinaId,
                        UtilizadorUtilizadorId = p.UtilizadorUtilizadorId
                    })
                    .FirstOrDefaultAsync();

                if (pedido == null)
                    return NotFound("Pedido de manutenção não encontrado.");

                return Ok(pedido);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter pedido: {ex.Message}");
            }
        }
        #endregion

        #region CriarPedido
        /// <summary>
        /// Cria um novo pedido de manutenção e envia notificação por Telegram.
        /// </summary>
        /// <param name="novoPedidoDto">Dados para criação do pedido.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="201">Pedido criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="500">Erro interno ao criar o pedido.</response>
        [Authorize]
        [HttpPost("CriarPedido")]
        public async Task<ActionResult> CriarPedido([FromBody] PedidoManutençãoDTO novoPedidoDto)
        {
            try
            {
                var idClaim = User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int utilizadorId))
                    return Unauthorized("Não foi possível identificar o utilizador.");

                // Chamar o serviço
                await _manutencaoService.CriarPedidoAsync(novoPedidoDto, utilizadorId);

                return StatusCode(201, "Pedido de manutenção criado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao criar pedido: {ex.Message}");
            }
        }
        #endregion


        #region AtualizarPedido
        /// <summary>
        /// Atualiza os dados de um pedido de manutenção existente.
        /// </summary>
        /// <param name="pedidoId">ID do pedido a atualizar.</param>
        /// <param name="pedidoAtualizado">Dados atualizados do pedido.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Pedido atualizado com sucesso.</response>
        /// <response code="404">Pedido não encontrado.</response>
        /// <response code="500">Erro interno ao tentar atualizar o pedido.</response>
        [HttpPut("AtualizarPedido/{pedidoId}")]
        [Authorize(Roles = "Tecnico")]
        [Produces("application/json")]
        public async Task<IActionResult> AtualizarPedido(int pedidoId, [FromBody] PedidoManutençãoDTO pedidoAtualizado)
        {
            try
            {
                var pedido = await _context.PedidosManutencao.FindAsync(pedidoId);

                if (pedido == null)
                    return NotFound("Pedido de manutenção não encontrado.");

                // Atualizar campos que não são o Estado (porque o Estado vai pelo serviço)
                pedido.Descricao = pedidoAtualizado.Descricao;
                pedido.DataAbertura = pedidoAtualizado.DataAbertura;
                pedido.MaquinaMaquinaId = pedidoAtualizado.MaquinaMaquinaId;
                pedido.UtilizadorUtilizadorId = pedidoAtualizado.UtilizadorUtilizadorId;

                // Atualizar Estado e possivelmente DataConclusao através do serviço
                await _manutencaoService.AtualizarEstadoPedido(pedidoId, pedidoAtualizado.Estado);

                await _context.SaveChangesAsync();

                return Ok("Pedido de manutenção atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar pedido: {ex.Message}");
            }
        }
        #endregion

        #region Pedidos de Manutenção Atrasados

        /// <summary>
        /// Obtém os pedidos de manutenção que estão abertos há mais de 7 dias.
        /// </summary>
        /// <remarks>
        /// Apenas são devolvidos pedidos que ainda não se encontram resolvidos.
        /// A data de abertura é usada como referência para calcular o tempo em aberto.
        /// </remarks>
        /// <returns>Lista de pedidos de manutenção em atraso.</returns>
        /// <response code="200">Pedidos obtidos com sucesso.</response>
        /// <response code="500">Erro ao obter os pedidos.</response>
        [Authorize(Roles = "Gestor")]
        [Produces("application/json")]
        [HttpGet("PedidosAtrasados")]
        public async Task<IActionResult> ObterPedidosAtrasados()
        {
            try
            {
                var pedidos = await _manutencaoService.ObterPedidosAtrasadosAsync();
                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter pedidos atrasados: {ex.Message}");
            }
        }
        #endregion

        #region ReabrirPedido
        /// <summary>
        /// Reabre um pedido de manutenção, definindo o estado como "Em Espera",
        /// limpando a data de conclusão e adicionando uma justificação à descrição.
        /// Também envia uma notificação via Telegram.
        /// </summary>
        /// <param name="dto">DTO contendo o ID do pedido e a justificação.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Pedido reaberto com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="500">Erro ao processar o pedido.</response>
        [Authorize]
        [HttpPut("ReabrirPedido")]
        [Produces("application/json")]
        public async Task<IActionResult> ReabrirPedido([FromBody] ReabrirPedidoManutencaoDTO dto)
        {
            try
            {
                await _manutencaoService.ReabrirPedidoManutencaoAsync(dto);
                return Ok("Pedido de manutenção reaberto com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao reabrir pedido: {ex.Message}");
            }
        }
        #endregion
    }
}

