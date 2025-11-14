namespace LogisControlAPI.DTO
{
    /// <summary>
    /// Dados para criar um pedido de cotação:
    /// qual pedido de compra e a que fornecedor.
    /// </summary>
    public class CriarPedidoCotacaoDTO
    {
        public int PedidoCompraId { get; set; }
        public int FornecedorId { get; set; }
    }
}