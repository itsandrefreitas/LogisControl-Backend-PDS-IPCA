using Xunit;
using Microsoft.EntityFrameworkCore;
using LogisControlAPI.Data;
using LogisControlAPI.Services;
using LogisControlAPI.Interfaces;
using LogisControlAPI.Models;
using System.Threading.Tasks;
using System;
using Moq;

/// <summary>
/// Fake do NotificationService para simular envio de emails nos testes sem falhas.
/// </summary>
public class FakeNotificationService : NotificationService
{
    public FakeNotificationService() : base(null) { }

    public override async Task NotificarAsync(string destinatario, string assunto, string mensagem)
    {
        await Task.CompletedTask;
    }


    /// <summary>
    /// Teste para verificar se o método NotificarAsync chama o método EnviarAsync do IEmailSender com os parâmetros corretos.
    /// </summary>
    /// <returns></returns>
    [Fact]
public async Task NotificarAsync_DeveChamarEnviarAsyncComParametrosCorretos()
{
    // Arrange
    var mockEmailSender = new Mock<IEmailSender>();
    var service = new NotificationService(mockEmailSender.Object);

    var destinatario = "teste@exemplo.com";
    var assunto = "Assunto de Teste";
    var mensagem = "Mensagem de teste";

    // Act
    await service.NotificarAsync(destinatario, assunto, mensagem);

    // Assert
    mockEmailSender.Verify(
        s => s.EnviarAsync(destinatario, assunto, mensagem), 
        Times.Once
    );
}


    /// <summary>
    /// Teste para verificar se o método NotificarAsync propaga exceções quando o envio de email falha.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task NotificarAsync_DevePropagarExcecao_SeEmailFalhar()
    {
        // Arrange
        var emailMock = new Mock<IEmailSender>();
        emailMock.Setup(e => e.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                 .ThrowsAsync(new Exception("Falha ao enviar email"));

        var service = new NotificationService(emailMock.Object);

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => service.NotificarAsync("a@a.com", "Erro", "teste"));
    }



    /// <summary>
    /// Teste para verificar se o método NotificarAsync lança exceção quando o destinatário é inválido.
    /// </summary>
    /// <param name="destinatario"></param>
    /// <returns></returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task NotificarAsync_DeveLancarExcecao_SeDestinatarioInvalido(string destinatario)
    {
        var mock = new Mock<IEmailSender>();
        var service = new NotificationService(mock.Object);

        await Assert.ThrowsAsync<ArgumentException>(() => service.NotificarAsync(destinatario, "Assunto", "Mensagem"));
    }


    /// <summary>
    /// Teste para verificar se o método NotificarAsync lança exceção quando o assunto é inválido.
    /// </summary>
    /// <param name="assunto"></param>
    /// <returns></returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task NotificarAsync_DeveLancarExcecao_SeAssuntoInvalido(string assunto)
    {
        var mockSender = new Mock<IEmailSender>();
        var service = new NotificationService(mockSender.Object);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.NotificarAsync("user@example.com", assunto, "Mensagem"));

        Assert.Equal("O assunto é obrigatório.", ex.Message);
    }

    /// <summary>
    /// Teste para verificar se o método NotificarAsync lança exceção quando a mensagem é inválida.
    /// </summary>
    /// <param name="mensagem"></param>
    /// <returns></returns>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task NotificarAsync_DeveLancarExcecao_SeMensagemInvalida(string mensagem)
    {
        var mockSender = new Mock<IEmailSender>();
        var service = new NotificationService(mockSender.Object);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.NotificarAsync("user@example.com", "Assunto", mensagem));

        Assert.Equal("A mensagem é obrigatória.", ex.Message);
    }
}



