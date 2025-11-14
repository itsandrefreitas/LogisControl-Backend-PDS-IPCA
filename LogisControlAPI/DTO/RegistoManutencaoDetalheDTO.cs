namespace LogisControlAPI.DTO
{
    public class RegistoManutencaoDetalheDTO
    {
        public int RegistoManutencaoId { get; set; }

        public string Descricao { get; set; } = null!;

        public string Estado { get; set; } = null!;

        public int PedidoManutencaoPedidoManutId { get; set; }

        public int UtilizadorUtilizadorId { get; set; }

        public int? AssistenciaExternaAssistenteId { get; set; } // Nullable
    }
}