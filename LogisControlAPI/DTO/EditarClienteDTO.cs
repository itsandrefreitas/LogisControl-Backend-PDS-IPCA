using System.ComponentModel.DataAnnotations;

namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO para atualização de dados de um cliente.
    /// </summary>
    public class AtualizarClienteDTO
    {
        [Required]
        public string Nome { get; set; }

        [Required]
        public int Nif { get; set; }

        [Required]
        public string Morada { get; set; }
    }
}
