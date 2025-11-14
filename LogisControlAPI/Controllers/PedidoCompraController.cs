using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using LogisControlAPI.DTO;
using LogisControlAPI.Services;

namespace LogisControlAPI.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão dos pedidos de compra:
    /// criação, listagem e detalhe.
    /// </summary>
    [ApiController]
    [Route("api/pedidos-compra")]
    public class PedidoCompraController : ControllerBase
    {
        private readonly ComprasService _comprasService;

        /// <summary>
        /// Injeta o serviço de compras que contém toda a lógica de negócio.
        /// </summary>
        public PedidoCompraController(ComprasService comprasService)
        {
            _comprasService = comprasService;
        }

        /// <summary>
        /// Cria um novo pedido de compra, incluindo os itens especificados.
        /// </summary>
        /// <param name="dto">Dados do pedido, incluindo lista de itens.</param>
        /// <returns>201 Created com header Location apontando para o GET de detalhe.</returns>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CriarPedidoCompra([FromBody] CriarPedidoCompraDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var id = await _comprasService.CriarPedidoCompraAsync(dto);

            return CreatedAtAction(
                nameof(ObterPedidoCompra),
                new { id },
                null
            );
        }

        /// <summary>
        /// Lista todos os pedidos de compra filtrados por estado.
        /// </summary>
        /// <param name="estado">Estado para filtrar (p.ex. "Aberto").</param>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PedidoCompraDTO>), 200)]
        public async Task<IActionResult> ListarPedidosCompra([FromQuery] string estado = "Aberto")
        {
            var lista = await _comprasService.ListarPedidosPorEstadoAsync(estado);
            return Ok(lista);
        }

        /// <summary>
        /// Obtém o detalhe completo de um pedido de compra,
        /// incluindo informações do utilizador e itens.
        /// </summary>
        /// <param name="id">ID do pedido a consultar.</param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PedidoCompraDetalheDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ObterPedidoCompra(int id)
        {
            var detalhe = await _comprasService.ObterPedidoCompraDetalheAsync(id);
            if (detalhe == null)
                return NotFound($"Pedido de compra com ID={id} não encontrado.");

            return Ok(detalhe);
        }
    }
}