namespace LogisControlAPI.DTO
{
    public class RegistoManutencaoDTO
    {
        public string Descricao { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public int PedidoManutencaoPedidoManutId { get; set; }
        public int? AssistenciaExternaAssistenteId { get; set; }
    }
}
