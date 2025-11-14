namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO para representar um item de orçamento.
    /// </summary>
    public class OrcamentoItemDTO
    {

        public int OrcamentoItemID { get; set; }
        public int OrcamentoID { get; set; }
        public int Quantidade { get; set; }
        public double PrecoUnit { get; set; }
        public int? PrazoEntrega { get; set; }
        public int MateriaPrimaID { get; set; }
        public string MateriaPrimaNome { get; set; } = null!;

    }
}