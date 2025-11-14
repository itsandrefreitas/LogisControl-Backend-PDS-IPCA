using Microsoft.AspNetCore.Mvc;
using LogisControlAPI.Data;

namespace LogisControlAPI.Controllers
{
    /// <summary>
    /// Controlador para testar a conectividade com a base de dados SQL Server.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TesteController : ControllerBase
    {
        private readonly LogisControlContext _context;

        /// <summary>
        /// Construtor que injeta o contexto da base de dados no controlador.
        /// </summary>
        /// <param name="context">Instância do AppDbContext para verificar a conexão com a BD.</param>
        public TesteController(LogisControlContext context)
        {
            _context = context;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            try
            {
                if (_context.Database.CanConnect())
                {
                    return Ok(new
                    {
                        sucesso = true,
                        mensagem = "Ligação ao SQL Server estabelecida com sucesso!"
                    });
                }
                else
                {
                    return StatusCode(500, new
                    {
                        sucesso = false,
                        mensagem = "Falha na ligação ao SQL Server."
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    sucesso = false,
                    mensagem = $"Erro: {ex.Message}"
                });
            }
        }
    }
}