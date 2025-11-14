using Microsoft.AspNetCore.Mvc;
using LogisControlAPI.Models;
using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LogisControlAPI.Controllers
{

    /// <summary>
    /// Controlador responsável pela gestão dos clientes.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class EncomendaClienteController : ControllerBase
    {
        private readonly LogisControlContext _context;

        /// <summary>
        /// Construtor do controlador que injeta o contexto da base de dados.
        /// </summary>
        /// <param name="context">Instância do contexto da base de dados.</param>
        public EncomendaClienteController(LogisControlContext context)
        {
            _context = context;
        }


        #region ListarEncomenda
        /// <summary>
        /// Lista encomendas
        /// </summary>
        /// <returns>Lista de encomendas.</returns>
        /// <response code="200">Lista obtida com sucesso.</response>
        /// <response code="500">Erro ao obter encomendas.</response>
        [HttpGet("ListarEncomendas")]
        public async Task<ActionResult<IEnumerable<EncomendaClienteDTO>>> ListarEncomendas()
        {
            try
            {
                var encomendas = await _context.EncomendasCliente
                    .Include(e => e.ClienteCliente)
                    .Select(e => new EncomendaClienteDTO
                    {
                        EncomendaClienteId = e.EncomendaClienteId,
                        DataEncomenda = e.DataEncomenda,
                        Estado = e.Estado,
                        NomeCliente = e.ClienteCliente.Nome
                    })
                    .ToListAsync();

                return Ok(encomendas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter encomendas: {ex.Message}");
            }
        }
        #endregion

        #region CriarEncomendaCliente
        /// <summary>
        /// Cria uma nova encomenda de cliente.
        /// </summary>
        /// <param name="dto">Dados da encomenda a ser criada.</param>
        /// <returns>Encomenda criada ou mensagem de erro.</returns>
        /// <response code="201">Encomenda criada com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="500">Erro interno ao criar a encomenda.</response>
        [HttpPost("CriarEncomenda")]
        public async Task<ActionResult> CriarEncomendaCliente([FromBody] EncomendaClienteDTO dto)
        {
            try
            {
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.Nome.ToLower() == dto.NomeCliente.ToLower());

                if (cliente == null)
                    return BadRequest("Cliente não encontrado pelo nome informado.");

                var novaEncomenda = new EncomendaCliente
                {
                    ClienteClienteId = cliente.ClienteId,
                    DataEncomenda = DateTime.Now,
                    Estado = dto.Estado
                };

                _context.EncomendasCliente.Add(novaEncomenda);
                await _context.SaveChangesAsync();

                // Devolver o ID da encomenda criada para o frontend
                return CreatedAtAction(nameof(CriarEncomendaCliente), new { id = novaEncomenda.EncomendaClienteId }, new
                {
                    mensagem = "Encomenda criada com sucesso.",
                    encomendaClienteId = novaEncomenda.EncomendaClienteId
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao criar encomenda: {ex.Message}");
            }
        }
        #endregion


        #region AtualizarEstado
        /// <summary>
        /// Atualiza manualmente o estado de uma encomenda.
        /// </summary>
        /// <param name="id">ID da encomenda a atualizar.</param>
        /// <param name="dto">Novo estado da encomenda.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Estado atualizado com sucesso.</response>
        /// <response code="404">Encomenda não encontrada.</response>
        /// <response code="500">Erro interno ao atualizar o estado.</response>
        [HttpPut("AtualizarEstado/{id}")]
        public async Task<IActionResult> AtualizarEstado(int id, [FromBody] AtualizarEstadoEncomendaDTO dto)
        {
            try
            {
                var encomenda = await _context.EncomendasCliente.FindAsync(id);
                if (encomenda == null)
                    return NotFound("Encomenda não encontrada.");

                encomenda.Estado = dto.Estado;
                await _context.SaveChangesAsync();

                return Ok("Estado da encomenda atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar estado: {ex.Message}");
            }
        }

        #endregion


    }
}
