using LogisControlAPI.DTO;
using LogisControlAPI.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/pedidos-cotacao")]
public class PedidoCotacaoController : ControllerBase
{
    private readonly ComprasService _service;
    public PedidoCotacaoController(ComprasService service) => _service = service;

    /// <summary>
    /// Cria um pedido de cotação para um pedido de compra existente,
    /// atribuindo-o a um fornecedor e gera um token de acesso.
    /// </summary>
    [HttpPost("{pedidoCompraId}/cotacao")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> CriarPedidoCotacao(
        [FromRoute] int pedidoCompraId,
        [FromQuery] int fornecedorId)
    {
        Console.WriteLine("Entrou em CriarPedidoCotacao");
        try
        {
            var (cotacaoId, token) = await _service
                .CriarPedidoCotacaoAsync(pedidoCompraId, fornecedorId);

            // 201 Created + Location (incluindo token) + body com token
            return CreatedAtAction(
                nameof(ObterCotacaoFornecedor),
                new { id = cotacaoId, token = token }, 
                new
                {
                    PedidoCotacaoId = cotacaoId,
                    TokenAcesso = token
                }
            );
        }
        catch (KeyNotFoundException knf)
        {
            return NotFound(knf.Message);
        }
        catch (InvalidOperationException io)
        {
            return BadRequest(io.Message);
        }
    }

    /// <summary>
    /// Retorna o pedido de cotação com seus orçamentos e itens.
    /// </summary>
    // Para o DEPARTAMENTO DE COMPRAS (sem token)
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PedidoCotacaoDetalhadoDTO), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterCotacaoAdmin([FromRoute] int id)
    {
        try
        {
            var dto = await _service.ObterPedidoCotacaoDetalhadoAsync(id);
            return Ok(dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Retorna o pedido de cotação com seus orçamentos e itens.
    /// </summary>
    // Para o FORNECEDOR (com token)
    [HttpGet("{id:int}/fornecedor")]
    [ProducesResponseType(typeof(PedidoCotacaoDetalhadoDTO), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterCotacaoFornecedor([FromRoute] int id, [FromQuery] string token)
    {
        try
        {
            var dto = await _service.ObterPedidoCotacaoParaFornecedorAsync(id, token);
            return Ok(dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Token inválido");
        }
    }

    /// <summary>
    /// Obtém o ID do pedido de cotação mais recente associado a um pedido de compra.
    /// </summary>
    /// <param name="pedidoCompraId">ID do pedido de compra.</param>
    /// <returns>ID do pedido de cotação.</returns>
    [HttpGet("por-compra/{pedidoCompraId}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ObterCotacaoPorPedidoCompraId([FromRoute] int pedidoCompraId)
    {
        var cotacao = await _service.ObterCotacaoPorPedidoCompraAsync(pedidoCompraId);

        if (cotacao == null)
            return NotFound("Não foi encontrada nenhuma cotação associada a este pedido de compra.");

        return Ok(new { pedidoCotacaoId = cotacao.PedidoCotacaoId });
    }

}