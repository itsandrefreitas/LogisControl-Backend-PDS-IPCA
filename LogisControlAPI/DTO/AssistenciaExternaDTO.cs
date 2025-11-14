namespace LogisControlAPI.DTO
{
    public class AssistenciaExternaDTO
    {
        public int AssistenteId { get; set; }

        public string Nome { get; set; } = null!;

        public int Nif { get; set; }

        public string Morada { get; set; } = null!;

        public int Telefone { get; set; }
    }
}
