using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using LogisControlAPI.Interfaces;
using LogisControlAPI.Models;
using LogisControlAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogisControlAPI.Controllers
{
    /// <summary>
    /// Controller responsável pela gestão das matérias-primas.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MateriaPrimaController : ControllerBase
    {
        private readonly LogisControlContext _context;
        private readonly IStockService _stockService;
        /// <summary>
        /// Construtor do controlador que injeta o contexto da base de dados.
        /// </summary>
        /// <param name="context">Instância do contexto da base de dados.</param>
        public MateriaPrimaController(LogisControlContext context, IStockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }

        /// <summary>
        /// Obtém todas as matérias-primas.
        /// </summary>
        /// <returns>
        /// 200 OK com a lista de <see cref="MateriaPrimaDTO"/>;
        /// 500 Internal Server Error em caso de falha.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MateriaPrimaDTO>), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<MateriaPrimaDTO>>> GetAll()
        {
            var list = await _context.MateriasPrimas
                .AsNoTracking()
                .Select(m => new MateriaPrimaDTO
                {
                    MateriaPrimaId = m.MateriaPrimaId,
                    Nome = m.Nome,
                    Quantidade = m.Quantidade,
                    Descricao = m.Descricao,
                    Categoria = m.Categoria,
                    CodInterno = m.CodInterno,
                    Preco = m.Preco
                })
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>
        /// Lista todas as matérias-primas associadas a uma determinada ordem de produção.
        /// </summary>
        /// <param name="ordemProducaoId">ID da ordem de produção.</param>
        /// <returns>Lista de matérias-primas relacionadas.</returns>
        /// <response code="200">Lista de matérias-primas obtida com sucesso.</response>
        /// <response code="404">Nenhuma matéria-prima encontrada para a ordem de produção.</response>
        [HttpGet("PorOrdemProducao/{ordemProducaoId:int}")]
        public async Task<ActionResult<IEnumerable<MateriaPrimaDTO>>> GetMateriasPrimasPorOrdemProducao(int ordemProducaoId)
        {
            var materias = await _context.MateriasPrimas
                .Where(mp => mp.MateriaPrimaProdutos
                    .Any(mpp => mpp.ProdutoProduto.OrdemProducaoOrdemProdId == ordemProducaoId))
                .Select(mp => new MateriaPrimaDTO
                {
                    MateriaPrimaId = mp.MateriaPrimaId,
                    Nome = mp.Nome,
                    Quantidade = mp.Quantidade,
                    Descricao = mp.Descricao,
                    Categoria = mp.Categoria,
                    CodInterno = mp.CodInterno,
                    Preco = mp.Preco
                })
                .ToListAsync();

            if (materias == null || !materias.Any())
                return NotFound($"Nenhuma matéria-prima associada à ordem de produção #{ordemProducaoId}.");

            return Ok(materias);
        }

        /// <summary>
        /// Obtém uma matéria-prima pelo seu ID.
        /// </summary>
        /// <param name="id">ID da matéria-prima (PK).</param>
        /// <returns>
        /// 200 OK com <see cref="MateriaPrimaDTO"/>;
        /// 404 Not Found se não existir;
        /// 500 Internal Server Error em caso de falha.
        /// </returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(MateriaPrimaDTO), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<MateriaPrimaDTO>> GetById(int id)
        {
            var m = await _context.MateriasPrimas
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.MateriaPrimaId == id);

            if (m == null)
                return NotFound();

            var dto = new MateriaPrimaDTO
            {
                MateriaPrimaId = m.MateriaPrimaId,
                Nome = m.Nome,
                Quantidade = m.Quantidade,
                Descricao = m.Descricao,
                Categoria = m.Categoria,
                CodInterno = m.CodInterno,
                Preco = m.Preco
            };
            return Ok(dto);
        }

        /// <summary>
        /// Cria uma nova matéria-prima.
        /// </summary>
        /// <param name="dto">Dados para criação (<see cref="CriarMateriaPrimaDTO"/>).</param>
        /// <returns>
        /// 201 Created com o recurso criado;
        /// 400 Bad Request se DTO inválido;
        /// 500 Internal Server Error em caso de falha.
        /// </returns>
        [HttpPost]
        [ProducesResponseType(typeof(MateriaPrimaDTO), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Create([FromBody] CriarMateriaPrimaDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var m = new MateriaPrima
            {
                Nome = dto.Nome,
                Quantidade = dto.Quantidade,
                Descricao = dto.Descricao,
                Categoria = dto.Categoria,
                CodInterno = dto.CodInterno,
                Preco = dto.Preco
            };

            _context.MateriasPrimas.Add(m);
            await _context.SaveChangesAsync();

            var resultDto = new MateriaPrimaDTO
            {
                MateriaPrimaId = m.MateriaPrimaId,
                Nome = m.Nome,
                Quantidade = m.Quantidade,
                Descricao = m.Descricao,
                Categoria = m.Categoria,
                CodInterno = m.CodInterno,
                Preco = m.Preco
            };

            return CreatedAtAction(
                nameof(GetById),
                new { id = m.MateriaPrimaId },
                resultDto);
        }

        /// <summary>
        /// Atualiza uma matéria-prima existente.
        /// </summary>
        /// <param name="id">ID da matéria-prima a atualizar.</param>
        /// <param name="dto">Dados de atualização (<see cref="UpdateMateriaPrimaDTO"/>).</param>
        /// <returns>
        /// 204 No Content se OK;
        /// 400 Bad Request se DTO inválido;
        /// 404 Not Found se não existir;
        /// 409 Conflict em caso de concorrência;
        /// 500 Internal Server Error em caso de falha.
        /// </returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMateriaPrimaDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var m = await _context.MateriasPrimas.FindAsync(id);
            if (m == null)
                return NotFound();

            // Guarda a quantidade anterior antes de alterar
            var quantidadeAnterior = m.Quantidade;

            m.Nome = dto.Nome;
            m.Quantidade = dto.Quantidade;
            m.Descricao = dto.Descricao;
            m.Categoria = dto.Categoria;
            m.CodInterno = dto.CodInterno;
            m.Preco = dto.Preco;

            try
            {
                await _context.SaveChangesAsync();
                await _stockService.VerificarStockCritico(id, quantidadeAnterior);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("O registo foi alterado por outro utilizador.");
            }

            return NoContent();
        }

        /// <summary>
        /// Remove uma matéria-prima.
        /// </summary>
        /// <param name="id">ID da matéria-prima a remover.</param>
        /// <returns>
        /// 204 No Content se OK;
        /// 404 Not Found se não existir;
        /// 500 Internal Server Error em caso de falha.
        /// </returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _context.MateriasPrimas.FindAsync(id);
            if (m == null)
                return NotFound();

            _context.MateriasPrimas.Remove(m);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}