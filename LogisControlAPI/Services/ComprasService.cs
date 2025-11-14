using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Data;
using LogisControlAPI.DTO;
using LogisControlAPI.Models;
using LogisControlAPI.Interfaces;

namespace LogisControlAPI.Services
{
    /// <summary>
    /// Serviço que encapsula toda a lógica de negócio para a gestão de compras.
    /// </summary>
    public class ComprasService
    {
        private readonly LogisControlContext _ctx;
        private readonly IEmailSender _emailSender;

        public ComprasService(LogisControlContext ctx, IEmailSender emailSender)
        {
            _ctx = ctx;
            _emailSender = emailSender;
        }

        /// Lista todos os pedidos de compra conforme o estado especificado.
        /// </summary>
        /// <param name="estado">Estado atual do pedido (Aberto, EmCotacao, etc.)</param>
        /// <returns>Lista de pedidos de compra no estado indicado.</returns>
        public async Task<List<PedidoCompraDTO>> ListarPedidosPorEstadoAsync(string estado)
        {
            var query = _ctx.PedidosCompra
                .Include(p => p.UtilizadorUtilizador)
                .AsQueryable();

            switch (estado)
            {
                case "Aberto":
                    query = query.Where(p => p.Estado == "Aberto");
                    break;

                case "EmCotacao":
                    query = query.Where(p => p.Estado == "EmCotacao");
                    break;

                case "ComOrcamentos":
                    query = query.Where(p => p.Estado == "ComOrcamentos");
                    break;

                case "Concluido":
                    query = query.Where(p => p.Estado == "Concluido");
                    break;
            }

            return await query
                .Select(p => new PedidoCompraDTO
                {
                    PedidoCompraId = p.PedidoCompraId,
                    Descricao = p.Descricao,
                    Estado = p.Estado,
                    DataAbertura = p.DataAbertura,
                    DataConclusao = p.DataConclusao,
                    NomeUtilizador = $"{p.UtilizadorUtilizador.PrimeiroNome} {p.UtilizadorUtilizador.Sobrenome}"
                })
                .ToListAsync();
        }

        /// <summary>
        /// Obtém o detalhe completo de um pedido de compra, incluindo itens.
        /// </summary>
        public async Task<PedidoCompraDetalheDTO?> ObterPedidoCompraDetalheAsync(int id)
        {
            var pedido = await _ctx.PedidosCompra
                .AsNoTracking()
                .Include(p => p.UtilizadorUtilizador)
                .Include(p => p.PedidoCompraItems)
                    .ThenInclude(i => i.MateriaPrima)
                .FirstOrDefaultAsync(p => p.PedidoCompraId == id);

            if (pedido == null)
                return null;

            return new PedidoCompraDetalheDTO
            {
                PedidoCompraId = pedido.PedidoCompraId,
                Descricao = pedido.Descricao,
                Estado = pedido.Estado,
                DataAbertura = pedido.DataAbertura,
                DataConclusao = pedido.DataConclusao,
                NomeUtilizador = $"{pedido.UtilizadorUtilizador.PrimeiroNome} {pedido.UtilizadorUtilizador.Sobrenome}",
                Itens = pedido.PedidoCompraItems.Select(i => new ItemPedidoDetalheDTO
                {
                    MateriaPrimaId = i.MateriaPrimaId,
                    MateriaPrimaNome = i.MateriaPrima.Nome,
                    Quantidade = i.Quantidade
                }).ToList()
            };
        }

