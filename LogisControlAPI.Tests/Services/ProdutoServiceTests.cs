using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Data;
using LogisControlAPI.Models;
using LogisControlAPI.DTO;
using LogisControlAPI.Services;
using LogisControlAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;

public class ProdutoServiceTests
{
    private LogisControlContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<LogisControlContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        return new LogisControlContext(options);
    }


    /// <summary>
    /// Testa o método CriarProdutoAsync da classe ProdutoService.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task CriarProdutoAsync_DeveCriarProdutoComMateriasPrimas()
    {
        var context = GetInMemoryDbContext();
        var stockMock = new Mock<IStockService>();
        var service = new ProdutoService(context, stockMock.Object);

        var dto = new CriarProdutoDTO
        {
            Nome = "Produto A",
            Quantidade = 10,
            Descricao = "Desc",
            CodInterno = "PA001",
            Preco = 50.5,
            MateriasPrimas = new List<MateriaPrimaProdutoCriacaoDTO>
            {
                new MateriaPrimaProdutoCriacaoDTO { MateriaPrimaId = 1, QuantidadeNec = 2 },
                new MateriaPrimaProdutoCriacaoDTO { MateriaPrimaId = 2, QuantidadeNec = 3 }
            }
        };

        context.MateriasPrimas.AddRange(
            new MateriaPrima { MateriaPrimaId = 1, Nome = "MP1", Quantidade = 100, Categoria = "Cat", CodInterno = "MP001", Descricao = "Desc" },
            new MateriaPrima { MateriaPrimaId = 2, Nome = "MP2", Quantidade = 100, Categoria = "Cat", CodInterno = "MP002", Descricao = "Desc" }
        );
        await context.SaveChangesAsync();

        await service.CriarProdutoAsync(dto);

        var produto = await context.Produtos.Include(p => p.MateriaPrimaProdutos).FirstOrDefaultAsync();
        Assert.NotNull(produto);
        Assert.Equal("Produto A", produto.Nome);
        Assert.Equal(2, produto.MateriaPrimaProdutos.Count);
    }

    /// <summary>
    /// Testa o método CriarProdutoAsync da classe ProdutoService.
    /// </summary>
    /// <param name="nome"></param>
    /// <param name="quantidade"></param>
    /// <param name="codInterno"></param>
    /// <param name="preco"></param>
    /// <param name="mensagemEsperada"></param>
    /// <returns></returns>
    [Theory]
    [InlineData(null, 10, "Código", 50.0, "Nome é obrigatório.")]
    [InlineData("", 10, "Código", 50.0, "Nome é obrigatório.")]
    [InlineData("Produto", -5, "Código", 50.0, "Quantidade do produto não pode ser negativa.")]
    [InlineData("Produto", 10, "", 50.0, "Código interno é obrigatório.")]
    [InlineData("Produto", 10, "Código", -10.0, "Preço não pode ser negativo.")]
    public async Task CriarProdutoAsync_DeveLancarExcecao_SeDadosInvalidos(
    string nome, int quantidade, string codInterno, double preco, string mensagemEsperada)
    {
        var context = GetInMemoryDbContext();
        var stockMock = new Mock<IStockService>();
        var service = new ProdutoService(context, stockMock.Object);

        var dto = new CriarProdutoDTO
        {
            Nome = nome,
            Quantidade = quantidade,
            CodInterno = codInterno,
            Preco = preco,
            Descricao = "Desc",
            MateriasPrimas = new List<MateriaPrimaProdutoCriacaoDTO>()
        };

        var ex = await Assert.ThrowsAsync<Exception>(() => service.CriarProdutoAsync(dto));
        Assert.Equal(mensagemEsperada, ex.Message);
    }





    /// <summary>
    /// Testa o método CriarProdutoAsync da classe ProdutoService.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AtualizarProdutoAsync_DeveAtualizarDadosEVerificarStock()
    {
        var context = GetInMemoryDbContext();
        var stockMock = new Mock<IStockService>();
        stockMock.Setup(s => s.VerificarStockCriticoProduto(It.IsAny<int>(), It.IsAny<int>()))
                 .Returns(Task.CompletedTask);

        var produto = new Produto { ProdutoId = 1, Nome = "Old", Quantidade = 5, Descricao = "Old", CodInterno = "OLD001", Preco = 10 };
        context.Produtos.Add(produto);
        context.MateriaPrimaProdutos.Add(new MateriaPrimaProduto { ProdutoProdutoId = 1, MateriaPrimaMateriaPrimaId = 1, QuantidadeNec = 1 });
        await context.SaveChangesAsync();

        var service = new ProdutoService(context, stockMock.Object);

        var dto = new CriarProdutoDTO
        {
            Nome = "Novo",
            Quantidade = 20,
            Descricao = "Atualizado",
            CodInterno = "NEW001",
            Preco = 99.9,
            MateriasPrimas = new List<MateriaPrimaProdutoCriacaoDTO>
            {
                new MateriaPrimaProdutoCriacaoDTO { MateriaPrimaId = 2, QuantidadeNec = 4 }
            }
        };

        context.MateriasPrimas.Add(new MateriaPrima { MateriaPrimaId = 2, Nome = "MP2", Quantidade = 100, Categoria = "Cat", CodInterno = "MP002", Descricao = "Desc" });
        await context.SaveChangesAsync();

        await service.AtualizarProdutoAsync(1, dto);

        var produtoAtualizado = await context.Produtos.Include(p => p.MateriaPrimaProdutos).FirstAsync();
        Assert.Equal("Novo", produtoAtualizado.Nome);
        Assert.Equal(20, produtoAtualizado.Quantidade);
        Assert.Single(produtoAtualizado.MateriaPrimaProdutos);

        stockMock.Verify(s => s.VerificarStockCriticoProduto(1, 5), Times.Once);
    }


    /// <summary>
    /// Testa o método AtualizarEstadoEObservacoesAsync da classe ProducaoService.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AtualizarEstadoEObservacoesAsync_NaoDeveFazerNada_SeEstadoForDesconhecido()
    {
        // Arrange
        var ctx = GetInMemoryDbContext();

        var ordem = new OrdemProducao { OrdemProdId = 1, Quantidade = 10, Estado = "Em Producao" };
        var registo = new RegistoProducao { RegistoProducaoId = 1, OrdemProducaoOrdemProdId = 1, Estado = "Inicial" };

        ctx.OrdensProducao.Add(ordem);
        ctx.RegistosProducao.Add(registo);
        await ctx.SaveChangesAsync();

        var notificadorMock = new Mock<NotificationService>(null as object);
        var service = new ProducaoService(ctx, notificadorMock.Object);

        // Act
        await service.AtualizarEstadoEObservacoesAsync(1, "Em Pausa", "Comentário irrelevante");

        // Assert
        var ordemFinal = await ctx.OrdensProducao.FindAsync(1);
        var registoFinal = await ctx.RegistosProducao.FindAsync(1);

        Assert.Equal("Em Pausa", registoFinal.Estado);  // o estado do registo é atualizado
        Assert.Equal("Em Producao", ordemFinal.Estado); // a ordem não deve ser alterada
        Assert.Null(ordemFinal.DataConclusao);          // não foi concluída

        notificadorMock.Verify(n => n.NotificarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Testa o método AtualizarEstadoEObservacoesAsync da classe ProducaoService.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AtualizarEstadoEObservacoesAsync_DeveLancarExcecao_SeRegistoNaoExistir()
    {
        // Arrange
        var ctx = GetInMemoryDbContext();
        var notificadorMock = new Mock<NotificationService>(null as object);
        var service = new ProducaoService(ctx, notificadorMock.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.AtualizarEstadoEObservacoesAsync(999, "Produzido", "Tentativa inválida"));

        Assert.Equal("Registo de produção não encontrado.", ex.Message);
    }


    /// <summary>
    /// Testa o método AtualizarEstadoEObservacoesAsync da classe ProducaoService.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task AtualizarEstadoEObservacoesAsync_DeveConcluirOrdemMesmoSemProduto()
    {
        // Arrange
        var ctx = GetInMemoryDbContext();

        var ordem = new OrdemProducao
        {
            OrdemProdId = 1,
            Estado = "Em Producao",
            Quantidade = 10
        };

        var registo = new RegistoProducao
        {
            RegistoProducaoId = 1,
            Estado = "Em Curso",
            OrdemProducaoOrdemProdId = 1
        };

        ctx.OrdensProducao.Add(ordem);
        ctx.RegistosProducao.Add(registo);
        await ctx.SaveChangesAsync();

        var notificadorMock = new Mock<NotificationService>(null as object);
        var service = new ProducaoService(ctx, notificadorMock.Object);

        // Act
        await service.AtualizarEstadoEObservacoesAsync(1, "Produzido", "Sem produto");

        // Assert
        var ordemAtualizada = await ctx.OrdensProducao.FindAsync(1);

        Assert.Equal("Concluido", ordemAtualizada.Estado);
        Assert.NotNull(ordemAtualizada.DataConclusao);

        notificadorMock.Verify(n => n.NotificarAsync(
            It.IsAny<string>(),
            It.Is<string>(s => s.Contains("Produção Concluída")),
            It.Is<string>(s => s.Contains("#1"))), Times.Once);
    }

}
