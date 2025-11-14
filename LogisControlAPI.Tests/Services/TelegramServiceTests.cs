using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using Moq;
using Moq.Protected;
using LogisControlAPI.Services;
using LogisControlAPI.Auxiliar;

public class TelegramServiceTests
{

    /// <summary>
    /// Testa o envio de uma mensagem para o Telegram.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task EnviarMensagemAsync_DeveEnviarComSucesso()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        var httpClient = new HttpClient(handlerMock.Object);

        var config = new TelegramConfig
        {
            BotToken = "fake-token",
            ChatIds = new Dictionary<string, string>
            {
                { "Manutencao", "123456789" }
            }
        };

        var service = new TelegramService(httpClient, config);

        var exception = await Record.ExceptionAsync(() =>
            service.EnviarMensagemAsync("Mensagem de teste", "Manutencao"));

        Assert.Null(exception); // Espera não lançar exceção
    }


    /// <summary>
    /// Testa o envio de uma mensagem para o Telegram com um tipo inválido.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task EnviarMensagemAsync_DeveLancarExcecao_SeTipoNaoExistir()
    {
        // Arrange: HttpClient real (não será usado, pois falha antes da chamada HTTP)
        var httpClient = new HttpClient();

        var config = new TelegramConfig
        {
            BotToken = "fake-token",
            ChatIds = new Dictionary<string, string>() // dicionário vazio!
        };

        var service = new TelegramService(httpClient, config);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            service.EnviarMensagemAsync("Mensagem de teste", "TipoInvalido"));

        Assert.Contains("ChatId para o tipo", ex.Message);
    }


}
