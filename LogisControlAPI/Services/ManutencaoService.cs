using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using LogisControlAPI.Models;
using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Interfaces;

namespace LogisControlAPI.Services
{
    public class ManutencaoService
    {
        private readonly LogisControlContext _context;
        private readonly ITelegramService _telegramService;


        public ManutencaoService(LogisControlContext context, ITelegramService telegramService)
        {
            _context = context;
            _telegramService = telegramService;
        }


        #region Operações de Pedidos de Manutenção

        /// <summary>
        /// Devolve os pedidos de manutenção com mais de 7 dias e ainda não concluídos.
        /// </summary>
        /// <returns>Lista de pedidos de manutenção em atraso</returns>
        /// <remarks>
        /// Considera como "em atraso" qualquer pedido que não esteja no estado "Resolvido"
        /// e cuja data de abertura seja anterior a 7 dias da data atual.
        /// </remarks>
        public async Task<List<PedidoManutencao>> ObterPedidosAtrasadosAsync()
        {
            var limite = DateTime.Now.AddDays(-7);
            return await _context.PedidosManutencao
                .Where(p => p.Estado != "Concluido" && p.DataAbertura < limite)
                .ToListAsync();
        }

        #endregion

        #region criarPedidoTelegram
        /// <summary>
        /// Cria um novo pedido de manutenção na base de dados e envia uma notificação para o Telegram.
        /// </summary>
        /// <param name="dto">Objeto DTO com os dados do pedido (descrição, máquina, etc.).</param>
        /// <param name="utilizadorId">ID do utilizador autenticado que está a criar o pedido.</param>
        /// <returns>Uma tarefa assíncrona.</returns>
        /// <exception cref="Exception">Pode lançar exceções se ocorrer erro ao guardar ou ao notificar.</exception>
        public async Task CriarPedidoAsync(PedidoManutençãoDTO dto, int utilizadorId)
        {

            if (string.IsNullOrWhiteSpace(dto.Descricao))
                throw new Exception("A descrição do pedido é obrigatória.");


            var maquina = await _context.Maquinas.FindAsync(dto.MaquinaMaquinaId);
            if (maquina == null)
                throw new Exception("Máquina não encontrada.");

          

            var pedido = new PedidoManutencao
            {
                Descricao = dto.Descricao,
                Estado = "Em Espera",
                DataAbertura = DateTime.UtcNow,
                DataConclusao = null,
                MaquinaMaquinaId = dto.MaquinaMaquinaId,
                UtilizadorUtilizadorId = utilizadorId
            };

            _context.PedidosManutencao.Add(pedido);
            await _context.SaveChangesAsync();

            // Obter o nome da máquina
            maquina = await _context.Maquinas.FindAsync(dto.MaquinaMaquinaId);
            var nomeMaquina = maquina?.Nome ?? "Desconhecida";

            var mensagem = $"📢 Novo Pedido de Manutenção\nID: {pedido.PedidoManutId}\nMáquina: {nomeMaquina}\nDescrição: {pedido.Descricao}";
            await _telegramService.EnviarMensagemAsync(mensagem, "Manutencao");
        }
        #endregion


        /// <summary>
        /// Atualiza o estado de um pedido de manutenção e define automaticamente a data de conclusão
        /// quando o estado for "Recusado". Envia uma notificação para a produção com a descrição.
        /// </summary>
        /// <param name="pedidoId">ID do pedido de manutenção a ser atualizado</param>
        /// <param name="novoEstado">Novo estado do pedido (deve ser um valor válido)</param>
        /// <exception cref="Exception">Lançada quando o pedido não é encontrado</exception>
        public async Task AtualizarEstadoPedido(int pedidoId, string novoEstado)
        {
            var pedido = await _context.PedidosManutencao
                .Include(p => p.MaquinaMaquina)
                .FirstOrDefaultAsync(p => p.PedidoManutId == pedidoId);

            if (pedido == null)
                throw new Exception("Pedido não encontrado");

            pedido.Estado = novoEstado;

            if (novoEstado == "Recusado")
            {
                pedido.DataConclusao = DateTime.Now;

                var nomeMaquina = pedido.MaquinaMaquina?.Nome ?? "Desconhecida";
                var mensagem = $"🚫 Pedido Recusado\nID: {pedido.PedidoManutId}\nMáquina: {nomeMaquina}\nDescrição: {pedido.Descricao}";

                await _telegramService.EnviarMensagemAsync(mensagem, "Producao");
            }

            await _context.SaveChangesAsync();
        }

