using LogisControlAPI.Data;
using LogisControlAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace LogisControlAPI.Services
{
    public class ProducaoService
    {
        private readonly LogisControlContext _context;
        private readonly NotificationService _notificador;

        /// <summary>
        /// Email fixo (temporário) do responsável de stock.
        /// </summary>
        private const string EmailResponsavelStock = "nunofernandescastro@gmail.com";

        public ProducaoService(LogisControlContext context, NotificationService notificador)
        {
            _context = context;
            _notificador = notificador;
        }

        /// <summary>
        /// Atualiza o estado e observações de um registo de produção.
        /// Envia um e-mail se o estado for "Produzido" ou "Parado devido defeito".
        /// </summary>
        /// <param name="registoId">ID do registo de produção a atualizar.</param>
        /// <param name="novoEstado">Novo estado (opcional).</param>
        /// <param name="observacoes">Novas observações (opcional).</param>
        /// <exception cref="Exception">Se o registo não for encontrado.</exception>
        public async Task AtualizarEstadoEObservacoesAsync(int registoId, string? novoEstado, string? observacoes)
        {
            var registo = await _context.RegistosProducao.FindAsync(registoId);
            if (registo == null) throw new Exception("Registo de produção não encontrado.");

            bool estadoFoiProduzido = false;
            bool estadoFoiCancelado = false;

            if (!string.IsNullOrEmpty(novoEstado))
            {
                registo.Estado = novoEstado;
                estadoFoiProduzido = novoEstado == "Produzido";
                estadoFoiCancelado = novoEstado == "Cancelado";
            }

            if (!string.IsNullOrEmpty(observacoes))
            {
                registo.Observacoes = observacoes;
            }

            await _context.SaveChangesAsync();

            var ordem = await _context.OrdensProducao
                .FirstOrDefaultAsync(o => o.OrdemProdId == registo.OrdemProducaoOrdemProdId);

            if (ordem != null)
            {
                if (estadoFoiProduzido)
                {
                    ordem.Estado = "Concluido";
                    ordem.DataConclusao = DateTime.Now;

                    var produto = await _context.Produtos
                        .FirstOrDefaultAsync(p => p.OrdemProducaoOrdemProdId == ordem.OrdemProdId);

                    if (produto != null)
                    {
                        produto.Quantidade += ordem.Quantidade;
                    }

                    await _context.SaveChangesAsync();

                    string assunto = "Produção Concluída";
                    string corpo = $"A produção do registo #{registo.RegistoProducaoId} foi concluída com sucesso.";
                    await _notificador.NotificarAsync(EmailResponsavelStock, assunto, corpo);
                }
                else if (estadoFoiCancelado)
                {
                    ordem.Estado = "Cancelada";
                    await _context.SaveChangesAsync();

                    string assunto = "Produção Cancelada";
                    string corpo = $"A produção do registo #{registo.RegistoProducaoId} foi cancelada.";
                    await _notificador.NotificarAsync(EmailResponsavelStock, assunto, corpo);
                }
                else if (registo.Estado == "Parado devido defeito")
                {
                    string assunto = "Produção Parada devido a Defeito";
                    string corpo = $"A produção do registo #{registo.RegistoProducaoId} foi parada devido a um defeito.";
                    await _notificador.NotificarAsync(EmailResponsavelStock, assunto, corpo);
                }
            }
        }
    }
}
