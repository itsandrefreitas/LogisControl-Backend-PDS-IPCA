namespace LogisControlAPI.DTO
{
    public class CriarPedidoManutencaoDTO
    {
        public string Descricao { get; set; }
        public int MaquinaMaquinaId { get; set; }
        public DateTime DataAbertura { get; set; }
        public string Estado { get; set; }
        public DateTime? DataConclusao { get; set; }
    }

}
