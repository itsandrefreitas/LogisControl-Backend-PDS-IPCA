using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Models;
using LogisControlAPI.Data;
using LogisControlAPI.DTO;



namespace LogisControlAPI.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão dos fornecedores.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FornecedorController : ControllerBase
    {
        private readonly LogisControlContext _context;

        /// <summary>
        /// Construtor do controlador que injeta o contexto da base de dados.
        /// </summary>
        public FornecedorController(LogisControlContext context)
        {
            _context = context;
        }

        #region ObterFornecedores
        /// <summary>
        /// Obtém a lista de todos os fornecedores registados.
        /// </summary>
        /// <returns>Lista de fornecedores.</returns>
        /// <response code="200">Retorna a lista de fornecedores com sucesso.</response>
        /// <response code="500">Erro interno ao tentar obter os fornecedores.</response>
        [HttpGet("ObterFornecedores")]
        public async Task<ActionResult<IEnumerable<FornecedorDTO>>> GetFornecedores()
        {
            try
            {
                var fornecedores = await _context.Fornecedores
                    .Select(f => new FornecedorDTO
                    {
                        FornecedorId = f.FornecedorId,
                        Nome = f.Nome,
                        Telefone = f.Telefone,
                        Email = f.Email
                    })
                    .ToListAsync();

                return Ok(fornecedores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao obter fornecedores: {ex.Message}");
            }
        }
        #endregion

        #region ObterFornecedorPorNome
        /// <summary>
        /// Obtém fornecedores pelo nome (pesquisa parcial ou total).
        /// </summary>
        /// <param name="nome">Nome ou parte do nome do fornecedor.</param>
        /// <returns>Lista de fornecedores que correspondem ao nome pesquisado.</returns>
        /// <response code="200">Fornecedores encontrados com sucesso.</response>
        /// <response code="404">Nenhum fornecedor encontrado.</response>
        /// <response code="500">Erro interno ao procurar fornecedores.</response>
        [HttpGet("ObterFornecedorPorNome/{nome}")]
        public async Task<ActionResult<IEnumerable<FornecedorDTO>>> GetFornecedorPorNome(string nome)
        {
            try
            {
                var fornecedores = await _context.Fornecedores
                    .Where(f => f.Nome.Contains(nome))
                    .Select(f => new FornecedorDTO
                    {
                        FornecedorId = f.FornecedorId,
                        Nome = f.Nome,
                        Telefone = f.Telefone,
                        Email = f.Email
                    })
                    .ToListAsync();

                if (fornecedores == null || !fornecedores.Any())
                    return NotFound("Nenhum fornecedor encontrado com esse nome.");

                return Ok(fornecedores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao procurar fornecedor: {ex.Message}");
            }
        }
        #endregion

        #region CriarFornecedor
        /// <summary>
        /// Cria um novo fornecedor na base de dados.
        /// </summary>
        /// <param name="novoFornecedorDto">Objeto com os dados do fornecedor a criar.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="201">Fornecedor criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="500">Erro interno ao tentar criar o fornecedor.</response>
        [HttpPost("CriarFornecedor")]
        public async Task<ActionResult> CriarFornecedor([FromBody] CriarFornecedorDTO novoFornecedorDto)
        {
            try
            {
                var novoFornecedor = new Fornecedor
                {
                    Nome = novoFornecedorDto.Nome,
                    Telefone = novoFornecedorDto.Telefone,
                    Email = novoFornecedorDto.Email
                };

                _context.Fornecedores.Add(novoFornecedor);
                await _context.SaveChangesAsync();

                return StatusCode(201, "Fornecedor criado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao criar fornecedor: {ex.Message}");
            }
        }
        #endregion

        #region EditarFornecedor
        /// <summary>
        /// Atualiza os dados de um fornecedor existente.
        /// </summary>
        /// <param name="fornecedorId">ID do fornecedor a atualizar.</param>
        /// <param name="fornecedorAtualizado">Dados atualizados do fornecedor.</param>
        /// <returns>Mensagem de sucesso ou erro.</returns>
        /// <response code="200">Fornecedor atualizado com sucesso.</response>
        /// <response code="404">Fornecedor não encontrado.</response>
        /// <response code="500">Erro interno ao tentar atualizar o fornecedor.</response>
        [HttpPut("AtualizarFornecedor/{fornecedorId}")]
        public async Task<IActionResult> AtualizarFornecedor(int fornecedorId, [FromBody] AtualizarFornecedorDTO fornecedorAtualizado)
        {
            try
            {
                var fornecedor = await _context.Fornecedores.FindAsync(fornecedorId);

                if (fornecedor == null)
                    return NotFound("Fornecedor não encontrado.");

                // Atualizar os dados
                fornecedor.Nome = fornecedorAtualizado.Nome;
                fornecedor.Telefone = fornecedorAtualizado.Telefone;
                fornecedor.Email = fornecedorAtualizado.Email;

                await _context.SaveChangesAsync();

                return Ok("Fornecedor atualizado com sucesso.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar fornecedor: {ex.Message}");
            }
        }
        #endregion

    }
}
