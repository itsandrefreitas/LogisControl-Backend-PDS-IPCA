using System.ComponentModel.DataAnnotations;
namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO que representa um cliente, contendo apenas informações essenciais e não sensíveis.
    /// </summary>
    public class ClienteDTO
    {
        /// <summary>
        /// Identificador único do cliente.
        /// </summary>
        public int ClienteId { get; set; }

        /// <summary>
        /// Nome do cliente.
        /// </summary>
        public string Nome { get; set; } = null!;

        /// <summary>
        /// Número de Identificação Fiscal (NIF) do cliente.
        /// </summary>
        public int Nif { get; set; }

        /// <summary>
        /// Morada completa do cliente.
        /// </summary>
        public string Morada { get; set; } = null!;
    }
}