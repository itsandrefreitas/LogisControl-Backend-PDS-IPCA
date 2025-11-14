using System.ComponentModel.DataAnnotations;

namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO utilizado para atualizar os dados de um fornecedor existente.
    /// </summary>
    public class AtualizarFornecedorDTO
    {
        [Required]
        public string Nome { get; set; }

        public int? Telefone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
    }
}