        /// <summary>
        /// Cria um novo pedido de compra (cabeçalho + itens).
        /// </summary>
        public async Task<int> CriarPedidoCompraAsync(CriarPedidoCompraDTO dto)
        {

            // Validação: descrição obrigatória
            if (string.IsNullOrWhiteSpace(dto.Descricao))
                throw new Exception("A descrição é obrigatória.");

            // Validação: verificar se o utilizador existe
            var utilizadorExiste = await _ctx.Utilizadores.AnyAsync(u => u.UtilizadorId == dto.UtilizadorId);
            if (!utilizadorExiste)
                throw new Exception("Utilizador não encontrado.");

            // Validação: lista de itens nula
            if (dto.Itens == null || !dto.Itens.Any())
                throw new Exception("É necessário adicionar pelo menos um item ao pedido.");

            // Validação: quantidades inválidas
            if (dto.Itens.Any(i => i.Quantidade <= 0))
                throw new Exception("Todos os itens devem ter uma quantidade superior a zero.");

            // Validação: verificar se todas as matérias-primas existem
            var idsMaterias = dto.Itens.Select(i => i.MateriaPrimaId).ToList();
            var materiasExistentes = await _ctx.MateriasPrimas
                .Where(mp => idsMaterias.Contains(mp.MateriaPrimaId))
                .Select(mp => mp.MateriaPrimaId)
                .ToListAsync();

            var materiasInexistentes = idsMaterias.Except(materiasExistentes).ToList();
            if (materiasInexistentes.Any())
                throw new Exception($"Matéria-prima(s) não encontrada(s): {string.Join(", ", materiasInexistentes)}");


            // 1) Cria o pedido
            var pedido = new PedidoCompra
            {
                Descricao = dto.Descricao,
                DataAbertura = DateTime.UtcNow,
                Estado = "Aberto",
                UtilizadorUtilizadorId = dto.UtilizadorId
            };
            _ctx.PedidosCompra.Add(pedido);
            await _ctx.SaveChangesAsync();

            // 2) Cria e guarda cada item
            foreach (var item in dto.Itens)
            {
                _ctx.PedidoCompraItems.Add(new PedidoCompraItem
                {
                    PedidoCompraId = pedido.PedidoCompraId,
                    MateriaPrimaId = item.MateriaPrimaId,
                    Quantidade = item.Quantidade
                });
            }
            await _ctx.SaveChangesAsync();

            return pedido.PedidoCompraId;
        }

        /// <summary>
        /// Gera um pedido de cotação: muda o estado do pedido de compra,
        /// verifica existência do fornecedor, cria o cabeçalho em PedidoCotacao
        /// e retorna também um token de acesso.
        /// </summary>
        public async Task<(int CotacaoId, string Token)> CriarPedidoCotacaoAsync(int pedidoCompraId, int fornecedorId)
        {
            // 1) Verifica se o pedido de compra existe e está "Aberto"
            var pedido = await _ctx.PedidosCompra.FindAsync(pedidoCompraId);
            if (pedido == null)
                throw new KeyNotFoundException($"Pedido de compra {pedidoCompraId} não encontrado.");
            if (pedido.Estado != "Aberto")
                throw new InvalidOperationException("Pedido deve estar em estado 'Aberto' para gerar cotação.");

            // 2) Verifica se o fornecedor existe
            bool fornecedorExiste = await _ctx.Fornecedores
                .AnyAsync(f => f.FornecedorId == fornecedorId);
            if (!fornecedorExiste)
                throw new KeyNotFoundException($"Fornecedor {fornecedorId} não encontrado.");

            // 3) Atualiza o estado do pedido de compra e persiste imediatamente
            pedido.Estado = "EmCotacao";
            await _ctx.SaveChangesAsync();

            // 4) Gera token e cria o cabeçalho em PedidoCotacao
            var token = Guid.NewGuid().ToString("N");
            var cotacao = new PedidoCotacao
            {
                PedidoCompraId = pedido.PedidoCompraId,
                Descricao = pedido.Descricao,
                Data = DateTime.UtcNow,
                Estado = "Emitido",
                FornecedorId = fornecedorId,
                TokenAcesso = token
            };
            _ctx.PedidosCotacao.Add(cotacao);
            await _ctx.SaveChangesAsync();
            Console.WriteLine($"Cotação criada: ID = {cotacao.PedidoCotacaoId}, FK Pedido = {cotacao.PedidoCompraId}, Fornecedor = {cotacao.FornecedorId}");

            // 5) Enviar email ao fornecedor
            var fornecedor = await _ctx.Fornecedores.FindAsync(fornecedorId);
            if (fornecedor != null && !string.IsNullOrWhiteSpace(fornecedor.Email))
            {
                var link = $"http://localhost:5173/fornecedor/cotacao/{cotacao.PedidoCotacaoId}?token={token}";

                var mensagem = $"Caro fornecedor {fornecedor.Nome},\n\n" +
                               $"Foi-lhe atribuído um pedido de cotação. " +
                               $"Clique no link abaixo para aceder ao pedido:\n\n{link}\n\n" +
                               "Este link é exclusivo para si.";

                try
                {
                    Console.WriteLine($"A enviar e-mail para: {fornecedor.Email}");
                    await _emailSender.EnviarAsync(fornecedor.Email, "Novo Pedido de Cotação", mensagem);
                    Console.WriteLine("Email enviado com sucesso!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERRO AO ENVIAR EMAIL:");
                    Console.WriteLine(ex.ToString());
                }

            }


            return (cotacao.PedidoCotacaoId, token);
        }

