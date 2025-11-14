using LogisControlAPI.Data;
using System;
using LogisControlAPI.Interfaces;
using System.Threading.Tasks;

namespace LogisControlAPI.Services
{
    /// <summary>
    /// Serviço responsável por verificar o stock de matérias-primas e emitir alertas se necessário.
    /// </summary>
    public class StockService : IStockService
    {
        private readonly LogisControlContext _context;
        private readonly NotificationService _notificador;

        /// <summary>
        /// Email fixo (temporário) do responsável de stock.
        /// </summary>
        private const string EmailResponsavelStock = "nunofernandescastro@gmail.com";

        /// <summary>
        /// Construtor que injeta as dependências necessárias.
        /// </summary>
        /// <param name="context">Contexto da base de dados.</param>
        /// <param name="notificador">Serviço de notificações.</param>
        public StockService(LogisControlContext context, NotificationService notificador)
        {
            _context = context;
            _notificador = notificador;
        }

        /// <summary>
        /// Verifica se a quantidade de uma matéria-prima diminuiu para abaixo do limite crítico e envia um alerta se necessário.
        /// </summary>
        /// <param name="materiaPrimaId">ID da matéria-prima a verificar.</param>
        /// <param name="quantidadeAnterior">Quantidade anterior da matéria-prima.</param>
        public async Task VerificarStockCritico(int materiaPrimaId, int quantidadeAnterior)
        {

            if (materiaPrimaId <= 0)
                throw new ArgumentException("O ID da matéria-prima deve ser maior que zero.");

            var materia = await _context.MateriasPrimas.FindAsync(materiaPrimaId);
            if (materia == null)
                return;

            if (materia.Quantidade < 10 && materia.Quantidade < quantidadeAnterior)
            {
                var assunto = $"Stock Baixo - {materia.Nome}";
                var mensagem = $"A matéria-prima \"{materia.Nome}\" tem apenas {materia.Quantidade} unidades em stock.";

                await _notificador.NotificarAsync(EmailResponsavelStock, assunto, mensagem);
            }
        }


        /// <summary>
        /// Verifica se a quantidade de um produto diminuiu para abaixo do limite crítico e envia um alerta se necessário.
        /// </summary>
        /// <param name="produtoId">ID do produto a verificar.</param>
        /// <param name="quantidadeAnterior">Quantidade anterior do produto.</param>
        public async Task VerificarStockCriticoProduto(int produtoId, int quantidadeAnterior)
        {
            var produto = await _context.Produtos.FindAsync(produtoId);
            if (produto == null)
                return;

            if (produto.Quantidade < 10 && produto.Quantidade < quantidadeAnterior)
            {
                var assunto = $"Stock Baixo - Produto {produto.Nome}";
                var mensagem = $"O produto \"{produto.Nome}\" tem apenas {produto.Quantidade} unidades em stock.";

                await _notificador.NotificarAsync(EmailResponsavelStock, assunto, mensagem);
            }
        }


    }
}
