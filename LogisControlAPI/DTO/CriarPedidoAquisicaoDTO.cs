using System.ComponentModel.DataAnnotations;

namespace LogisControlAPI.DTO
{
    public class CriarPedidoAquisicaoDTO
    {

        [Required]
        public string Descricao { get; set; }

        [Required]
        public int UtilizadorId { get; set; }
    }
}