using System.ComponentModel.DataAnnotations;

namespace LogisControlAPI.DTO
{
    public class LoginDTO
    {

        [Required]
        public int NumFuncionario { get; set; }  // Número do funcionário para login

        [Required]
        public string Password { get; set; }  // Senha digitada pelo utilizador

    }
}
