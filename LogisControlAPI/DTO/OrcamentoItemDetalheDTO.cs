namespace LogisControlAPI.DTO
{
    public class OrcamentoItemDetalheDTO
    {
        public int OrcamentoItemID { get; set; }
        public int MateriaPrimaID { get; set; }
        public string MateriaPrimaNome { get; set; }
        public int Quantidade { get; set; }
        public double PrecoUnit { get; set; }
        public int PrazoEntrega { get; set; }
    }
}