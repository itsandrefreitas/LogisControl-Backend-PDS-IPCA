namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO usado para o administrador atualizar o estado e o perfil do utilizador.
    /// </summary>
    public class UtilizadorUpdateAdminDTO
    {
        public string Role { get; set; } = null!;
        public bool Estado { get; set; }
    }
}