        /// <summary>
        /// Obtém um pedido de cotação com orçamentos e seus itens.
        /// </summary>
        public async Task<PedidoCotacaoDetalhadoDTO> ObterPedidoCotacaoDetalhadoAsync(int cotacaoId)
        {
            var cot = await _ctx.PedidosCotacao
                .AsNoTracking()
                .Include(c => c.Fornecedor) // <-- adicionar isto
                .Include(c => c.Orcamentos)
                    .ThenInclude(o => o.OrcamentoItems)
                    .ThenInclude(i => i.MateriaPrima)
                .FirstOrDefaultAsync(c => c.PedidoCotacaoId == cotacaoId);

            if (cot == null)
                throw new KeyNotFoundException($"Pedido de cotação {cotacaoId} não encontrado.");

            return new PedidoCotacaoDetalhadoDTO
            {
                Header = new PedidoCotacaoDTO
                {
                    PedidoCotacaoID = cot.PedidoCotacaoId,
                    Descricao = cot.Descricao,
                    Data = cot.Data,
                    Estado = cot.Estado,
                    FornecedorID = cot.FornecedorId,
                    TokenAcesso = cot.TokenAcesso,
                    FornecedorNome = cot.Fornecedor.Nome
                },
                Orcamentos = cot.Orcamentos.Select(o => new OrcamentoDTO
                {
                    OrcamentoID = o.OrcamentoID,
                    PedidoCotacaoID = o.PedidoCotacaoPedidoCotacaoID,
                    Data = o.Data,
                    Estado = o.Estado
                }).ToList(),
                Itens = cot.Orcamentos
                    .SelectMany(o => o.OrcamentoItems)
                    .Select(i => new OrcamentoItemDTO
                    {
                        OrcamentoItemID = i.OrcamentoItemID,
                        OrcamentoID = i.OrcamentoOrcamentoID,
                        MateriaPrimaID = i.MateriaPrimaID,
                        Quantidade = i.Quantidade,
                        PrecoUnit = i.PrecoUnit,
                        PrazoEntrega = i.PrazoEntrega ?? 0,
                        MateriaPrimaNome = i.MateriaPrima?.Nome
                    })
                    .ToList()
            };
        }

