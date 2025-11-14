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
    public class ClienteController : ControllerBase
    {
        private readonly LogisControlContext _context;

        /// <summary>
        /// Construtor do controlador que injeta o contexto da base de dados.
        /// </summary>
        /// <param name="context">Instância do contexto da base de dados.</param>
        public ClienteController(LogisControlContext context)
        {
            _context = context;
        }

        #region ObterClientes
        /// <summary>
        /// Obtém a lista de todos os clientes registados.
        /// </summary>
        /// <returns>Lista de clientes sem encomendas associadas.</returns>
        /// <response code="200">Retorna a lista de clientes com sucesso.</response>
        /// <response code="500">Erro interno ao tentar obter os clientes.</response>
        [HttpGet("ObterClientes")]
        public async Task<ActionResult<IEnumerable<ClienteDTO>>> GetClientes()
        {
            try
            {
                var clientes = await _context.Clientes
                    .Select(c => new ClienteDTO
                    {
                        ClienteId = c.ClienteId,
                        Nome = c.Nome,
                        Nif = c.Nif,
                        Morada = c.Morada
                    })
                    .ToListAsync();

                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao obter clientes: {ex.Message}");
            }
        }
        #endregion

        #region ObterClientePorNif
        /// <summary>
        /// Obtém um cliente pelo seu NIF.
        /// </summary>
        /// <param name="nif">Número de Identificação Fiscal (NIF) do cliente.</param>
        /// <returns>Dados do cliente correspondente.</returns>
        /// <response code="200">Cliente encontrado com sucesso.</response>
        /// <response code="404">Cliente não encontrado.</response>
        /// <response code="500">Erro interno ao procurar o cliente.</response>
        [HttpGet("ObterCliente/{nif}")]
        public async Task<ActionResult<ClienteDTO>> GetClientePorNif(int nif)
        {
            try
            {
                var cliente = await _context.Clientes
                    .Where(c => c.Nif == nif)
                    .Select(c => new ClienteDTO
                    {
                        ClienteId = c.ClienteId,
                        Nome = c.Nome,
                        Nif = c.Nif,
                        Morada = c.Morada
                    })
                    .FirstOrDefaultAsync();

                if (cliente == null)
                    return NotFound("Cliente não encontrado.");

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter cliente: {ex.Message}");
            }
        }
        #endregion

        #region CriarCliente
        /// <summary>
        /// Cria um novo cliente.
        /// </summary>
        /// <param name="dto">Dados para criação do pedido.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="201">Cliente criado com sucesso.</response>
        /// <response code="500">Erro interno ao criar o pedido de compra.</response>
        [HttpPost("CriarCliente")]
        public async Task<ActionResult> CriarCliente([FromBody] CriarClienteDTO novoClienteDto)
        {
            try
            {
                // Verifica se já existe cliente com o mesmo NIF
                var clienteExistente = await _context.Clientes.AnyAsync(c => c.Nif == novoClienteDto.Nif);
                if (clienteExistente)
                    return BadRequest("Já existe um cliente com o mesmo NIF.");

                var novoCliente = new Cliente
                {
                    Nome = novoClienteDto.Nome,
                    Nif = novoClienteDto.Nif,
                    Morada = novoClienteDto.Morada
                };

                _context.Clientes.Add(novoCliente);
                await _context.SaveChangesAsync();

                return StatusCode(201, "Cliente criado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao criar cliente: {ex.Message}");
            }
        }
        #endregion

        #region AtualizarCliente
        /// <summary>
        /// Atualiza os dados de um cliente existente.
        /// </summary>
        /// <param name="clienteId">ID do cliente a atualizar.</param>
        /// <param name="clienteAtualizado">Dados atualizados do cliente.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Cliente atualizado com sucesso.</response>
        /// <response code="404">Cliente não encontrado.</response>
        /// <response code="400">NIF duplicado ou dados inválidos.</response>
        /// <response code="500">Erro interno ao tentar atualizar o cliente.</response>
        [HttpPut("AtualizarCliente/{clienteId}")]
        public async Task<IActionResult> AtualizarCliente(int clienteId, [FromBody] AtualizarClienteDTO clienteAtualizado)
        {
            try
            {
                var cliente = await _context.Clientes.FindAsync(clienteId);

                if (cliente == null)
                    return NotFound("Cliente não encontrado.");

                // Verificar se o novo NIF já pertence a outro cliente
                bool nifDuplicado = await _context.Clientes
                    .AnyAsync(c => c.Nif == clienteAtualizado.Nif && c.ClienteId != clienteId);

                if (nifDuplicado)
                    return BadRequest("Já existe outro cliente com o mesmo NIF.");

                // Atualizar os campos
                cliente.Nome = clienteAtualizado.Nome;
                cliente.Nif = clienteAtualizado.Nif;
                cliente.Morada = clienteAtualizado.Morada;

                await _context.SaveChangesAsync();

                return Ok("Cliente atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar cliente: {ex.Message}");
            }
        }
        #endregion

    }
}