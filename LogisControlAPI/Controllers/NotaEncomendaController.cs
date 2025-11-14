using LogisControlAPI.Services;
using Microsoft.AspNetCore.Mvc;
using LogisControlAPI.DTO;
using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Data;
using LogisControlAPI.Models;

namespace LogisControlAPI.Controllers
{
    [ApiController]
    [Route("api/notas-encomenda")]
    public class NotaEncomendaController : ControllerBase
    {
        private readonly ComprasService _service;

        public NotaEncomendaController(ComprasService service)
        {
            _service = service;
        }

        /// <summary>
        /// Devolve todas as notas de encomenda que estão em estado "Pendente".
        /// </summary>
        [HttpGet("pendentes")]
        public async Task<IActionResult> ListarTodasPendentes()
        {
            var notas = await _service.ObterNotasPendentesAsync();
            return Ok(notas);
        }

        /// <summary>
        /// Regista a receção de uma nota de encomenda.
        /// Permite marcar como recebida com sucesso ou reclamada.
        /// </summary>
        [HttpPatch("{id}/receber")]
        public async Task<IActionResult> ReceberNota(int id, [FromBody] RececaoDTO dto)
        {
            try
            {
                await _service.ReceberNotaEncomendaAsync(id, dto.EmBoasCondicoes);
                return Ok(new { message = "Nota atualizada com sucesso." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Devolve os detalhes de uma nota de encomenda a partir do seu ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> ObterNotaEncomenda(int id)
        {
            try
            {
                var dto = await _service.ObterNotaEncomendaAsync(id);
                return Ok(dto);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Nota de encomenda não encontrada.");
            }
        }

        /// <summary>
        /// Obtém a nota de encomenda associada a um determinado orçamento.
        /// </summary>
        [HttpGet("por-orcamento/{orcamentoId}")]
        public async Task<IActionResult> ObterNotaPorOrcamento(int orcamentoId)
        {
            try
            {
                var dto = await _service.ObterNotaPorOrcamentoAsync(orcamentoId);
                if (dto == null)
                    return NotFound("Nota de encomenda não encontrada para este orçamento.");

                return Ok(dto);
            }
            catch
            {
                return StatusCode(500, "Erro interno ao obter a nota de encomenda.");
            }
        }

        /// <summary>
        /// Lista todas as notas de encomenda pendentes que contêm uma dada matéria-prima.
        /// </summary>
        [HttpGet("pendentes/por-materia/{materiaPrimaId}")]
        public async Task<IActionResult> ListarPendentesPorMateria(int materiaPrimaId)
        {
            try
            {
                var notas = await _service.ListarNotasPendentesPorMateriaAsync(materiaPrimaId);

                if (!notas.Any())
                    return NotFound("Nenhuma nota pendente encontrada para esta matéria-prima.");

                return Ok(notas);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro ao listar notas pendentes:", ex);
                return StatusCode(500, "Erro interno ao procurar notas pendentes.");
            }
        }

        /// <summary>
        /// Devolve todas as notas que estão atualmente em estado "Reclamada".
        /// </summary>
        [HttpGet("reclamadas")]
        public async Task<IActionResult> ListarNotasReclamadas()
        {
            var notas = await _service.ObterNotasPorEstadoAsync("Reclamada");
            return Ok(notas);
        }

        /// <summary>
        /// Envia um e-mail ao fornecedor a informar sobre uma reclamação.
        /// </summary>
        [HttpPost("{id}/enviar-email-substituicao")]
        public async Task<IActionResult> EnviarEmailSubstituicao(int id)
        {
            try
            {
                await _service.EnviarEmailReclamacaoFornecedorAsync(id);
                return Ok("Email enviado com sucesso.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Erro ao enviar email ao fornecedor.");
            }
        }

        /// <summary>
        /// Confirma que o fornecedor aceitou reenviar o material e cria nova nota.
        /// </summary>
        [HttpPost("{id}/nova-entrega")]
        public async Task<IActionResult> ConfirmarNovaEntrega(int id)
        {
            try
            {
                await _service.ConfirmarNovaEntregaAsync(id);
                return Ok("Entrega substituta registada. A nova nota será validada pelo operador.");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch
            {
                return StatusCode(500, "Erro ao registar nova entrega.");
            }
        }
    }
}
