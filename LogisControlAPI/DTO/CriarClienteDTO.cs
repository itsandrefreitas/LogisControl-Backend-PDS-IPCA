using System.ComponentModel.DataAnnotations;

namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO utilizado para criar um novo cliente.
    /// </summary>
    public class CriarClienteDTO
    {
        /// <summary>
        /// Nome do cliente.
        /// </summary>
        [Required]
        public string Nome { get; set; }

        /// <summary>
        /// Número de Identificação Fiscal do cliente.
        /// </summary>
        [Required]
        public int Nif { get; set; }

        /// <summary>
        /// Morada do cliente.
        /// </summary>
        [Required]
        public string Morada { get; set; }
    }
}