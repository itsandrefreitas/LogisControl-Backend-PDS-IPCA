using Xunit;
using LogisControlAPI.Services;
using LogisControlAPI.Models;
using LogisControlAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

/// <summary>
/// Conjunto de testes unitários para o serviço UtilizadorService.
/// Valida a criação de hashes de passwords, verificação de credenciais e verificação de existência de utilizadores.
/// </summary>
public class UtilizadorServiceTests
{
    /// <summary>
    /// Cria um contexto de base de dados em memória (InMemory) para testes isolados.
    /// </summary>
    /// <returns>Instância de LogisControlContext com base de dados temporária.</returns>
    private LogisControlContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<LogisControlContext>()
            .UseInMemoryDatabase(databaseName: "TestDB_" + System.Guid.NewGuid())
            .Options;

        return new LogisControlContext(options);
    }

    /// <summary>
    /// Garante que a função HashPassword gera um hash diferente da senha original.
    /// </summary>
    [Fact]
    public void HashPassword_DeveGerarHashDiferenteDaSenhaOriginal()
    {
        var service = new UtilizadorService(null);
        var hash = service.HashPassword("123456");

        Assert.NotEqual("123456", hash);
        Assert.NotNull(hash);
    }

    /// <summary>
    /// Verifica que a password gerada é validada corretamente quando correta.
    /// </summary>
    [Fact]
    public void VerifyPassword_DeveRetornarTrue_SeSenhaCorreta()
    {
        var service = new UtilizadorService(null);
        var senha = "abc123";
        var hash = service.HashPassword(senha);

        var resultado = service.VerifyPassword(hash, senha);

        Assert.True(resultado);
    }

    /// <summary>
    /// Verifica que uma password incorreta não valida com o hash.
    /// </summary>
    [Fact]
    public void VerifyPassword_DeveRetornarFalse_SeSenhaIncorreta()
    {
        var service = new UtilizadorService(null);
        var hash = service.HashPassword("correta");

        var resultado = service.VerifyPassword(hash, "errada");

        Assert.False(resultado);
    }

    /// <summary>
    /// Garante que o método VerificarSeExisteNumeroFuncionario retorna true quando o número existe na base de dados.
    /// </summary>
    [Fact]
    public async Task VerificarSeExisteNumeroFuncionario_DeveRetornarTrue_SeNumeroExistir()
    {
        var context = GetInMemoryDbContext();

        context.Utilizadores.Add(new Utilizador
        {
            NumFuncionario = 1234,
            PrimeiroNome = "Teste",
            Sobrenome = "Teste",
            Password = "hashed-password",
            Role = "Gestor"
        });
        await context.SaveChangesAsync();

        var service = new UtilizadorService(context);

        var resultado = await service.VerificarSeExisteNumeroFuncionario(1234);

        Assert.True(resultado);
    }

    /// <summary>
    /// Garante que o método VerificarSeExisteNumeroFuncionario retorna false quando o número não existe.
    /// </summary>
    [Fact]
    public async Task VerificarSeExisteNumeroFuncionario_DeveRetornarFalse_SeNumeroNaoExistir()
    {
        var context = GetInMemoryDbContext();
        var service = new UtilizadorService(context);

        var resultado = await service.VerificarSeExisteNumeroFuncionario(9999);

        Assert.False(resultado);
    }

    /// <summary>
    /// Verifica que o hash gerado para a mesma password é diferente em chamadas separadas (devido ao salt).
    /// </summary>
    [Fact]
    public void HashPassword_DeveGerarHashDiferenteParaMesmaSenha()
    {
        var service = new UtilizadorService(null);
        var senha = "senha123";

        var hash1 = service.HashPassword(senha);
        var hash2 = service.HashPassword(senha);

        Assert.NotEqual(hash1, hash2);
    }

    /// <summary>
    /// Verifica que a verificação falha quando a password é nula ou vazia.
    /// </summary>
    [Fact]
    public void VerifyPassword_DeveRetornarFalse_SeSenhaForNulaOuVazia()
    {
        var service = new UtilizadorService(null);
        var hash = service.HashPassword("validapass");

        Assert.False(service.VerifyPassword(hash, null));
        Assert.False(service.VerifyPassword(hash, ""));
    }

    /// <summary>
    /// Password com caracteres especiais deve ser validada corretamente.
    /// </summary>
    [Fact]
    public void HashPassword_DeveAceitarCaracteresEspeciais()
    {
        var service = new UtilizadorService(null);
        var senhaEspecial = "P@$$w0rd!#€&/()=çÇ~^";

        var hash = service.HashPassword(senhaEspecial);

        Assert.NotNull(hash);
        Assert.True(service.VerifyPassword(hash, senhaEspecial)); // tem de validar corretamente
    }


    /// <summary>
    /// Verifica se o método HashPassword lança uma exceção quando a senha é nula ou vazia.
    /// </summary>
    /// <param name="senhaInvalida"></param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void HashPassword_DeveLancarExcecao_SeSenhaForNulaOuVazia(string senhaInvalida)
    {
        var service = new UtilizadorService(null);

        var excecao = Assert.Throws<ArgumentException>(() => service.HashPassword(senhaInvalida));
        Assert.Equal("A password não pode ser vazia ou nula.", excecao.Message);
    }

}
