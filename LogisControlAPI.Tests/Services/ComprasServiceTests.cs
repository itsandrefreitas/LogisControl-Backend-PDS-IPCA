using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Data;
using LogisControlAPI.Services;
using LogisControlAPI.DTO;
using LogisControlAPI.Models;
using LogisControlAPI.Interfaces;
using Microsoft.VisualStudio.CodeCoverage;
using static Azure.Core.HttpHeader;
using System;

/// <summary>
/// Testes unitários para o serviço de ComprasService.
/// Valida os comportamentos esperados para criação, listagem e obtenção de pedidos de compra.
/// </summary>
public class ComprasServiceTests
{
    /// <summary>
    /// Cria um contexto em memória para testes isolados.
    /// </summary>
    private LogisControlContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<LogisControlContext>()
            .UseInMemoryDatabase("TestDb_" + System.Guid.NewGuid())
            .Options;
        return new LogisControlContext(options);
    }


    #region Pedidos Compra

    /// <summary>
    /// Garante que apenas pedidos com o estado especificado são devolvidos.
    /// </summary>
    [Fact]
    public async Task ListarPedidosPorEstadoAsync_DeveRetornarSomentePedidosDoEstado()
    {
        var context = GetInMemoryDbContext();

        context.Utilizadores.Add(new Utilizador
        {
            UtilizadorId = 1,
            PrimeiroNome = "João",
            Sobrenome = "Silva",
            Password = "hashedpassword",
            Role = "Gestor",
        });

        context.PedidosCompra.AddRange(
            new PedidoCompra
            {
                PedidoCompraId = 1,
                Estado = "Aberto",
                Descricao = "Pedido A",
                UtilizadorUtilizadorId = 1,
                DataAbertura = DateTime.UtcNow
            },
            new PedidoCompra
            {
                PedidoCompraId = 2,
                Estado = "Concluido",
                Descricao = "Pedido B",
                UtilizadorUtilizadorId = 1,
                DataAbertura = DateTime.UtcNow
            }
        );

        await context.SaveChangesAsync();

        var emailSenderMock = new Mock<IEmailSender>();
        var service = new ComprasService(context, emailSenderMock.Object);

        var resultado = await service.ListarPedidosPorEstadoAsync("Aberto");

        Assert.Single(resultado);
        Assert.Equal("Pedido A", resultado.First().Descricao);
        Assert.Equal("João Silva", resultado.First().NomeUtilizador);
    }

    /// <summary>
    /// Deve retornar lista vazia se não houver nenhum pedido com o estado.
    /// </summary>
    [Fact]
    public async Task ListarPedidosPorEstadoAsync_DeveRetornarVazio_ParaEstadoInvalido()
    {
        var context = GetInMemoryDbContext();
        var emailSenderMock = new Mock<IEmailSender>();
        var service = new ComprasService(context, emailSenderMock.Object);

        var resultado = await service.ListarPedidosPorEstadoAsync("EstadoInexistente");

        Assert.Empty(resultado);
    }


    /// <summary>
    /// Garante que retorna null quando o pedido não for encontrado.
    /// </summary>
    [Fact]
    public async Task ObterPedidoCompraDetalheAsync_DeveRetornarNull_SeNaoEncontrar()
    {
        var context = GetInMemoryDbContext();
        var emailSenderMock = new Mock<IEmailSender>();
        var service = new ComprasService(context, emailSenderMock.Object);

        var resultado = await service.ObterPedidoCompraDetalheAsync(999); // inexistente

        Assert.Null(resultado);
    }

    /// <summary>
    /// Garante que os detalhes do pedido são devolvidos corretamente se o ID existir.
    /// </summary>
    [Fact]
    public async Task ObterPedidoCompraDetalheAsync_DeveRetornarDetalhes_SeEncontrar()
    {
        var context = GetInMemoryDbContext();

        var materiaPrima = new MateriaPrima
        {
            MateriaPrimaId = 1,
            Nome = "Aço",
            Categoria = "Metais",
            CodInterno = "AC001",
            Descricao = "Aço carbono de alta resistência"
        };

        var utilizador = new Utilizador
        {
            UtilizadorId = 1,
            PrimeiroNome = "Maria",
            Sobrenome = "Silva",
            Password = "hashed",
            Role = "Gestor"
        };

        context.MateriasPrimas.Add(materiaPrima);
        context.Utilizadores.Add(utilizador);

        context.PedidosCompra.Add(new PedidoCompra
        {
            PedidoCompraId = 1,
            Descricao = "Compra A",
            Estado = "Aberto",
            DataAbertura = DateTime.UtcNow,
            UtilizadorUtilizadorId = 1,
            PedidoCompraItems = new List<PedidoCompraItem>
            {
                new PedidoCompraItem
                {
                    MateriaPrimaId = 1,
                    Quantidade = 5,
                    MateriaPrima = materiaPrima
                }
            }
        });

        await context.SaveChangesAsync();

        var emailSenderMock = new Mock<IEmailSender>();
        var service = new ComprasService(context, emailSenderMock.Object);

        var resultado = await service.ObterPedidoCompraDetalheAsync(1);

        Assert.NotNull(resultado);
        Assert.Equal("Compra A", resultado.Descricao);
        Assert.Single(resultado.Itens);
        Assert.Equal("Aço", resultado.Itens.First().MateriaPrimaNome);
    }

    

   
    /// <summary>
    /// Garante que um pedido com itens é criado corretamente.
    /// </summary>
    [Fact]
    public async Task CriarPedidoCompraAsync_DeveCriarPedidoComItens()
    {
        var context = GetInMemoryDbContext();

        context.Utilizadores.Add(new Utilizador
        {
            UtilizadorId = 1,
            PrimeiroNome = "Carlos",
            Sobrenome = "Souza",
            Password = "hashed",
            Role = "Gestor"
        });

        context.MateriasPrimas.Add(new MateriaPrima
        {
            MateriaPrimaId = 1,
            Nome = "Aço",
            Categoria = "Metais",
            CodInterno = "M001",
            Descricao = "Metal"
        });

        await context.SaveChangesAsync();

        var emailSenderMock = new Mock<IEmailSender>();
        var service = new ComprasService(context, emailSenderMock.Object);

        var dto = new CriarPedidoCompraDTO
        {
            UtilizadorId = 1,
            Descricao = "Nova compra",
            Itens = new List<ItemPedidoDTO>
            {
                new ItemPedidoDTO { MateriaPrimaId = 1, Quantidade = 3 }
            }
        };

        var id = await service.CriarPedidoCompraAsync(dto);

        var pedidoCriado = await context.PedidosCompra.Include(p => p.PedidoCompraItems).FirstOrDefaultAsync(p => p.PedidoCompraId == id);

        Assert.NotNull(pedidoCriado);
        Assert.Single(pedidoCriado.PedidoCompraItems);
        Assert.Equal("Nova compra", pedidoCriado.Descricao);
    }

    /// <summary>
