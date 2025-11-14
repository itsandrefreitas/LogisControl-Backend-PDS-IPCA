using Microsoft.AspNetCore.Mvc;
using LogisControlAPI.Models;
using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


namespace LogisControlAPI.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão das máquinas.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MaquinaController : ControllerBase
    {
        private readonly LogisControlContext _context;

        /// <summary>
        /// Construtor do controlador que injeta o contexto da base de dados.
        /// </summary>
        /// <param name="context">Instância do contexto da base de dados.</param>
        public MaquinaController(LogisControlContext context)
        {
            _context = context;
        }

        #region ObterMaquinas
        /// <summary>
        /// Obtém a lista de todas as máquinas registadas.
        /// </summary>
        /// <returns>Lista de máquinas sem relações associadas.</returns>
        /// <response code="200">Retorna a lista de máquinas com sucesso.</response>
        /// <response code="500">Erro interno ao tentar obter as máquinas.</response>
        [HttpGet("ObterMaquinas")]
        [Authorize()]
        [Produces("application/json")]
        public async Task<ActionResult<IEnumerable<MaquinaDTO>>> GetMaquinas()
        {
            try
            {
                var maquinas = await _context.Maquinas
                    .Select(m => new MaquinaDTO
                    {
                        MaquinaId = m.MaquinaId,
                        Nome = m.Nome,
                        LinhaProd = m.LinhaProd,
                        AssistenciaExternaAssistenteId = m.AssistenciaExternaAssistenteId
                    })
                    .ToListAsync();

                return Ok(maquinas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao obter máquinas: {ex.Message}");
            }
        }
        #endregion

        #region ObterMaquinaPorId
        /// <summary>
        /// Obtém uma máquina pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único da máquina.</param>
        /// <returns>Dados da máquina correspondente.</returns>
        /// <response code="200">Máquina encontrada com sucesso.</response>
        /// <response code="404">Máquina não encontrada.</response>
        /// <response code="500">Erro interno ao procurar a máquina.</response>
        [HttpGet("ObterMaquina/{id}")]
        [Authorize(Roles = "Tecnico")]
        [Produces("application/json")]
        public async Task<ActionResult<MaquinaDTO>> GetMaquinaPorId(int id)
        {
            try
            {
                var maquina = await _context.Maquinas
                    .Where(m => m.MaquinaId == id)
                    .Select(m => new MaquinaDTO
                    {
                        MaquinaId = m.MaquinaId,
                        Nome = m.Nome,
                        LinhaProd = m.LinhaProd,
                        AssistenciaExternaAssistenteId = m.AssistenciaExternaAssistenteId
                    })
                    .FirstOrDefaultAsync();

                if (maquina == null)
                    return NotFound("Máquina não encontrada.");

                return Ok(maquina);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter máquina: {ex.Message}");
            }
        }
        #endregion

        #region CriarMaquina
        /// <summary>
        /// Cria uma nova máquina.
        /// </summary>
        /// <param name="novaMaquinaDto">Dados para criação da máquina.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="201">Máquina criada com sucesso.</response>
        /// <response code="400">Dados inválidos ou duplicados.</response>
        /// <response code="500">Erro interno ao criar a máquina.</response>
        [HttpPost("CriarMaquina")]
        [Authorize(Roles = "Tecnico")]
        [Produces("application/json")]
        public async Task<ActionResult> CriarMaquina([FromBody] MaquinaDTO novaMaquinaDto)
        {
            try
            {
                // Verifica se já existe máquina com o mesmo nome e linha de produção
                var maquinaExistente = await _context.Maquinas
                    .AnyAsync(m => m.Nome == novaMaquinaDto.Nome && m.LinhaProd == novaMaquinaDto.LinhaProd);

                if (maquinaExistente)
                    return BadRequest("Já existe uma máquina com o mesmo nome na mesma linha de produção.");

                var novaMaquina = new Maquina
                {
                    Nome = novaMaquinaDto.Nome,
                    LinhaProd = novaMaquinaDto.LinhaProd,
                    AssistenciaExternaAssistenteId = novaMaquinaDto.AssistenciaExternaAssistenteId
                };

                _context.Maquinas.Add(novaMaquina);
                await _context.SaveChangesAsync();

                return StatusCode(201, "Máquina criada com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao criar máquina: {ex.Message}");
            }
        }
        #endregion

        #region AtualizarMaquina
        /// <summary>
        /// Atualiza os dados de uma máquina existente.
        /// </summary>
        /// <param name="maquinaId">ID da máquina a atualizar.</param>
        /// <param name="maquinaAtualizada">Dados atualizados da máquina.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Máquina atualizada com sucesso.</response>
        /// <response code="404">Máquina não encontrada.</response>
        /// <response code="400">Dados inválidos ou duplicados.</response>
        /// <response code="500">Erro interno ao tentar atualizar a máquina.</response>
        [HttpPut("AtualizarMaquina/{maquinaId}")]
        [Authorize(Roles = "Tecnico")]
        [Produces("application/json")]
        public async Task<IActionResult> AtualizarMaquina(int maquinaId, [FromBody] MaquinaDTO maquinaAtualizada)
        {
            try
            {
                var maquina = await _context.Maquinas.FindAsync(maquinaId);

                if (maquina == null)
                    return NotFound("Máquina não encontrada.");

                // Verificar se já existe outra máquina com o mesmo nome e linha de produção
                bool maquinaDuplicada = await _context.Maquinas
                    .AnyAsync(m => m.Nome == maquinaAtualizada.Nome && m.LinhaProd == maquinaAtualizada.LinhaProd && m.MaquinaId != maquinaId);

                if (maquinaDuplicada)
                    return BadRequest("Já existe outra máquina com o mesmo nome na mesma linha de produção.");

                // Atualizar os campos
                maquina.Nome = maquinaAtualizada.Nome;
                maquina.LinhaProd = maquinaAtualizada.LinhaProd;
                maquina.AssistenciaExternaAssistenteId = maquinaAtualizada.AssistenciaExternaAssistenteId;

                await _context.SaveChangesAsync();

                return Ok("Máquina atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar máquina: {ex.Message}");
            }
        }
        #endregion
    }
}