        #region ReabrirPedidoManutenção

        /// <summary>
        /// Reabre um pedido de manutenção, define o estado como "Em Espera",
        /// limpa a data de conclusão e envia notificação por Telegram.
        /// </summary>
        /// <param name="pedidoId">ID do pedido a reabrir.</param>
        /// <param name="descricaoComJustificacao">Nova descrição com a justificação incluída.</param>
        /// <exception cref="Exception">Lançada se o pedido não for encontrado.</exception>
        public async Task ReabrirPedidoManutencaoAsync(ReabrirPedidoManutencaoDTO dto)
        {
            var pedido = await _context.PedidosManutencao
                .Include(p => p.MaquinaMaquina)
                .FirstOrDefaultAsync(p => p.PedidoManutId == dto.PedidoManutId);

            if (pedido == null)
                throw new Exception("Pedido não encontrado.");

            var nome = "Operador"; // Ou podes obter do contexto JWT
            var dataHora = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            var anotacao = $"\n\nReaberto por {nome} em {dataHora}:\n{dto.Justificacao}";

            pedido.Descricao += anotacao;
            pedido.Estado = "Em Espera";
            pedido.DataConclusao = null;

            var maquina = pedido.MaquinaMaquina?.Nome ?? "Desconhecida";
            var mensagem = $"🔄 Pedido de Manutenção Reaberto\nID: {pedido.PedidoManutId}\nMáquina: {maquina}\nJustificação:\n{dto.Justificacao}";

            await _telegramService.EnviarMensagemAsync(mensagem, "Manutencao");

            await _context.SaveChangesAsync();
        }
        #endregion

        #region Operações de Registos de Manutenção

        /// <summary>
        /// Verifica se um registo de manutenção foi marcado como "Resolvido" e, em caso afirmativo,
        /// atualiza automaticamente o estado do pedido associado para "Concluido" com a data atual,
        /// e envia uma notificação para a produção com os detalhes.
        /// </summary>
        /// <param name="registoId">ID do registo de manutenção a verificar</param>
        /// <exception cref="Exception">
        /// Lançada quando o registo ou o pedido associado não são encontrados
        /// </exception>
        /// <remarks>
        /// Esta função é tipicamente chamada após a atualização de um registo de manutenção
        /// para garantir a sincronização entre o estado do registo e do pedido associado.
        /// </remarks>
        public async Task AtualizarEstadoPedidoSeRegistoResolvido(int registoId)
        {
            var registo = await _context.RegistosManutencao
                .Include(r => r.PedidoManutencaoPedidoManut) // Para aceder ao pedido
                    .ThenInclude(p => p.MaquinaMaquina)      // Para aceder à máquina
                .FirstOrDefaultAsync(r => r.RegistoManutencaoId == registoId);

            if (registo == null)
                throw new Exception("Registo de manutenção não encontrado.");

            if (registo.Estado == "Resolvido")
            {
                var pedido = registo.PedidoManutencaoPedidoManut;

                if (pedido == null)
                    throw new Exception("Pedido de manutenção associado não encontrado.");

                pedido.Estado = "Concluido";
                pedido.DataConclusao = DateTime.Now;

                await _context.SaveChangesAsync();

                // Enviar notificação para produção
                var nomeMaquina = pedido.MaquinaMaquina?.Nome ?? "Desconhecida";
                var mensagemResolucao = registo.Descricao ?? "Sem mensagem.";

                var mensagem = $"✅ Pedido Concluído\nID: {pedido.PedidoManutId}\nMáquina: {nomeMaquina}\n" +
                               $"\nResolução: {mensagemResolucao}";

                await _telegramService.EnviarMensagemAsync(mensagem, "Producao");
            }
        }
        #endregion

    }
}