/// Deve lançar exceção ao tentar criar um pedido sem itens.
/// </summary>
[Fact]
public async Task CriarPedidoCompraAsync_DeveLancarExcecao_SeSemItens()
{
    var context = GetInMemoryDbContext();
    context.Utilizadores.Add(new Utilizador
    {
        UtilizadorId = 1,
        PrimeiroNome = "Ana",
        Sobrenome = "Costa",
        Password = "abc",
        Role = "Gestor"
    });
    await context.SaveChangesAsync();

    var service = new ComprasService(context, new Mock<IEmailSender>().Object);

    var dto = new CriarPedidoCompraDTO
    {
        UtilizadorId = 1,
        Descricao = "Pedido Sem Itens",
        Itens = new List<ItemPedidoDTO>() // lista vazia
    };

    var ex = await Assert.ThrowsAsync<Exception>(() => service.CriarPedidoCompraAsync(dto));
    Assert.Equal("É necessário adicionar pelo menos um item ao pedido.", ex.Message);
}


    /// <summary>
    /// Deve lançar exceção se uma matéria-prima referenciada não existir.
    /// </summary>
    [Fact]
    public async Task CriarPedidoCompraAsync_DeveLancarExcecao_SeMateriaPrimaInvalida()
    {
        var context = GetInMemoryDbContext();

        context.Utilizadores.Add(new Utilizador
        {
            UtilizadorId = 1,
            PrimeiroNome = "Tiago",
            Sobrenome = "Lima",
            Password = "teste123",
            Role = "Gestor"
        });

        await context.SaveChangesAsync();

        var emailSenderMock = new Mock<IEmailSender>();
        var service = new ComprasService(context, emailSenderMock.Object);

        var dto = new CriarPedidoCompraDTO
        {
            UtilizadorId = 1,
            Descricao = "Pedido com matéria inválida",
            Itens = new List<ItemPedidoDTO>
        {
            new ItemPedidoDTO { MateriaPrimaId = 99, Quantidade = 2 } // não existe
        }
        };

        await Assert.ThrowsAsync<Exception>(() => service.CriarPedidoCompraAsync(dto));
    }

    /// <summary>
    /// Deve lançar exceção se a descrição estiver vazia.
    /// </summary>
    [Fact]
    public async Task CriarPedidoCompraAsync_DeveLancarExcecao_SeDescricaoVazia()
    {
        var context = GetInMemoryDbContext();
        context.Utilizadores.Add(new Utilizador { UtilizadorId = 1, PrimeiroNome = "Marta", Sobrenome = "Lopes", Password = "x", Role = "Gestor" });
        await context.SaveChangesAsync();

        var emailSenderMock = new Mock<IEmailSender>();
        var service = new ComprasService(context, emailSenderMock.Object);

        var dto = new CriarPedidoCompraDTO
        {
            UtilizadorId = 1,
            Descricao = "", // inválido
            Itens = new List<ItemPedidoDTO>()
        };

        await Assert.ThrowsAsync<Exception>(() => service.CriarPedidoCompraAsync(dto));
    }

    /// <summary>
    /// Deve lançar exceção se a lista de itens for vazia.
    /// </summary>
    [Fact]
    public async Task CriarPedidoCompraAsync_DeveLancarExcecao_SeListaItensVazia()
    {
        var context = GetInMemoryDbContext();
        context.Utilizadores.Add(new Utilizador { UtilizadorId = 1, PrimeiroNome = "Ana", Sobrenome = "Costa", Password = "abc", Role = "Gestor" });
        await context.SaveChangesAsync();

        var emailSenderMock = new Mock<IEmailSender>();
        var service = new ComprasService(context, emailSenderMock.Object);

        var dto = new CriarPedidoCompraDTO
        {
            UtilizadorId = 1,
            Descricao = "Pedido Vazio",
            Itens = new List<ItemPedidoDTO>() // vazio
        };

        await Assert.ThrowsAsync<Exception>(() => service.CriarPedidoCompraAsync(dto));
    }

    /// <summary>
    /// Deve lançar exceção se a lista de itens for nula.
    /// </summary>
    [Fact]
    public async Task CriarPedidoCompraAsync_DeveLancarExcecao_SeListaItensForNull()
    {
        var context = GetInMemoryDbContext();
        context.Utilizadores.Add(new Utilizador { UtilizadorId = 1, PrimeiroNome = "Luis", Sobrenome = "Ferreira", Password = "123", Role = "Gestor" });
        await context.SaveChangesAsync();

        var emailSenderMock = new Mock<IEmailSender>();
        var service = new ComprasService(context, emailSenderMock.Object);

        var dto = new CriarPedidoCompraDTO
        {
            UtilizadorId = 1,
            Descricao = "Itens nulos",
            Itens = null
        };

        await Assert.ThrowsAsync<Exception>(() => service.CriarPedidoCompraAsync(dto));
    }

    /// <summary>
    /// Deve lançar exceção se a quantidade de um item for zero.
    /// </summary>
    [Fact]
    public async Task CriarPedidoCompraAsync_DeveLancarExcecao_SeQuantidadeZero()
    {
        var context = GetInMemoryDbContext();
        context.Utilizadores.Add(new Utilizador { UtilizadorId = 1, PrimeiroNome = "Sofia", Sobrenome = "Lima", Password = "abc", Role = "Gestor" });
        context.MateriasPrimas.Add(new MateriaPrima { MateriaPrimaId = 1, Nome = "Cobre", Categoria = "Metais", CodInterno = "CB01", Descricao = "Descrição" });
        await context.SaveChangesAsync();

        var emailSenderMock = new Mock<IEmailSender>();
        var service = new ComprasService(context, emailSenderMock.Object);

        var dto = new CriarPedidoCompraDTO
        {
            UtilizadorId = 1,
            Descricao = "Quantidade Zero",
            Itens = new List<ItemPedidoDTO>
        {
            new ItemPedidoDTO { MateriaPrimaId = 1, Quantidade = 0 }
        }
        };

        await Assert.ThrowsAsync<Exception>(() => service.CriarPedidoCompraAsync(dto));
    }

    #endregion

    #region Cotações

    /// <summary>
    /// Deve retornar null se não existir nenhuma cotação para o pedido.
    /// </summary>
    [Fact]
    public async Task ObterCotacaoPorPedidoCompraAsync_DeveRetornarNull_SeNaoExistir()
    {
        var ctx = GetInMemoryDbContext();
        var service = new ComprasService(ctx, new Mock<IEmailSender>().Object);

        var result = await service.ObterCotacaoPorPedidoCompraAsync(99);

        Assert.Null(result);
    }

    /// <summary>
    /// Deve retornar a cotação mais recente para o pedido.
    /// </summary>
    [Fact]
    public async Task ObterCotacaoPorPedidoCompraAsync_DeveRetornarMaisRecente()
    {
        var ctx = GetInMemoryDbContext();
        ctx.PedidosCotacao.AddRange(
            new PedidoCotacao { PedidoCotacaoId = 1, PedidoCompraId = 5, Data = DateTime.UtcNow.AddDays(-1), Descricao = "desc", Estado = "Emitido", TokenAcesso = "abc" },
            new PedidoCotacao { PedidoCotacaoId = 2, PedidoCompraId = 5, Data = DateTime.UtcNow, Descricao = "desc", Estado = "Emitido", TokenAcesso = "def" });
        await ctx.SaveChangesAsync();

        var service = new ComprasService(ctx, new Mock<IEmailSender>().Object);
        var result = await service.ObterCotacaoPorPedidoCompraAsync(5);

        Assert.NotNull(result);
        Assert.Equal(2, result.PedidoCotacaoId);
    }

    /// <summary>
    /// Deve criar um pedido de cotação e retornar o ID e token gerado, se o pedido estiver em estado 'Aberto' e o fornecedor existir.
    /// </summary>
    [Fact]
    public async Task CriarPedidoCotacaoAsync_DeveCriarCotacao_SePedidoEFornecedorForemValidos()
    {
        // Arrange
        var ctx = GetInMemoryDbContext();

        // Dados simulados
        ctx.PedidosCompra.Add(new PedidoCompra
        {
            PedidoCompraId = 1,
            Estado = "Aberto",
            Descricao = "Compra A",
            DataAbertura = DateTime.UtcNow,
            UtilizadorUtilizadorId = 1
        });

        ctx.Fornecedores.Add(new Fornecedor
        {
            FornecedorId = 10,
            Nome = "Fornecedor X",
            Email = "x@email.com"
        });

        await ctx.SaveChangesAsync();

        var emailSenderMock = new Mock<IEmailSender>();
        var service = new ComprasService(ctx, emailSenderMock.Object);

        // Act
        var (cotacaoId, token) = await service.CriarPedidoCotacaoAsync(1, 10);

        // Assert
        var cotacao = await ctx.PedidosCotacao.FindAsync(cotacaoId);

        Assert.NotNull(cotacao);
        Assert.Equal(1, cotacao.PedidoCompraId);
        Assert.Equal("Emitido", cotacao.Estado);
        Assert.Equal(10, cotacao.FornecedorId);
        Assert.Equal(cotacao.TokenAcesso, token);
        Assert.Equal("EmCotacao", (await ctx.PedidosCompra.FindAsync(1))!.Estado);

        // Garante que email foi tentado enviar
        emailSenderMock.Verify(es => es.EnviarAsync(
            "x@email.com",
            It.Is<string>(s => s.Contains("Cotação")),
            It.IsAny<string>()), Times.Once);
    }

    #endregion

    #region Notas Encomenda

    /// <summary>
    /// Deve retornar uma nota de encomenda completa se existir.
    /// </summary>
    [Fact]
    public async Task ObterNotaPorOrcamentoAsync_DeveRetornarNota_SeExistir()
    {
        var ctx = GetInMemoryDbContext();
        ctx.MateriasPrimas.Add(new MateriaPrima { MateriaPrimaId = 1, Nome = "Ferro", Categoria = "Metais", CodInterno = "FER01", Descricao = "Ferro puro" });
        ctx.NotasEncomenda.Add(new NotaEncomenda
        {
            NotaEncomendaId = 1,
            OrcamentoId = 10,
            Estado = "Pendente",
            ValorTotal = 100,
            Itens = new List<NotaEncomendaItens>
            {
                new NotaEncomendaItens { MateriaPrimaId = 1, Quantidade = 5, PrecoUnit = 20 }
            }
        });
        await ctx.SaveChangesAsync();

        var service = new ComprasService(ctx, new Mock<IEmailSender>().Object);
        var result = await service.ObterNotaPorOrcamentoAsync(10);

        Assert.NotNull(result);
        Assert.Equal(100, result.ValorTotal);
        Assert.Single(result.Itens);
    }

    /// <summary>
    /// Deve retornar null se nenhuma nota existir para o orçamento.
    /// </summary>
    [Fact]
    public async Task ObterNotaPorOrcamentoAsync_DeveRetornarNull_SeNaoExistir()
    {
        var ctx = GetInMemoryDbContext();
        var service = new ComprasService(ctx, new Mock<IEmailSender>().Object);
        var result = await service.ObterNotaPorOrcamentoAsync(999);

        Assert.Null(result);
    }

    /// <summary>
    /// Deve listar apenas notas com estado "Pendente".
    /// </summary>
    [Fact]
    public async Task ObterNotasPendentesAsync_DeveRetornarApenasPendentes()
    {
        var ctx = GetInMemoryDbContext();

        // Instância única de MateriaPrima
        var materia = new MateriaPrima
        {
            MateriaPrimaId = 1,
            Nome = "Zinco",
            Categoria = "Metais",
            CodInterno = "ZNC01",
            Descricao = "Zinco refinado"
        };

        ctx.MateriasPrimas.Add(materia);

        ctx.NotasEncomenda.AddRange(
            new NotaEncomenda
            {
                NotaEncomendaId = 1,
                Estado = "Pendente",
                ValorTotal = 200,
                Itens = new List<NotaEncomendaItens>
                {
                new NotaEncomendaItens
                {
                    MateriaPrimaId = 1,
                    Quantidade = 10,
                    PrecoUnit = 20,
                    MateriaPrima = materia // Usa a mesma instância
                }
                }
            },
            new NotaEncomenda
            {
                NotaEncomendaId = 2,
                Estado = "Recebida",
                Itens = new List<NotaEncomendaItens>() // vazio mas obrigatório
            });

        await ctx.SaveChangesAsync();

        var service = new ComprasService(ctx, new Mock<IEmailSender>().Object);
        var notas = await service.ObterNotasPendentesAsync();

        Assert.Single(notas);
        Assert.Equal(200, notas.First().ValorTotal);
    }


    /// <summary>
    /// Deve retornar todas as notas que tenham o estado especificado.
    /// </summary>
    [Fact]
    public async Task ObterNotasPorEstadoAsync_DeveRetornarPorEstado()
    {
        var ctx = GetInMemoryDbContext();
        ctx.MateriasPrimas.Add(new MateriaPrima { MateriaPrimaId = 1, Nome = "Alumínio", Categoria = "Metais", CodInterno = "ALU01", Descricao = "Alumínio refinado" });
        ctx.NotasEncomenda.Add(new NotaEncomenda
        {
            NotaEncomendaId = 1,
            Estado = "Pendente",
            ValorTotal = 150,
            Itens = new List<NotaEncomendaItens>
            {
                new NotaEncomendaItens { MateriaPrimaId = 1, Quantidade = 5, PrecoUnit = 30 }
            }
        });
        await ctx.SaveChangesAsync();

        var service = new ComprasService(ctx, new Mock<IEmailSender>().Object);
        var result = await service.ObterNotasPorEstadoAsync("Pendente");

        Assert.Single(result);
        Assert.Equal(150, result.First().ValorTotal);
    }

    #endregion


}

