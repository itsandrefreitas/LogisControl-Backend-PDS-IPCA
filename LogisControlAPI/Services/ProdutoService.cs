using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using LogisControlAPI.Interfaces;
using LogisControlAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LogisControlAPI.Services
{
    /// <summary>
    /// Serviço responsável pela lógica de negócios relacionada aos produtos.
    /// </summary>
    public class ProdutoService
    {
        private readonly LogisControlContext _context;
        private readonly IStockService _stockService;

        public ProdutoService(LogisControlContext context, IStockService stockService)
        {
            _context = context;
            _stockService = stockService;
        }


        #region CriarProduto

        /// <summary>
        /// Cria um novo produto e associa-lhe as matérias-primas necessárias.
        /// </summary>
        /// <param name="dto">Objeto DTO com os dados do produto e das matérias-primas associadas.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        /// <exception cref="Exception">Lança exceção se ocorrer falha ao gravar na base de dados.</exception>
        public async Task CriarProdutoAsync(CriarProdutoDTO dto)
        {

            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new Exception("Nome é obrigatório.");

            if (dto.Quantidade < 0)
                throw new Exception("Quantidade do produto não pode ser negativa.");

            if (string.IsNullOrWhiteSpace(dto.CodInterno))
                throw new Exception("Código interno é obrigatório.");

            if (dto.Preco < 0)
                throw new Exception("Preço não pode ser negativo.");


            // Criar a entidade Produto a partir do DTO recebido
            var novoProduto = new Produto
            {
                Nome = dto.Nome,
                Quantidade = dto.Quantidade,
                Descricao = dto.Descricao,
                CodInterno = dto.CodInterno,
                Preco = dto.Preco
            };

            // Adicionar o produto ao contexto e guardar para obter o ID
            _context.Produtos.Add(novoProduto);
            await _context.SaveChangesAsync();

            // Criar associações com as matérias-primas
            foreach (var materia in dto.MateriasPrimas)
            {
                var relacao = new MateriaPrimaProduto
                {
                    ProdutoProdutoId = novoProduto.ProdutoId,
                    MateriaPrimaMateriaPrimaId = materia.MateriaPrimaId,
                    QuantidadeNec = materia.QuantidadeNec
                };

                _context.MateriaPrimaProdutos.Add(relacao);
            }

            // Guardar associações
            await _context.SaveChangesAsync();
        }

        #endregion
        #region ObterProdutoParaEdicao

        /// <summary>
        /// Obtém os dados de um produto e as suas matérias-primas para edição.
        /// </summary>
        /// <param name="id">ID do produto.</param>
        /// <returns>CriarProdutoDTO com os dados preenchidos.</returns>
        public async Task<CriarProdutoDTO?> ObterProdutoParaEdicaoAsync(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.MateriaPrimaProdutos)
                .FirstOrDefaultAsync(p => p.ProdutoId == id);

            if (produto == null)
                return null;

            return new CriarProdutoDTO
            {
                ProdutoId = produto.ProdutoId,
                Nome = produto.Nome,
                Quantidade = produto.Quantidade,
                Descricao = produto.Descricao,
                CodInterno = produto.CodInterno,
                Preco = produto.Preco,
                MateriasPrimas = produto.MateriaPrimaProdutos.Select(mp => new MateriaPrimaProdutoCriacaoDTO
                {
                    MateriaPrimaId = mp.MateriaPrimaMateriaPrimaId,
                    QuantidadeNec = mp.QuantidadeNec
                }).ToList()
            };
        }
        #endregion

        #region AtualizarProduto

        /// <summary>
        /// Atualiza um produto existente e as suas matérias-primas.
        /// </summary>
        /// <param name="id">ID do produto a atualizar.</param>
        /// <param name="dto">DTO com os dados atualizados.</param>
        /// <returns>Tarefa assíncrona.</returns>
        public async Task AtualizarProdutoAsync(int id, CriarProdutoDTO dto)
        {
            var produto = await _context.Produtos
                .Include(p => p.MateriaPrimaProdutos)
                .FirstOrDefaultAsync(p => p.ProdutoId == id);

            if (produto == null)
                throw new Exception("Produto não encontrado.");

            var quantidadeAnterior = produto.Quantidade;

            // Atualizar campos principais
            produto.Nome = dto.Nome;
            produto.Quantidade = dto.Quantidade;
            produto.Descricao = dto.Descricao;
            produto.CodInterno = dto.CodInterno;
            produto.Preco = dto.Preco;

            // Atualizar matérias-primas
            _context.MateriaPrimaProdutos.RemoveRange(produto.MateriaPrimaProdutos);

            foreach (var materia in dto.MateriasPrimas)
            {
                var novaRelacao = new MateriaPrimaProduto
                {
                    ProdutoProdutoId = produto.ProdutoId,
                    MateriaPrimaMateriaPrimaId = materia.MateriaPrimaId,
                    QuantidadeNec = materia.QuantidadeNec
                };

                _context.MateriaPrimaProdutos.Add(novaRelacao);
            }

            await _context.SaveChangesAsync();

            // Verificar stock crítico após atualizar
            await _stockService.VerificarStockCriticoProduto(id, quantidadeAnterior);
        }
        #endregion


    }
}
