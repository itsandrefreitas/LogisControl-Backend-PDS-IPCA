using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Data;
using LogisControlAPI.Models;
using LogisControlAPI.Services;

public class ProducaoServiceTests
{
    private LogisControlContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<LogisControlContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new LogisControlContext(options);
    }

    [Fact]
    public async Task AtualizarEstadoEObservacoesAsync_DeveConcluirOrdemEAtualizarProduto_SeEstadoProduzido()
    {
        var context = GetInMemoryDbContext();

        var ordem = new OrdemProducao { OrdemProdId = 1, Quantidade = 10, Estado = "Em Producao" };
        var produto = new Produto { ProdutoId = 1, Quantidade = 5, OrdemProducaoOrdemProdId = 1, CodInterno = "1", Descricao = "teste", Nome = "teste" };
        var registo = new RegistoProducao { RegistoProducaoId = 1, Estado = "", OrdemProducaoOrdemProdId = 1 };

        context.OrdensProducao.Add(ordem);
        context.Produtos.Add(produto);
        context.RegistosProducao.Add(registo);
        await context.SaveChangesAsync();

        var notificadorMock = new Mock<NotificationService>(null as object);
        var service = new ProducaoService(context, notificadorMock.Object);

        await service.AtualizarEstadoEObservacoesAsync(1, "Produzido", "Finalizado com sucesso");

        var ordemAtualizada = await context.OrdensProducao.FindAsync(1);
        var produtoAtualizado = await context.Produtos.FindAsync(1);

        Assert.Equal("Concluido", ordemAtualizada.Estado);
        Assert.Equal(15, produtoAtualizado.Quantidade);
        Assert.NotNull(ordemAtualizada.DataConclusao);

        notificadorMock.Verify(n => n.NotificarAsync(
            It.Is<string>(email => email.Contains("nunofernandescastro")),
            It.Is<string>(a => a.Contains("Produção Concluída")),
            It.Is<string>(m => m.Contains("#1"))
        ), Times.Once);
    }

    [Fact]
    public async Task AtualizarEstadoEObservacoesAsync_DeveCancelarOrdemENotificar_SeEstadoCancelado()
    {
        var context = GetInMemoryDbContext();

        var ordem = new OrdemProducao { OrdemProdId = 2, Quantidade = 5, Estado = "Em Producao" };
        var registo = new RegistoProducao { RegistoProducaoId = 2, Estado = "", OrdemProducaoOrdemProdId = 2 };

        context.OrdensProducao.Add(ordem);
        context.RegistosProducao.Add(registo);
        await context.SaveChangesAsync();

        var notificadorMock = new Mock<NotificationService>(null as object);
        var service = new ProducaoService(context, notificadorMock.Object);

        await service.AtualizarEstadoEObservacoesAsync(2, "Cancelado", "Problema de qualidade");

        var ordemAtualizada = await context.OrdensProducao.FindAsync(2);
        Assert.Equal("Cancelada", ordemAtualizada.Estado);

        notificadorMock.Verify(n => n.NotificarAsync(
            It.Is<string>(email => email.Contains("nunofernandescastro")),
            It.Is<string>(a => a.Contains("Produção Cancelada")),
            It.Is<string>(m => m.Contains("#2"))
        ), Times.Once);
    }

    [Fact]
    public async Task AtualizarEstadoEObservacoesAsync_DeveNotificar_SeEstadoParadoPorDefeito()
    {
        var context = GetInMemoryDbContext();

        var ordem = new OrdemProducao { OrdemProdId = 3, Quantidade = 8, Estado = "Em Producao" };
        var registo = new RegistoProducao { RegistoProducaoId = 3, Estado = "", OrdemProducaoOrdemProdId = 3 };

        context.OrdensProducao.Add(ordem);
        context.RegistosProducao.Add(registo);
        await context.SaveChangesAsync();

        var notificadorMock = new Mock<NotificationService>(null as object);
        var service = new ProducaoService(context, notificadorMock.Object);

        await service.AtualizarEstadoEObservacoesAsync(3, "Parado devido defeito", "Falha mecânica");

        notificadorMock.Verify(n => n.NotificarAsync(
            It.Is<string>(email => email.Contains("nunofernandescastro")),
            It.Is<string>(a => a.Contains("Produção Parada devido a Defeito")),
            It.Is<string>(m => m.Contains("#3"))
        ), Times.Once);
    }


    /// <summary>
    ///    Testa se o método AtualizarEstadoEObservacoesAsync lança uma exceção se o registo não existir
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AtualizarEstadoEObservacoesAsync_DeveLancarExcecao_SeRegistoNaoExistir()
    {
        var ctx = GetInMemoryDbContext();
        var service = new ProducaoService(ctx, new Mock<NotificationService>(null as object).Object);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.AtualizarEstadoEObservacoesAsync(999, "Produzido", "Sem registo"));

        Assert.Equal("Registo de produção não encontrado.", ex.Message);
    }

    /// <summary>
    ///     Testa se o método AtualizarEstadoEObservacoesAsync atualiza apenas as observações
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AtualizarEstadoEObservacoesAsync_DeveAtualizarApenasObservacoes_SeEstadoNulo()
    {
        var ctx = GetInMemoryDbContext();
        ctx.RegistosProducao.Add(new RegistoProducao { RegistoProducaoId = 4, Estado = "Inicial", Observacoes = "", OrdemProducaoOrdemProdId = 1 });
        ctx.OrdensProducao.Add(new OrdemProducao { OrdemProdId = 1, Estado = "Em Producao", Quantidade = 0 });
        await ctx.SaveChangesAsync();

        var service = new ProducaoService(ctx, new Mock<NotificationService>(null as object).Object);

        await service.AtualizarEstadoEObservacoesAsync(4, null, "Só observações");

        var registo = await ctx.RegistosProducao.FindAsync(4);
        Assert.Equal("Inicial", registo.Estado);
        Assert.Equal("Só observações", registo.Observacoes);
    }


    /// <summary>
    ///   Testa se o método AtualizarEstadoEObservacoesAsync atualiza apenas o estado
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AtualizarEstadoEObservacoesAsync_DeveAtualizarApenasEstado_SeObservacoesNulas()
    {
        var ctx = GetInMemoryDbContext();
        ctx.RegistosProducao.Add(new RegistoProducao { RegistoProducaoId = 5, Estado = "Inicial", Observacoes = "Antigo", OrdemProducaoOrdemProdId = 1 });
        ctx.OrdensProducao.Add(new OrdemProducao { OrdemProdId = 1, Estado = "Em Producao", Quantidade = 0 });
        await ctx.SaveChangesAsync();

        var service = new ProducaoService(ctx, new Mock<NotificationService>(null as object).Object);

        await service.AtualizarEstadoEObservacoesAsync(5, "Parado", null);

        var registo = await ctx.RegistosProducao.FindAsync(5);
        Assert.Equal("Parado", registo.Estado);
        Assert.Equal("Antigo", registo.Observacoes);
    }

    [Fact]
    public async Task AtualizarEstadoEObservacoesAsync_DeveConcluirOrdemMesmoSemProduto()
    {
        var ctx = GetInMemoryDbContext();
        ctx.OrdensProducao.Add(new OrdemProducao { OrdemProdId = 6, Quantidade = 2, Estado = "Em Producao" });
        ctx.RegistosProducao.Add(new RegistoProducao { RegistoProducaoId = 6, Estado = "", OrdemProducaoOrdemProdId = 6 });
        await ctx.SaveChangesAsync();

        var mock = new Mock<NotificationService>(null as object);
        var service = new ProducaoService(ctx, mock.Object);

        await service.AtualizarEstadoEObservacoesAsync(6, "Produzido", "Sem produto");

        var ordem = await ctx.OrdensProducao.FindAsync(6);
        Assert.Equal("Concluido", ordem.Estado);

        mock.Verify(n => n.NotificarAsync(
            It.IsAny<string>(),
            It.Is<string>(a => a.Contains("Concluída")),
            It.IsAny<string>()
        ), Times.Once);
    }

    [Fact]
    public async Task AtualizarEstadoEObservacoesAsync_DeveLancarExcecao_SeOrdemProducaoNaoExistir()
    {
        // Arrange
        var ctx = GetInMemoryDbContext();

        // Registo aponta para uma ordem que não existe na BD
        ctx.RegistosProducao.Add(new RegistoProducao
        {
            RegistoProducaoId = 7,
            Estado = "Produzido",
            OrdemProducaoOrdemProdId = 999 // inexistente
        });

        await ctx.SaveChangesAsync();

        var service = new ProducaoService(ctx, new Mock<NotificationService>(null as object).Object);

        // Act & Assert
        var ex = await Record.ExceptionAsync(() =>
            service.AtualizarEstadoEObservacoesAsync(7, "Produzido", "Teste com ordem inexistente"));

        Assert.Null(ex);
    }


}

