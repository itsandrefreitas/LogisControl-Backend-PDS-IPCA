using LogisControlAPI.Models;
using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Services;

namespace LogisControlAPI.Controllers
{

    /// <summary>
    /// Controlador responsável pela gestão das assistências externas.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoAquisicaoController : Controller
    {

        private readonly LogisControlContext _context;
        private readonly UtilizadorService _utilizadorService;

        public PedidoAquisicaoController(LogisControlContext context, UtilizadorService utilizadorService)
        {
            _context = context;
            _utilizadorService = utilizadorService;
        }

        #region Criar Pedido Aquisicao
        /// <response code="400">Utilizador não encontrado ou dados inválidos.</response>
        /// <response code="500">Erro interno ao criar o pedido de compra.</response>
        [HttpPost("CriarPedidoAquisicao")]
        [Authorize()]
        [Produces("application/json")]
        public async Task<IActionResult> CriarPedidoAquisicao([FromBody] CriarPedidoAquisicaoDTO dto)
        {
            try
            {

                var idClaim = User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int utilizadorId))
                    return Unauthorized("Não foi possível identificar o utilizador.");

                var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
                if (utilizador == null)
                    return BadRequest("Utilizador não encontrado.");

                var novoPedido = new PedidoCompra
                {
                    Descricao = dto.Descricao,
                    Estado = "Pendente",
                    DataAbertura = DateTime.UtcNow,
                    UtilizadorUtilizadorId = utilizadorId
                };

                _context.PedidosCompra.Add(novoPedido);
                await _context.SaveChangesAsync();

                return Ok(new { novoPedido.PedidoCompraId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao criar pedido de compra: {ex.Message}");
            }
        }
        #endregion

        #region AtualizarEstadoPedidoAquisicao 
        /// <summary>  
        /// Atualiza o estado de um pedido de compra. Se o estado for "Aceite" ou "Recusado", define também a data de conclusão.  
        /// </summary>  
        /// <response code="400">Dados inválidos.</response>  
        /// <response code="404">Pedido não encontrado.</response>  
        /// <response code="500">Erro interno ao atualizar o estado.</response>  
        [HttpPut("AtualizarEstadoAquisicao/{pedidoId}")]
        [Authorize(Roles = "Gestor")]
        [Produces("application/json")]
        public async Task<IActionResult> AtualizarEstadoPedidoAquisicao(int pedidoId, [FromBody] AtualizarEstadoPedidoDTO dto)
        {
            try
            {
                // 1. Carregar o pedido da base de dados
                var pedido = await _context.PedidosCompra.FindAsync(pedidoId);

                if (pedido == null)
                    return NotFound("Pedido de aquisição não encontrado.");

                // 2. Atualizar o estado
                pedido.Estado = dto.Estado;

                // 3. Atualizar a data de conclusão se for Aceite ou Recusado
                if (dto.Estado == "Aceite" || dto.Estado == "Recusado")
                {
                    pedido.DataConclusao = DateTime.UtcNow;
                }
                else
                {
                    pedido.DataConclusao = null; // limpa a data se mudar para estado anterior
                }

                // 4. Guardar alterações
                await _context.SaveChangesAsync();

                return Ok("Estado do pedido atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar o estado do pedido: {ex.Message}");
            }
        }
        #endregion

        #region AtualizarDescricaoPedidoAquisicao
        /// <response code="404">Pedido não encontrado.</response>
        /// <response code="500">Erro interno.</response>
        [HttpPut("AtualizarDescricaoAquisicao/{pedidoId}")]
        [Authorize]
        [Produces("application/json")]
        public async Task<IActionResult> AtualizarDescricaoAquisicao(int pedidoId, [FromBody] AtualizarDescricaoPedidoDTO dto)
        {
            try
            {
                var pedido = await _context.PedidosCompra.FindAsync(pedidoId);
                if (pedido == null)
                    return NotFound("Pedido de aquisição não encontrado.");

                pedido.Descricao = dto.NovaDescricao;
                await _context.SaveChangesAsync();

                return Ok("Descrição atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar a descrição: {ex.Message}");
            }
        }
        #endregion

        #region ListarPedidoAqusicaoPorUtilizador
        /// <summary>
        /// Lista os pedidos de compra do utilizador autenticado.
        /// </summary>
        /// <returns>Lista de pedidos de compra do utilizador.</returns>
        /// <response code="200">Lista obtida com sucesso.</response>
        /// <response code="401">Utilizador não autenticado.</response>
        /// <response code="500">Erro ao obter os pedidos.</response>
        [HttpGet("ListarPedidoAquisicaoPorUtilizador")]
        [Authorize()]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<PedidoCompraDTO>>> ListarPedidosAquisicaoPorUtilizador()
        {
            try
            {
                // Obter o ID do token JWT
                var idClaim = User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int utilizadorId))
                    return Unauthorized("Não foi possível identificar o utilizador.");

                var pedidos = await _context.PedidosCompra
                    .Include(p => p.UtilizadorUtilizador)
                    .Where(p => p.UtilizadorUtilizadorId == utilizadorId)
                    .Select(p => new PedidoCompraDTO
                    {
                        PedidoCompraId = p.PedidoCompraId,
                        Descricao = p.Descricao,
                        Estado = p.Estado,
                        DataAbertura = p.DataAbertura,
                        DataConclusao = p.DataConclusao,
                        NomeUtilizador = $"{p.UtilizadorUtilizador.PrimeiroNome} {p.UtilizadorUtilizador.Sobrenome}"
                    })
                    .ToListAsync();

                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter pedidos de compra: {ex.Message}");
            }
        }
        #endregion

        #region ListarPedidosAquisicaoPorRole
        /// <summary>
        /// Lista os pedidos de aquisição feitos por utilizadores com um determinado Role.
        /// </summary>
        /// <param name="role">O role dos utilizadores cujos pedidos queremos listar (ex: "Tecnico", "Compras").</param>
        /// <returns>Lista de pedidos de aquisição filtrados pelo role.</returns>
        /// <response code="200">Lista obtida com sucesso.</response>
        /// <response code="400">Role inválido ou não encontrado.</response>
        /// <response code="500">Erro interno ao obter os pedidos.</response>
        [HttpGet("ListarPedidosAquisicaoPorRole")]
        [Authorize(Roles = "Gestor")] 
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<PedidoCompraDTO>>> ListarPedidosAquisicaoPorRole()
        {
            try
            {
                var pedidos = await _context.PedidosCompra
                    .Include(p => p.UtilizadorUtilizador)
                    .Where(p => p.UtilizadorUtilizador.Role == "Tecnico") // Aqui é fixo "Tecnico"
                    .Select(p => new PedidoCompraDTO
                    {
                        PedidoCompraId = p.PedidoCompraId,
                        Descricao = p.Descricao,
                        Estado = p.Estado,
                        DataAbertura = p.DataAbertura,
                        DataConclusao = p.DataConclusao,
                        NomeUtilizador = $"{p.UtilizadorUtilizador.PrimeiroNome} {p.UtilizadorUtilizador.Sobrenome}"
                    })
                    .ToListAsync();

                return Ok(pedidos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter pedidos de aquisição: {ex.Message}");
            }
        }
        #endregion



    }
}
