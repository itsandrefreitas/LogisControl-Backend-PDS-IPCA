using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LogisControlAPI.DTO
{
    public class CriarPedidoCompraDTO
    {
        [Required]
        public string Descricao { get; set; } = null!;

        [Required]
        public int UtilizadorId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Deve indicar pelo menos um item no pedido.")]
        public List<ItemPedidoDTO> Itens { get; set; } = new();
    }
    public class ItemPedidoDTO
    {
        [Required]
        public int MateriaPrimaId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantidade deve ser pelo menos 1.")]
        public int Quantidade { get; set; }
    }
}