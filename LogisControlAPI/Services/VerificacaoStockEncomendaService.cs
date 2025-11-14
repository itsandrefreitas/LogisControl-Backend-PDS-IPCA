using LogisControlAPI.Data;
using LogisControlAPI.Services;
using Microsoft.EntityFrameworkCore;


public class VerificacaoStockEncomendaService
{
    private readonly LogisControlContext _context;
    private readonly NotificationService _notificador;
    private const string EmailResponsavelStock = "nunofernandescastro@gmail.com";

    public VerificacaoStockEncomendaService(LogisControlContext context, NotificationService notificador)
    {
        _context = context;
        _notificador = notificador;
    }

    public async Task VerificarStockParaEncomenda(int encomendaClienteId)
    {
        var itens = await _context.EncomendasItem
            .Where(ei => ei.EncomendaClienteEncomendaClienteId == encomendaClienteId)
            .Include(ei => ei.Produto)
                .ThenInclude(p => p.MateriaPrimaProdutos)
                    .ThenInclude(mp => mp.MateriaPrimaMateriaPrimaIDNavigation)
            .ToListAsync();

        if (!itens.Any()) return;

        var mensagens = new List<string>();

        foreach (var item in itens)
        {
            var produto = item.Produto;
            int qtdEncomendada = item.Quantidade ?? 0;

            foreach (var mp in produto.MateriaPrimaProdutos)
            {
                var materia = mp.MateriaPrimaMateriaPrimaIDNavigation;
                int stockAtual = materia.Quantidade;
                int qtdNecessaria = mp.QuantidadeNec * qtdEncomendada;

                if (stockAtual < qtdNecessaria)
                {
                    mensagens.Add(
                        $"⚠️ '{materia.Nome}': precisa de {qtdNecessaria} p/ {produto.Nome}, só há {stockAtual}."
                    );
                }
            }
        }

        if (mensagens.Any())
        {
            var corpo = string.Join("\n", mensagens);
            await _notificador.NotificarAsync(EmailResponsavelStock,
                $"⚠️ Stock Insuficiente para Encomenda {encomendaClienteId}",
                corpo);

            var encomenda = await _context.EncomendasCliente.FindAsync(encomendaClienteId);
            if (encomenda != null)
            {
                encomenda.Estado = "Pendente por Falta de Stock";
                await _context.SaveChangesAsync();
            }
        }
    }
}
