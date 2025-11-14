using System.ComponentModel.DataAnnotations;

namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO utilizado para criar um novo utilizador, contendo apenas os dados necessários para o registo.
    /// </summary>
    public class CriarUtilizadorDTO
    {
        /// <summary>
        /// Primeiro nome do utilizador.
        /// </summary>
        [Required]
        public string PrimeiroNome { get; set; }

        /// <summary>
        /// Sobrenome ou apelido do utilizador.
        /// </summary>
        [Required]
        public string Sobrenome { get; set; }

        /// <summary>
        /// Número único do funcionário (identificador interno).
        /// </summary>
        [Required]
        public int NumFuncionario { get; set; }

        /// <summary>
        /// Palavra-passe do utilizador. Deve ter entre 3 e 8 caracteres.
        /// </summary>
        [Required]
        [StringLength(8, MinimumLength = 3, ErrorMessage = "A Password deve ter entre 3 e 8 caracteres.")]
        public string Password { get; set; }

        /// <summary>
        /// Função atribuída ao utilizador (ex: Administrador, Operador, etc.).
        /// </summary>
        [Required]
        public string Role { get; set; }
    }
}
