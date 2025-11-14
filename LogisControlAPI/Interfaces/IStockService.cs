using System.Threading.Tasks;

namespace LogisControlAPI.Interfaces
{
    public interface IStockService
    {
        Task VerificarStockCritico(int materiaPrimaId, int quantidadeAnterior);
        Task VerificarStockCriticoProduto(int produtoId, int quantidadeAnterior);
    }
}
