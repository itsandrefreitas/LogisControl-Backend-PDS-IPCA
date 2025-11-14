using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Data;
using LogisControlAPI.Services;
using LogisControlAPI.Models;
using System.Threading.Tasks;

/// <summary>
/// Conjunto de testes unitários para o serviço StockService.
/// Valida a emissão de alertas de stock crítico de matérias-primas.
/// </summary>
public class StockServiceTests
{
    /// <summary>
    /// Cria um contexto de base de dados em memória (InMemory) para testes isolados.
    /// </summary>
    private LogisControlContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<LogisControlContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_" + System.Guid.NewGuid())
            .Options;
        return new LogisControlContext(options);
    }

    /// <summary>
    /// Garante que, se a matéria-prima não existir, nenhuma notificação é enviada.
    /// </summary>
    [Fact]
    public async Task VerificarStockCritico_NaoFazNada_SeMateriaPrimaNaoExistir()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var notificadorMock = new Mock<NotificationService>(null as object);
        var service = new StockService(context, notificadorMock.Object);

        // Act
        await service.VerificarStockCritico(999, 20); // ID inexistente, valor anterior arbitrário

        // Assert
        notificadorMock.Verify(
            n => n.NotificarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never()
        );
    }

    /// <summary>
    /// Garante que uma notificação é enviada se a quantidade da matéria-prima for inferior a 10 e diminuiu.
    /// </summary>
    [Fact]
    public async Task VerificarStockCritico_DeveEnviarNotificacao_SeQuantidadeInferiorA10()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.MateriasPrimas.Add(new MateriaPrima
        {
            MateriaPrimaId = 1,
            Nome = "Aço",
            Quantidade = 5,
            Categoria = "Metais",
            CodInterno = "AC001",
            Descricao = "Aço carbono para estruturas"
        });
        await context.SaveChangesAsync();

        var notificadorMock = new Mock<NotificationService>(null as object);
        var service = new StockService(context, notificadorMock.Object);

        // Act
        await service.VerificarStockCritico(1, 15); // Redução de 15 para 5

        // Assert
        notificadorMock.Verify(
            n => n.NotificarAsync(
                "nunofernandescastro@gmail.com",
                It.Is<string>(s => s.Contains("Stock Baixo")),
                It.Is<string>(m => m.Contains("Aço") && m.Contains("5"))
            ),
            Times.Once()
        );
    }

    /// <summary>
    /// Garante que nenhuma notificação é enviada se a quantidade for >= 10.
    /// </summary>
    [Fact]
    public async Task VerificarStockCritico_NaoEnviaNotificacao_SeQuantidadeIgualOuSuperiorA10()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.MateriasPrimas.Add(new MateriaPrima
        {
            MateriaPrimaId = 2,
            Nome = "Ferro",
            Quantidade = 12,
            Categoria = "Metais",
            CodInterno = "FE001",
            Descricao = "Ferro fundido para peças"
        });
        await context.SaveChangesAsync();

        var notificadorMock = new Mock<NotificationService>(null as object);
        var service = new StockService(context, notificadorMock.Object);

        // Act
        await service.VerificarStockCritico(2, 15); // Redução de 15 para 12, mas ainda acima do limiar

        // Assert
        notificadorMock.Verify(
            n => n.NotificarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never()
        );
    }

    /// <summary>
    /// Garante que nenhuma notificação é enviada se a quantidade atual for menor que 10, mas não diminuiu.
    /// </summary>
    [Fact]
    public async Task VerificarStockCritico_NaoEnviaNotificacao_SeNaoHouveReducao()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        context.MateriasPrimas.Add(new MateriaPrima
        {
            MateriaPrimaId = 3,
            Nome = "Cobre",
            Quantidade = 8,
            Categoria = "Metais",
            CodInterno = "CB001",
            Descricao = "Cobre eletrolítico"
        });
        await context.SaveChangesAsync();

        var notificadorMock = new Mock<NotificationService>(null as object);
        var service = new StockService(context, notificadorMock.Object);

        // Act
        await service.VerificarStockCritico(3, 5); // Aumentou de 5 para 8

        // Assert
        notificadorMock.Verify(
            n => n.NotificarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never()
        );
    }

    /// <summary>
    /// Garante que uma exceção é lançada se o ID da matéria-prima for inválido (0 ou negativo).
    /// </summary>
    /// <param name="idInvalido"></param>
    /// <returns></returns>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task VerificarStockCritico_DeveLancarExcecao_SeIdInvalido(int idInvalido)
    {
        var ctx = GetInMemoryDbContext();
        var notificador = new Mock<NotificationService>(null as object);
        var service = new StockService(ctx, notificador.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.VerificarStockCritico(idInvalido, 10));
    }

    /// <summary>
    /// Garante que uma notificação não é enviada se a quantidade atual for igual à anterior.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task VerificarStockCritico_NaoEnviaNotificacao_SeValorNaoAlterou()
    {
        var ctx = GetInMemoryDbContext();
        ctx.MateriasPrimas.Add(new MateriaPrima
        {
            MateriaPrimaId = 5,
            Nome = "Níquel",
            Quantidade = 9,
            Categoria = "Metais",
            CodInterno = "NI001",
            Descricao = "Níquel refinado"
        });
        await ctx.SaveChangesAsync();

        var notificacoes = new Mock<NotificationService>(null as object);
        var service = new StockService(ctx, notificacoes.Object);

        await service.VerificarStockCritico(5, 9); // Quantidade igual

        notificacoes.Verify(n => n.NotificarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }


    /// <summary>
    /// Garante que nenhuma notificação é enviada se a quantidade atual for >= 10 ou se não houve redução.
    /// </summary>
    /// <param name="materiaPrimaId"></param>
    /// <param name="quantidadeAnterior"></param>
    /// <returns></returns>
    [Theory]
    [InlineData(999, 20)] // matéria-prima inexistente
    [InlineData(1, 12)]   // stock >= 10
    [InlineData(1, 3)]    // stock < 10 mas não diminuiu
    public async Task VerificarStockCritico_NaoFazNada_SeCondicoesInvalidas(int materiaPrimaId, int quantidadeAnterior)
    {
        // Arrange
        var context = GetInMemoryDbContext();

        if (materiaPrimaId == 1)
        {
            context.MateriasPrimas.Add(new MateriaPrima
            {
                MateriaPrimaId = 1,
                Nome = "Teste",
                Quantidade = quantidadeAnterior >= 10 ? 12 : 5,
                Categoria = "Geral",
                CodInterno = "MAT001",
                Descricao = "Teste"
            });
            await context.SaveChangesAsync();
        }

        var notificadorMock = new Mock<NotificationService>(null as object);
        var service = new StockService(context, notificadorMock.Object);

        // Act
        await service.VerificarStockCritico(materiaPrimaId, quantidadeAnterior);

        // Assert
        notificadorMock.Verify(
            n => n.NotificarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never
        );
    }


}