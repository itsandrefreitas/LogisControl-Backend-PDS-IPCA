using Xunit;
using LogisControlAPI.Controllers;
using LogisControlAPI.Data;
using LogisControlAPI.Models;
using LogisControlAPI.Services;
using LogisControlAPI.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Testes unitários para o PedidoAquisicaoController.
/// Valida os principais fluxos de pedidos de aquisição.
/// </summary>
public class PedidoAquisicaoControllerTests
{
    /// <summary>
    /// Cria um contexto de base de dados em memória (InMemory) para testes isolados.
    /// </summary>
    private LogisControlContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<LogisControlContext>()
            .UseInMemoryDatabase(databaseName: "PedidoAquisicaoDb_" + System.Guid.NewGuid())
            .Options;
        return new LogisControlContext(options);
    }

    /// <summary>
    /// Testa a criação de um pedido de aquisição válido.
    /// </summary>
    [Fact]
    public async Task CriarPedidoAquisicao_DeveCriarPedido_ComSucesso()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var utilizador = new Utilizador
        {
            UtilizadorId = 1,
            PrimeiroNome = "Utilizador",
            Sobrenome = "Teste",
            Password = "fake-hash",
            Role = "Tecnico",
            Estado = true,
            NumFuncionario = 1234
        };
        context.Utilizadores.Add(utilizador);
        await context.SaveChangesAsync();

        var utilizadorService = new UtilizadorService(context);
        var controller = new PedidoAquisicaoController(context, utilizadorService);

        // Simular utilizador autenticado
        controller.ControllerContext = ControllerHelpers.CreateFakeContextWithUser(utilizador.UtilizadorId);

        var dto = new CriarPedidoAquisicaoDTO { Descricao = "Novo pedido de aquisição" };

        // Act
        var result = await controller.CriarPedidoAquisicao(dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    /// <summary>
    /// Testa a atualização do estado de um pedido existente.
    /// </summary>
    [Fact]
    public async Task AtualizarEstadoPedidoAquisicao_DeveAtualizarEstado()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var pedido = new PedidoCompra { PedidoCompraId = 1, Descricao = "Pedido original", Estado = "Aberto", DataAbertura = DateTime.UtcNow, UtilizadorUtilizadorId = 1 };
        context.PedidosCompra.Add(pedido);
        await context.SaveChangesAsync();

        var utilizadorService = new UtilizadorService(context);
        var controller = new PedidoAquisicaoController(context, utilizadorService);

        var dto = new AtualizarEstadoPedidoDTO { Estado = "Aceite" };

        // Act
        var result = await controller.AtualizarEstadoPedidoAquisicao(1, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Estado do pedido atualizado com sucesso.", okResult.Value);
    }

    /// <summary>
    /// Testa a atualização da descrição de um pedido existente.
    /// </summary>
    [Fact]
    public async Task AtualizarDescricaoAquisicao_DeveAtualizarDescricao()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var pedido = new PedidoCompra { PedidoCompraId = 1, Descricao = "Antiga", Estado = "Aberto", DataAbertura = DateTime.UtcNow, UtilizadorUtilizadorId = 1 };
        context.PedidosCompra.Add(pedido);
        await context.SaveChangesAsync();

        var utilizadorService = new UtilizadorService(context);
        var controller = new PedidoAquisicaoController(context, utilizadorService);

        var dto = new AtualizarDescricaoPedidoDTO { NovaDescricao = "Nova descrição" };

        // Act
        var result = await controller.AtualizarDescricaoAquisicao(1, dto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Descrição atualizada com sucesso.", okResult.Value);
    }

    /// <summary>
    /// Testa a listagem de pedidos de um utilizador autenticado.
    /// </summary>
    [Fact]
    public async Task ListarPedidosAquisicaoPorUtilizador_DeveRetornarPedidos()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var utilizador = new Utilizador { UtilizadorId = 1, PrimeiroNome = "Utilizador", Sobrenome = "Teste", Password = "senhaFake123", Role = "Tecnico", Estado = true, NumFuncionario = 1234 };
        context.Utilizadores.Add(utilizador);
        context.PedidosCompra.Add(new PedidoCompra { Descricao = "Pedido 1", Estado = "Aberto", UtilizadorUtilizadorId = 1, DataAbertura = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var utilizadorService = new UtilizadorService(context);
        var controller = new PedidoAquisicaoController(context, utilizadorService);

        controller.ControllerContext = ControllerHelpers.CreateFakeContextWithUser(utilizador.UtilizadorId);

        // Act
        var result = await controller.ListarPedidosAquisicaoPorUtilizador();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pedidos = Assert.IsAssignableFrom<IEnumerable<PedidoCompraDTO>>(okResult.Value);
        Assert.Single(pedidos);
    }

    /// <summary>
    /// Testa a listagem de pedidos filtrados por Role ("Tecnico").
    /// </summary>
    [Fact]
    public async Task ListarPedidosAquisicaoPorRole_DeveRetornarPedidosDeTecnicos()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var utilizador = new Utilizador { UtilizadorId = 1, PrimeiroNome = "Tecnico", Sobrenome = "Teste", Role = "Tecnico", Estado = true, NumFuncionario = 4321, Password = "senhaFake123" };
        context.Utilizadores.Add(utilizador);
        context.PedidosCompra.Add(new PedidoCompra { Descricao = "Pedido Tecnico", Estado = "Aberto", UtilizadorUtilizadorId = 1, DataAbertura = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var utilizadorService = new UtilizadorService(context);
        var controller = new PedidoAquisicaoController(context, utilizadorService);

        // Act
        var result = await controller.ListarPedidosAquisicaoPorRole();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pedidos = Assert.IsAssignableFrom<IEnumerable<PedidoCompraDTO>>(okResult.Value);
        Assert.Single(pedidos);
    }
}
