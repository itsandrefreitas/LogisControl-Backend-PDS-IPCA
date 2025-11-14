using Xunit;
using LogisControlAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using LogisControlAPI.Auxiliar;

/// <summary>
/// Conjunto de testes unitários para o AuthService.
/// Valida a geração de tokens JWT com claims corretos, validade e robustez.
/// </summary>
public class AuthServiceTests
{
    /// <summary>
    /// Define a chave JWT usada durante os testes.
    /// Esta versão substitui a original para garantir um valor fixo e válido.
    /// </summary>
    public AuthServiceTests()
    {
        AuthSettings.PrivateKey = "CHAVE-DE-TESTE-SEGURA-1234567890123456";
    }

    /// <summary>
    /// Garante que o método GenerateToken devolve um token não vazio.
    /// </summary>
    [Fact]
    public void GenerateToken_DeveRetornarTokenNaoVazio()
    {
        var service = new AuthService();
        var token = service.GenerateToken(1, 1234, "Gestor");

        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    /// <summary>
    /// Valida que o token JWT contém os claims esperados (id, numFuncionario e role).
    /// </summary>
    [Fact]
    public void GenerateToken_DeveConterClaimsCorretos()
    {
        var service = new AuthService();
        var tokenString = service.GenerateToken(10, 1001, "Gestor");
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);
        var claims = token.Claims;

        Assert.Contains(claims, c => c.Value == "10");
        Assert.Contains(claims, c => c.Value == "1001");
        Assert.Contains(claims, c => c.Value == "Gestor");
    }

    /// <summary>
    /// Verifica se a validade do token é de 15 minutos.
    /// </summary>
    [Fact]
    public void GenerateToken_DeveExpirarEm15Minutos()
    {
        var service = new AuthService();
        var tokenString = service.GenerateToken(1, 1234, "User");
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        var diferenca = token.ValidTo - token.ValidFrom;
        Assert.Equal(15.0, diferenca.TotalMinutes, precision: 1);
    }

    /// <summary>
    /// Garante que o token gerado pode ser interpretado como JWT válido.
    /// </summary>
    [Fact]
    public void GenerateToken_DeveGerarJwtValido()
    {
        var service = new AuthService();
        var tokenString = service.GenerateToken(1, 1234, "Gestor");

        var handler = new JwtSecurityTokenHandler();
        var podeLer = handler.CanReadToken(tokenString);

        Assert.True(podeLer);
    }

    /// <summary>
    /// Garante que o claim "role" é incluído exatamente como foi recebido,
    /// independentemente do uso de maiúsculas/minúsculas.
    /// </summary>
    /// <param name="role">O valor da role a testar.</param>
    [Theory]
    [InlineData("Gestor")]
    [InlineData("gestor")]
    [InlineData("GESTOR")]
    public void GenerateToken_DeveIncluirRoleComoFoiRecebida(string role)
    {
        var service = new AuthService();
        var tokenString = service.GenerateToken(1, 1001, role);
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);

        Assert.Contains(token.Claims, c => c.Value == role);
    }

    /// <summary>
    /// Verifica se o token contém exatamente 3 partes separadas por ponto (JWT formatado).
    /// </summary>
    [Fact]
    public void GenerateToken_DeveTerFormatoJwtComTresPartes()
    {
        var service = new AuthService();
        var token = service.GenerateToken(1, 1234, "Gestor");

        var partes = token.Split('.');
        Assert.Equal(3, partes.Length);
    }

    /// <summary>
    /// Garante que o algoritmo usado na assinatura do token é HS256 (HMAC-SHA256).
    /// </summary>
    [Fact]
    public void GenerateToken_DeveUsarAlgoritmoHS256()
    {
        var service = new AuthService();
        var token = new JwtSecurityTokenHandler().ReadJwtToken(service.GenerateToken(1, 1234, "Gestor"));

        Assert.Equal("HS256", token.Header.Alg);
    }

    /// <summary>
    /// Valida que o token não define issuer nem audience, ou que correspondem aos esperados.
    /// </summary>
    [Fact]
    public void GenerateToken_DeveTerIssuerEAudienceNulos()
    {
        var service = new AuthService();
        var token = new JwtSecurityTokenHandler().ReadJwtToken(service.GenerateToken(1, 1234, "Gestor"));

        Assert.True(string.IsNullOrEmpty(token.Issuer));
        Assert.Empty(token.Audiences);
    }

}