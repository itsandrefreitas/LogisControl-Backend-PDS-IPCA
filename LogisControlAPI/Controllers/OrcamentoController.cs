using LogisControlAPI.DTO;
using LogisControlAPI.Models;
using LogisControlAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Services;

namespace LogisControlAPI.Controllers
{
    [ApiController]
    [Route("api/orcamentos")]
    public class OrcamentoController : ControllerBase
    {
        private readonly LogisControlContext _ctx;
        private readonly ComprasService _comprasService;

        public OrcamentoController(LogisControlContext ctx, ComprasService comprasService)
        {
            _ctx = ctx;
            _comprasService = comprasService;
        }

        /// <summary>
        /// 1) Fornecedor cria um orçamento (cabeçalho) para um pedido de cotação.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CriarOrcamento([FromBody] OrcamentoDTO dto)
        {
            // 1.1) valida FK em PedidoCotacao
            var existePc = await _ctx.PedidosCotacao
                .AnyAsync(pc => pc.PedidoCotacaoId == dto.PedidoCotacaoID);
            if (!existePc)
                return BadRequest("Pedido de cotação inválido.");

            // 1.2) cria o cabeçalho
            var orc = new Orcamento
            {
                PedidoCotacaoPedidoCotacaoID = dto.PedidoCotacaoID,
                Data = DateTime.UtcNow,
                Estado = "Respondido"
            };
            _ctx.Orcamentos.Add(orc);
            await _ctx.SaveChangesAsync();

            // 1.3) devolve 201 Created com Location para GET /api/orcamentos/{orcId}
            return CreatedAtAction(
                nameof(ObterOrcamento),
                new { orcId = orc.OrcamentoID },
                new { OrcamentoId = orc.OrcamentoID }
            );
        }

        /// <summary>
        /// 2) Fornecedor adiciona linhas ao orçamento.
        /// </summary>
        [HttpPost("{orcId:int}/itens")]
        [ProducesResponseType(201)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> AdicionarItem(
            [FromRoute] int orcId,
            [FromBody] CriarOrcamentoItemDTO dto)
        {
            try
            {
                if (!await _ctx.Orcamentos.AnyAsync(o => o.OrcamentoID == orcId))
                    return NotFound("Orçamento não encontrado.");

                var item = new OrcamentoItem
                {
                    OrcamentoOrcamentoID = orcId,
                    MateriaPrimaID = dto.MateriaPrimaID,
                    Quantidade = dto.Quantidade,
                    PrecoUnit = dto.PrecoUnit,
                    PrazoEntrega = dto.PrazoEntrega
                };

                _ctx.OrcamentosItem.Add(item);

                // Atualiza o estado do orçamento
                var orcamento = await _ctx.Orcamentos
        .Include(o => o.PedidoCotacaoPedidoCotacao)
        .FirstOrDefaultAsync(o => o.OrcamentoID == orcId);

                if (orcamento == null)
                    return NotFound("Orçamento não encontrado.");

                orcamento.Estado = "Respondido";

                var cotacao = orcamento.PedidoCotacaoPedidoCotacao;
                if (cotacao != null)
                {
                    cotacao.Estado = "ComOrcamentos";

                    var pedidoCompra = await _ctx.PedidosCompra
                        .FirstOrDefaultAsync(p => p.PedidoCompraId == cotacao.PedidoCompraId);

                    if (pedidoCompra != null && pedidoCompra.Estado == "EmCotacao")
                    {
                        pedidoCompra.Estado = "ComOrcamentos";
                    }
                }

                Console.WriteLine("Orçamento recebido:");
                Console.WriteLine($"OrcamentoID: {orcId}");
                Console.WriteLine($"MateriaPrimaID: {dto.MateriaPrimaID}");
                Console.WriteLine($"PedidoCotacaoID: {orcamento.PedidoCotacaoPedidoCotacaoID}");
                Console.WriteLine($"Cotacao FK: {(orcamento.PedidoCotacaoPedidoCotacao != null ? "ok" : "null")}");

                await _ctx.SaveChangesAsync();

                return CreatedAtAction(nameof(ObterOrcamento), new { orcId }, new { item.OrcamentoItemID });
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERRO INTERNO:");
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Erro interno: " + ex.Message);
            }
        }

        /// <summary>
        /// 3) Recupera um orçamento e todos os seus itens.
        /// </summary>
        [HttpGet("{orcId:int}")]
        [ProducesResponseType(typeof(OrcamentoDetalheDTO), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ObterOrcamento([FromRoute] int orcId)
        {
            var orc = await _ctx.Orcamentos
                .Include(o => o.OrcamentoItems)
                    .ThenInclude(it => it.MateriaPrima)
                .Include(o => o.PedidoCotacaoPedidoCotacao)
                .FirstOrDefaultAsync(o => o.OrcamentoID == orcId);

            if (orc == null)
                return NotFound();

            var detalhe = new OrcamentoDetalheDTO
            {
                OrcamentoID = orc.OrcamentoID,
                PedidoCotacaoID = orc.PedidoCotacaoPedidoCotacaoID,
                Data = orc.Data,
                Estado = orc.Estado,
                Itens = orc.OrcamentoItems.Select(i => new OrcamentoItemDetalheDTO
                {
                    OrcamentoItemID = i.OrcamentoItemID,
                    MateriaPrimaID = i.MateriaPrimaID,
                    MateriaPrimaNome = i.MateriaPrima.Nome,
                    Quantidade = i.Quantidade,
                    PrecoUnit = i.PrecoUnit,
                    PrazoEntrega = i.PrazoEntrega ?? 0
                }).ToList()
            };

            return Ok(detalhe);
        }


        /// <summary>
        /// Aceita um orçamento e recusa todos os outros do mesmo pedido de cotação.
        /// </summary>
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [HttpPost("{orcId:int}/aceitar")]
        public async Task<IActionResult> AceitarOrcamento([FromRoute] int orcId)
        {
            try
            {
                var notaId = await _comprasService.AceitarOrcamentoAsync(orcId);
                return Ok(new { Mensagem = "Orçamento aceite com sucesso", NotaEncomendaId = notaId });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Orçamento não encontrado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERRO AO ACEITAR ORÇAMENTO:");
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Erro interno ao aceitar orçamento.");
            }
        }
    }
}