        public async Task<PedidoCotacaoDetalhadoDTO> ObterPedidoCotacaoParaFornecedorAsync(int id, string token)
        {
            Console.WriteLine("[Fornecedor] A iniciar validação da cotação...");
            Console.WriteLine($"ID Recebido: {id}");
            Console.WriteLine($"Token Recebido: {token}");

            // 1) Buscar cotação com orçamentos
            var cot = await _ctx.PedidosCotacao
                .Include(c => c.Fornecedor)
                .Include(c => c.Orcamentos)
                    .ThenInclude(o => o.OrcamentoItems)
                .FirstOrDefaultAsync(c => c.PedidoCotacaoId == id);

            if (cot == null)
                throw new KeyNotFoundException();

            if (!string.Equals(cot.TokenAcesso.Trim(), token.Trim(), StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException();

            Console.WriteLine("Token válido. Cotação encontrada.");

            // 2) Buscar pedido de compra com itens e matéria-prima
            var pedidoCompra = await _ctx.PedidosCompra
                .Include(p => p.PedidoCompraItems)
                    .ThenInclude(i => i.MateriaPrima)
                .FirstOrDefaultAsync(p => p.PedidoCompraId == cot.PedidoCompraId);

            if (pedidoCompra == null)
            {
                Console.WriteLine("Pedido de compra não encontrado!");
                throw new Exception("Pedido de compra correspondente não encontrado.");
            }

            Console.WriteLine("Pedido de compra correspondente encontrado.");
            Console.WriteLine($"Total de itens do pedido: {pedidoCompra.PedidoCompraItems.Count}");

            foreach (var item in pedidoCompra.PedidoCompraItems)
            {
                Console.WriteLine($"Item: {item.MateriaPrimaId} - Qtd: {item.Quantidade} - Nome: {item.MateriaPrima?.Nome}");
            }

            // 3) Construir DTO de resposta
            return new PedidoCotacaoDetalhadoDTO
            {
                Header = new PedidoCotacaoDTO
                {
                    PedidoCotacaoID = cot.PedidoCotacaoId,
                    Descricao = cot.Descricao,
                    Data = cot.Data,
                    Estado = cot.Estado,
                    FornecedorID = cot.FornecedorId,
                    TokenAcesso = cot.TokenAcesso,
                    FornecedorNome = cot.Fornecedor?.Nome
                },
                Orcamentos = cot.Orcamentos.Select(o => new OrcamentoDTO
                {
                    OrcamentoID = o.OrcamentoID,
                    PedidoCotacaoID = o.PedidoCotacaoPedidoCotacaoID,
                    Data = o.Data,
                    Estado = o.Estado
                }).ToList(),
                Itens = pedidoCompra.PedidoCompraItems
                    .Select(i => new OrcamentoItemDTO
                    {
                        MateriaPrimaID = i.MateriaPrimaId,
                        MateriaPrimaNome = i.MateriaPrima?.Nome ?? "(sem nome)",
                        Quantidade = i.Quantidade,
                        PrecoUnit = 0,
                        PrazoEntrega = 0
                    }).ToList()
            };
        }

        public async Task<PedidoCotacao?> ObterCotacaoPorPedidoCompraAsync(int pedidoCompraId)
        {
            return await _ctx.PedidosCotacao
                .AsNoTracking()
                .Where(c => c.PedidoCompraId == pedidoCompraId)
                .OrderByDescending(c => c.Data) // caso existam várias
                .FirstOrDefaultAsync();
        }

        public async Task<int> AceitarOrcamentoAsync(int orcamentoId)
        {
            // 1) Carrega orçamento com itens e cotação associada
            var orc = await _ctx.Orcamentos
                .Include(o => o.OrcamentoItems)
                .Include(o => o.PedidoCotacaoPedidoCotacao)
                    .ThenInclude(pc => pc.PedidoCompra)
                .FirstOrDefaultAsync(o => o.OrcamentoID == orcamentoId);

            if (orc == null)
                throw new KeyNotFoundException("Orçamento não encontrado.");

            var pedidoCotacao = orc.PedidoCotacaoPedidoCotacao;
            if (pedidoCotacao == null)
                throw new InvalidOperationException("Orçamento não está associado a uma cotação.");

            // 2) Recusar todos os outros orçamentos do mesmo pedido de cotação
            var todosOrcamentos = await _ctx.Orcamentos
                .Where(o => o.PedidoCotacaoPedidoCotacaoID == pedidoCotacao.PedidoCotacaoId)
                .ToListAsync();

            foreach (var o in todosOrcamentos)
                o.Estado = (o.OrcamentoID == orcamentoId ? "Aceite" : "Recusado");

            // 3) Atualizar o estado da cotação
            pedidoCotacao.Estado = "Finalizado";

            // 4) Atualizar o estado do pedido de compra
            var pedidoCompra = pedidoCotacao.PedidoCompra;
            if (pedidoCompra != null)
            {
                pedidoCompra.Estado = "Concluido";
                pedidoCompra.DataConclusao = DateTime.UtcNow;
            }

            // 5) Criar Nota de Encomenda
            var nota = new NotaEncomenda
            {
                DataEmissao = DateTime.UtcNow,
                Estado = "Pendente",
                OrcamentoId = orcamentoId,
                ValorTotal = orc.OrcamentoItems.Sum(i => i.Quantidade * i.PrecoUnit)
            };
            _ctx.NotasEncomenda.Add(nota);
            await _ctx.SaveChangesAsync();

            // 6) Criar itens da nota
            foreach (var item in orc.OrcamentoItems)
            {
                _ctx.NotasEncomendaItem.Add(new NotaEncomendaItens
                {
                    NotaEncomendaId = nota.NotaEncomendaId,
                    MateriaPrimaId = item.MateriaPrimaID,
                    Quantidade = item.Quantidade,
                    PrecoUnit = item.PrecoUnit
                });
            }

            // 7) Persistir tudo
            await _ctx.SaveChangesAsync();

            return nota.NotaEncomendaId;
        }


        public async Task<NotaEncomendaDetalheDTO> ObterNotaEncomendaAsync(int id)
        {
            var nota = await _ctx.NotasEncomenda
                .Include(n => n.Itens).ThenInclude(it => it.MateriaPrima)
                .FirstOrDefaultAsync(n => n.NotaEncomendaId == id);
            if (nota == null) throw new KeyNotFoundException();

            return new NotaEncomendaDetalheDTO
            {
                NotaEncomendaId = nota.NotaEncomendaId,
                DataEmissao = nota.DataEmissao,
                Estado = nota.Estado,
                ValorTotal = nota.ValorTotal,
                OrcamentoId = nota.OrcamentoId,
                Itens = nota.Itens.Select(it => new NotaEncomendaItemDTO
                {
                    MateriaPrimaId = it.MateriaPrimaId,
                    MateriaPrimaNome = it.MateriaPrima.Nome,
                    Quantidade = it.Quantidade,
                    PrecoUnit = it.PrecoUnit
                }).ToList()
            };
        }

        public async Task<NotaEncomendaDetalheDTO?> ObterNotaPorOrcamentoAsync(int orcamentoId)
        {
            var nota = await _ctx.NotasEncomenda
                .Include(n => n.Itens)
                .ThenInclude(i => i.MateriaPrima)
                .FirstOrDefaultAsync(n => n.OrcamentoId == orcamentoId);

            if (nota == null)
                return null;

            return new NotaEncomendaDetalheDTO
            {
                NotaEncomendaId = nota.NotaEncomendaId,
                DataEmissao = nota.DataEmissao,
                Estado = nota.Estado,
                ValorTotal = nota.ValorTotal,
                OrcamentoId = nota.OrcamentoId,
                Itens = nota.Itens.Select(i => new NotaEncomendaItemDTO
                {
                    MateriaPrimaId = i.MateriaPrimaId,
                    MateriaPrimaNome = i.MateriaPrima.Nome,
                    Quantidade = i.Quantidade,
                    PrecoUnit = i.PrecoUnit
                }).ToList()
            };
        }
        public async Task<bool> ReceberNotaEncomendaAsync(int id, bool emBoasCondicoes)
        {
            var nota = await _ctx.NotasEncomenda
                .Include(n => n.Itens)
                .Include(n => n.Orcamento)
                    .ThenInclude(o => o.PedidoCotacaoPedidoCotacao)
                    .ThenInclude(pc => pc.PedidoCompra)
                .FirstOrDefaultAsync(n => n.NotaEncomendaId == id);

            if (nota == null)
                throw new KeyNotFoundException();

            if (nota.Estado != "Pendente")
                throw new InvalidOperationException("Nota já foi processada.");

            if (emBoasCondicoes)
            {
                nota.Estado = "Recebida";

                foreach (var item in nota.Itens)
                {
                    var materia = await _ctx.MateriasPrimas.FindAsync(item.MateriaPrimaId);
                    if (materia != null)
                        materia.Quantidade += item.Quantidade;
                }

                var pedido = nota.Orcamento?.PedidoCotacaoPedidoCotacao?.PedidoCompra;
                if (pedido != null && pedido.Estado != "Recebido")
                {
                    pedido.Estado = "Recebido";
                    pedido.DataConclusao = DateTime.UtcNow;
                }
            }
            else
            {
                //Garante que não envia o email mais do que uma vez
                if (nota.Estado == "Reclamada" || nota.Estado == "Reentregue")
                    throw new InvalidOperationException("Esta nota já foi reclamada ou reenviada anteriormente.");

                nota.Estado = "Reclamada";
                await EnviarEmailReclamacaoFornecedorAsync(nota.NotaEncomendaId);
            }

            await _ctx.SaveChangesAsync();
            return true;
        }

        public async Task<List<NotaEncomendaDetalheDTO>> ListarNotasPendentesPorMateriaAsync(int materiaPrimaId)
        {
            var notas = await _ctx.NotasEncomenda
                .Where(n => n.Estado == "Pendente" && n.Itens.Any(i => i.MateriaPrimaId == materiaPrimaId))
                .Include(n => n.Itens)
                .ThenInclude(i => i.MateriaPrima)
                .ToListAsync();

            return notas.Select(n => new NotaEncomendaDetalheDTO
            {
                NotaEncomendaId = n.NotaEncomendaId,
                DataEmissao = n.DataEmissao,
                Estado = n.Estado,
                ValorTotal = n.ValorTotal,
                OrcamentoId = n.OrcamentoId,
                Itens = n.Itens.Select(i => new NotaEncomendaItemDTO
                {
                    MateriaPrimaId = i.MateriaPrimaId,
                    MateriaPrimaNome = i.MateriaPrima.Nome,
                    Quantidade = i.Quantidade,
                    PrecoUnit = i.PrecoUnit
                }).ToList()
            }).ToList();
        }

        public async Task<List<NotaEncomendaDetalheDTO>> ObterNotasPendentesAsync()
        {
            return await _ctx.NotasEncomenda
                .Where(n => n.Estado == "Pendente")
                .Include(n => n.Itens)
                    .ThenInclude(i => i.MateriaPrima)
                .Select(n => new NotaEncomendaDetalheDTO
                {
                    NotaEncomendaId = n.NotaEncomendaId,
                    DataEmissao = n.DataEmissao,
                    Estado = n.Estado,
                    ValorTotal = n.ValorTotal,
                    OrcamentoId = n.OrcamentoId,
                    Itens = n.Itens.Select(i => new NotaEncomendaItemDTO
                    {
                        MateriaPrimaId = i.MateriaPrimaId,
                        MateriaPrimaNome = i.MateriaPrima.Nome,
                        Quantidade = i.Quantidade,
                        PrecoUnit = i.PrecoUnit
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<NotaEncomendaDetalheDTO>> ObterNotasPorEstadoAsync(string estado)
        {
            return await _ctx.NotasEncomenda
                .Where(n => n.Estado == estado)
                .Include(n => n.Itens).ThenInclude(i => i.MateriaPrima)
                .Select(n => new NotaEncomendaDetalheDTO
                {
                    NotaEncomendaId = n.NotaEncomendaId,
                    DataEmissao = n.DataEmissao,
                    Estado = n.Estado,
                    ValorTotal = n.ValorTotal,
                    OrcamentoId = n.OrcamentoId,
                    Itens = n.Itens.Select(i => new NotaEncomendaItemDTO
                    {
                        MateriaPrimaId = i.MateriaPrimaId,
                        MateriaPrimaNome = i.MateriaPrima.Nome,
                        Quantidade = i.Quantidade,
                        PrecoUnit = i.PrecoUnit
                    }).ToList()
                })
                .ToListAsync();
        }

        /// <summary>
        /// Envia um e-mail ao fornecedor a informar que a nota de encomenda foi reclamada,
        /// solicitando nova entrega dos materiais.
        /// Garante que o e-mail só é enviado uma vez.
        /// </summary>
        /// <param name="notaId">Identificador da nota de encomenda reclamada.</param>
        /// <returns>True se o e-mail for enviado com sucesso; False caso contrário.</returns>
        /// <exception cref="InvalidOperationException">Se a nota não existir, não estiver reclamada ou já houver nova entrega.</exception>
        /// <exception cref="Exception">Se o fornecedor não tiver email válido.</exception>
        public async Task<bool> EnviarEmailReclamacaoFornecedorAsync(int notaId)
        {
            var nota = await _ctx.NotasEncomenda
                .Include(n => n.Orcamento)
                    .ThenInclude(o => o.PedidoCotacaoPedidoCotacao)
                    .ThenInclude(pc => pc.Fornecedor)
                .FirstOrDefaultAsync(n => n.NotaEncomendaId == notaId);

            if (nota == null)
                throw new InvalidOperationException("Nota não encontrada.");

            if (nota.Estado != "Reclamada")
                throw new InvalidOperationException("A nota não está em estado 'Reclamada'.");

            // ❗ Evitar envio se já foi reenviado
            var jaReentregue = await _ctx.NotasEncomenda
                .AnyAsync(n => n.OrcamentoId == nota.OrcamentoId && n.Estado == "Pendente" && n.NotaEncomendaId != notaId);

            if (jaReentregue)
                throw new InvalidOperationException("Já foi registada nova entrega para esta nota.");

            var fornecedor = nota.Orcamento?.PedidoCotacaoPedidoCotacao?.Fornecedor;
            if (fornecedor == null || string.IsNullOrWhiteSpace(fornecedor.Email))
                throw new Exception("Fornecedor não encontrado ou sem email.");

            var link = $"http://localhost:5173/fornecedor/nova-entrega/{nota.NotaEncomendaId}";
            var mensagem = $"""
                Caro fornecedor {fornecedor.Nome},

                Foi detetado um problema com a entrega da nota #{nota.NotaEncomendaId}.
                Solicitamos nova entrega dos materiais.

                Por favor, aceda ao seguinte link para confirmar a nova entrega:

                {link}

                Este link é exclusivo para si.
                """;

            try
            {
                Console.WriteLine($"A enviar e-mail para: {fornecedor.Email}");
                await _emailSender.EnviarAsync(fornecedor.Email, "Reclamação de Entrega - Nova Ação Requerida", mensagem);
                Console.WriteLine("Email enviado com sucesso!");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERRO AO ENVIAR EMAIL:");
                Console.WriteLine(ex.ToString());
                return false;
            }
        }


        public async Task<bool> ConfirmarNovaEntregaAsync(int notaId)
        {
            var notaOriginal = await _ctx.NotasEncomenda
                .Include(n => n.Itens)
                .FirstOrDefaultAsync(n => n.NotaEncomendaId == notaId);

            if (notaOriginal == null || notaOriginal.Estado != "Reclamada")
                throw new InvalidOperationException("Nota inválida ou não reclamada.");

            // Marcar nota original como "Reclamada-Finalizada".
            notaOriginal.Estado = "Reentregue";
            await _ctx.SaveChangesAsync();

            // Criar nova nota com os mesmos dados
            var novaNota = new NotaEncomenda
            {
                DataEmissao = DateTime.UtcNow,
                Estado = "Pendente", // para que o operador a valide
                OrcamentoId = notaOriginal.OrcamentoId,
                ValorTotal = notaOriginal.ValorTotal
            };
            _ctx.NotasEncomenda.Add(novaNota);
            await _ctx.SaveChangesAsync();

            foreach (var item in notaOriginal.Itens)
            {
                _ctx.NotasEncomendaItem.Add(new NotaEncomendaItens
                {
                    NotaEncomendaId = novaNota.NotaEncomendaId,
                    MateriaPrimaId = item.MateriaPrimaId,
                    Quantidade = item.Quantidade,
                    PrecoUnit = item.PrecoUnit
                });
            }

            await _ctx.SaveChangesAsync();
            return true;
        }

    }
}