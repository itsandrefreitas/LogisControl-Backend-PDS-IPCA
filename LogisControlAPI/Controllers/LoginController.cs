using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using LogisControlAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LogisControlAPI.Controllers
{
    /// <summary>
    /// Controlador responsável pela autenticação dos utilizadores.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly LogisControlContext _context;
        private readonly UtilizadorService _utilizadorService;
        private readonly AuthService _authService;

        public LoginController(LogisControlContext context, UtilizadorService utilizadorService, AuthService authService)
        {
            _context = context;
            _utilizadorService = utilizadorService;
            _authService = authService;
        }

        /// <summary>
        /// Autentica o utilizador e devolve um token JWT.
        /// </summary>
        /// <param name="loginDto">Credenciais do utilizador (nº funcionário e password).</param>
        /// <returns>Token JWT e dados do utilizador.</returns>
        /// <response code="200">Login efetuado com sucesso.</response>
        /// <response code="401">Número de funcionário ou password inválidos.</response>
        /// <response code="500">Erro interno ao efetuar login.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            try
            {
                var utilizador = await _context.Utilizadores
                    .FirstOrDefaultAsync(u => u.NumFuncionario == loginDto.NumFuncionario);

                if (utilizador == null)
                    return Unauthorized("Número de funcionário ou senha inválidos.");

                bool senhaCorreta = _utilizadorService.VerifyPassword(utilizador.Password, loginDto.Password);

                if (!senhaCorreta)
                    return Unauthorized("Número de funcionário ou senha inválidos.");

                var token = _authService.GenerateToken(utilizador.UtilizadorId,utilizador.NumFuncionario, utilizador.Role);

                return Ok(new
                {
                    Token = token,
                    Id = utilizador.UtilizadorId,
                    Nome = utilizador.PrimeiroNome,
                    Role = utilizador.Role,
                    Estado = utilizador.Estado,
                    NumFuncionario = utilizador.NumFuncionario
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao efetuar login: {ex.Message}");
            }
        }
    }
}