using Xunit;
using LogisControlAPI.Controllers;
using LogisControlAPI.Data;
using LogisControlAPI.Models;
using LogisControlAPI.Services;
using LogisControlAPI.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

/// <summary>
/// Testes unitários para o LoginController.
/// Valida os diferentes cenários de autenticação.
/// </summary>
public class LoginControllerTests
{
    /// <summary>
    /// Cria um contexto de base de dados em memória (InMemory) para testes isolados.
    /// </summary>
    /// <returns>Instância de LogisControlContext para utilização em testes.</returns>
    private LogisControlContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<LogisControlContext>()
            .UseInMemoryDatabase(databaseName: "LoginDb_" + System.Guid.NewGuid())
            .Options;
        return new LogisControlContext(options);
    }

    /// <summary>
    /// Verifica se o login retorna 200 OK e token válido quando as credenciais são corretas.
    /// </summary>
    [Fact]
    public async Task Login_DeveRetornarOk_SeCredenciaisForemValidas()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var utilizadorServiceAuxiliar = new UtilizadorService(null);

        var utilizador = new Utilizador
        {
            UtilizadorId = 1,
            PrimeiroNome = "João",
            Sobrenome = "Silva",
            Password = utilizadorServiceAuxiliar.HashPassword("password123"),
            Role = "Gestor",
            Estado = true,
            NumFuncionario = 1234
        };
        context.Utilizadores.Add(utilizador);
        await context.SaveChangesAsync();

        var utilizadorService = new UtilizadorService(context);
        var authService = new AuthService();
        var controller = new LoginController(context, utilizadorService, authService);

        var loginDto = new LoginDTO { NumFuncionario = 1234, Password = "password123" };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = okResult.Value;

        // Reflection para aceder ao objeto anónimo
        var token = response.GetType().GetProperty("Token").GetValue(response, null)?.ToString();
        var id = (int)response.GetType().GetProperty("Id").GetValue(response, null);
        var nome = response.GetType().GetProperty("Nome").GetValue(response, null)?.ToString();
        var role = response.GetType().GetProperty("Role").GetValue(response, null)?.ToString();
        var estado = (bool)response.GetType().GetProperty("Estado").GetValue(response, null);
        var numFuncionario = (int)response.GetType().GetProperty("NumFuncionario").GetValue(response, null);

        Assert.Equal(1, id);
        Assert.Equal("João", nome);
        Assert.Equal("Gestor", role);
        Assert.Equal(true, estado);
        Assert.Equal(1234, numFuncionario);
        Assert.NotNull(token);
    }

    /// <summary>
    /// Verifica se o login retorna 401 Unauthorized quando o número de funcionário não existe.
    /// </summary>
    [Fact]
    public async Task Login_DeveRetornarUnauthorized_SeNumeroFuncionarioNaoExistir()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var utilizadorService = new UtilizadorService(context);
        var authService = new AuthService();
        var controller = new LoginController(context, utilizadorService, authService);

        var loginDto = new LoginDTO { NumFuncionario = 9999, Password = "invalido" };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Número de funcionário ou senha inválidos.", unauthorized.Value);
    }

    /// <summary>
    /// Verifica se o login retorna 401 Unauthorized quando a password está incorreta.
    /// </summary>
    [Fact]
    public async Task Login_DeveRetornarUnauthorized_SePasswordIncorreta()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var utilizadorServiceAuxiliar = new UtilizadorService(null);

        var utilizador = new Utilizador
        {
            UtilizadorId = 2,
            PrimeiroNome = "Maria",
            Sobrenome = "Pereira",
            Password = utilizadorServiceAuxiliar.HashPassword("senhaCorreta"),
            Role = "Tecnico",
            Estado = true,
            NumFuncionario = 5678
        };
        context.Utilizadores.Add(utilizador);
        await context.SaveChangesAsync();

        var utilizadorService = new UtilizadorService(context);
        var authService = new AuthService();
        var controller = new LoginController(context, utilizadorService, authService);

        var loginDto = new LoginDTO { NumFuncionario = 5678, Password = "senhaErrada" };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Número de funcionário ou senha inválidos.", unauthorized.Value);
    }

    /// <summary>
    /// Verifica se o login retorna 500 Internal Server Error em caso de exceção inesperada.
    /// </summary>
    [Fact]
    public async Task Login_DeveRetornarErro500_SeExcecaoForLancada()
    {
        // Arrange
        var contextMock = new Moq.Mock<LogisControlContext>();
        var utilizadorService = new UtilizadorService(contextMock.Object);
        var authService = new AuthService();
        var controller = new LoginController(contextMock.Object, utilizadorService, authService);

        var loginDto = new LoginDTO { NumFuncionario = 1, Password = "erro" };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var errorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, errorResult.StatusCode);
    }
}
