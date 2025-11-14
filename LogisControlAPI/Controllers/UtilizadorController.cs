using Microsoft.AspNetCore.Mvc;
using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Models;
using LogisControlAPI.Services;
using Microsoft.AspNetCore.Authorization;


namespace LogisControlAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilizadorController : ControllerBase
    {
        private readonly LogisControlContext _context;
        private readonly UtilizadorService _utilizadorService;

        public UtilizadorController(LogisControlContext context, UtilizadorService utilizadorService)
        {
            _context = context;
            _utilizadorService = utilizadorService;
        }

        #region Obter Utilizadores
        /// <summary>
        /// Obtém a lista de todos os utilizadores registados.
        /// </summary>
        /// <returns>Lista de utilizadores com dados públicos (sem password).</returns>
        /// <response code="200">Lista de utilizadores obtida com sucesso.</response>
        [Authorize(Roles = "Gestor")]
        [HttpGet ("ObterUtilizadores")]
        [Produces("application/json")]
        public async Task<IEnumerable<UtilizadorDTO>> GetUtilizadores()
        {
            return await _context.Utilizadores
                .Select(u => new UtilizadorDTO
                {
                    UtilizadorId = u.UtilizadorId,
                    PrimeiroNome = u.PrimeiroNome,
                    Sobrenome = u.Sobrenome,
                    NumFuncionario = u.NumFuncionario,
                    Role = u.Role,
                    Estado = u.Estado
                })
                .ToListAsync();
        }

        #endregion

        #region Criar Utilizador
        /// <summary>
        /// Cria um novo utilizador com os dados fornecidos.
        /// </summary>
        /// <param name="novoUtilizadorDto">Dados do novo utilizador.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Utilizador criado com sucesso.</response>
        /// <response code="400">Número de funcionário já existe.</response>
        /// <response code="500">Erro interno ao criar o utilizador.</response>
        [Authorize(Roles = "Gestor")]
        [HttpPost("CriarUtilizador")]
        [Produces("application/json")]
        public async Task<IActionResult> CriarUtilizador([FromBody] CriarUtilizadorDTO novoUtilizadorDto)
        {
            // Verifica se o número de funcionário já existe
            if (await _utilizadorService.VerificarSeExisteNumeroFuncionario(novoUtilizadorDto.NumFuncionario))
                return BadRequest("Já existe um utilizador com esse número de funcionário.");

            // Gerar o hash da senha antes de guardar
            string senhaHash = _utilizadorService.HashPassword(novoUtilizadorDto.Password);

            // Criar novo utilizador com a senha hashada
            Utilizador novoUtilizador = new Utilizador
            {
                PrimeiroNome = novoUtilizadorDto.PrimeiroNome,
                Sobrenome = novoUtilizadorDto.Sobrenome,
                NumFuncionario = novoUtilizadorDto.NumFuncionario,
                Password = senhaHash,
                Role = novoUtilizadorDto.Role,
                Estado = true 
            };

            _context.Utilizadores.Add(novoUtilizador);
            await _context.SaveChangesAsync();

            return Ok("Utilizador criado com sucesso!");
        }

        #endregion

        #region AtualizarPerfil
        /// <summary>
        /// Atualiza o primeiro nome, sobrenome e/ou password do utilizador autenticado.
        /// Apenas os campos preenchidos serão alterados.
        /// </summary>
        /// <param name="dto">Dados do utilizador a atualizar.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Perfil atualizado com sucesso.</response>
        /// <response code="401">Utilizador não identificado (token inválido).</response>
        /// <response code="404">Utilizador não encontrado.</response>
        /// <response code="500">Erro interno ao atualizar o perfil.</response>
        [Authorize]
        [HttpPut("AtualizarPerfil")]
        [Produces("application/json")]
        public async Task<IActionResult> AtualizarPerfil([FromBody] UtilizadorUpdateDTO dto)
        {
            try
            {
                // Obter ID do utilizador autenticado a partir das claims (JWT)
                var idClaim = User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int utilizadorId))
                    return Unauthorized("Não foi possível identificar o utilizador.");

                var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
                if (utilizador == null)
                    return NotFound("Utilizador não encontrado.");

                // Atualizar apenas os campos fornecidos
                if (!string.IsNullOrWhiteSpace(dto.PrimeiroNome))
                    utilizador.PrimeiroNome = dto.PrimeiroNome;

                if (!string.IsNullOrWhiteSpace(dto.Sobrenome))
                    utilizador.Sobrenome = dto.Sobrenome;

                if (!string.IsNullOrWhiteSpace(dto.NovaPassword))
                {
                    var novaHash = _utilizadorService.HashPassword(dto.NovaPassword);
                    utilizador.Password = novaHash;
                }

                await _context.SaveChangesAsync();
                return Ok("Perfil atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar perfil: {ex.Message}");
            }
        }
        #endregion

        #region ObterPerfil
        /// <summary>
        /// Obtém os dados do perfil do utilizador autenticado.
        /// </summary>
        /// <returns>Dados do utilizador autenticado.</returns>
        /// <response code="200">Perfil obtido com sucesso.</response>
        /// <response code="401">Token inválido ou ID não encontrado nas claims.</response>
        /// <response code="404">Utilizador não encontrado.</response>
        /// <response code="500">Erro interno ao obter perfil.</response>
        [Authorize]
        [HttpGet("ObterPerfil")]
        [Produces("application/json")]
        public async Task<IActionResult> ObterPerfil()
        {
            try
            {
                // Obter o ID do token JWT
                var idClaim = User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(idClaim) || !int.TryParse(idClaim, out int utilizadorId))
                    return Unauthorized("Não foi possível identificar o utilizador.");

                
                var utilizador = await _context.Utilizadores.FindAsync(utilizadorId);
                if (utilizador == null)
                    return NotFound("Utilizador não encontrado.");

                
                return Ok(new
                {
                    utilizador.PrimeiroNome,
                    utilizador.Sobrenome,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter perfil: {ex.Message}");
            }
        }
        #endregion


        #region AtualizarEstadoRole
        /// <summary>
        /// Atualiza o estado e o perfil (role) de um utilizador. Apenas usado por administradores.
        /// </summary>
        /// <param name="id">ID do utilizador a atualizar.</param>
        /// <param name="dto">Novo role e estado.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Utilizador atualizado com sucesso.</response>
        /// <response code="404">Utilizador não encontrado.</response>
        /// <response code="500">Erro interno ao atualizar o utilizador.</response>
        [Authorize(Roles = "Gestor")]
        [HttpPut("AtualizarEstadoRole/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> AtualizarEstadoRole(int id, [FromBody] UtilizadorUpdateAdminDTO dto)
        {
            try
            {
                var utilizador = await _context.Utilizadores.FindAsync(id);
                if (utilizador == null)
                    return NotFound("Utilizador não encontrado.");

                utilizador.Role = dto.Role;
                utilizador.Estado = dto.Estado;

                await _context.SaveChangesAsync();
                return Ok("Utilizador atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar o utilizador: {ex.Message}");
            }
        }
        #endregion

        #region ResetPassword
        /// <summary>
        /// Redefine a password de um utilizador com base no número de funcionário.
        /// Apenas acessível a gestores.
        /// </summary>
        /// <param name="dto">Dados para reset da password.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Password redefinida com sucesso.</response>
        /// <response code="404">Utilizador não encontrado.</response>
        /// <response code="500">Erro interno ao redefinir password.</response>
        [Authorize(Roles = "Gestor")]
        [HttpPut("ResetPassword")]
        [Produces("application/json")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            try
            {
                var sucesso = await _utilizadorService.ResetPasswordAsync(dto.NumFuncionario, dto.NovaPassword);

                if (!sucesso)
                    return NotFound("Utilizador não encontrado.");

                return Ok("Password redefinida com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao redefinir a password: {ex.Message}");
            }
        }
        #endregion

    }
}