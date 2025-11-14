namespace LogisControlAPI.DTO
{
    public class RegistoManutencaoVisualDTO
    {
        public int RegistoManutencaoId { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}