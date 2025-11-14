using System.ComponentModel.DataAnnotations;


    namespace LogisControlAPI.DTO
    {
        /// <summary>
        /// DTO utilizado para criar um novo fornecedor.
        /// </summary>
        public class CriarFornecedorDTO
        {
            [Required]
            public string Nome { get; set; }

            public int? Telefone { get; set; }

            [EmailAddress]
            public string? Email { get; set; }
        }
    }