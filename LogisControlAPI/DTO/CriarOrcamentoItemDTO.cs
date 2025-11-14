namespace LogisControlAPI.DTO
{
    public class CriarOrcamentoItemDTO
    {
        public int MateriaPrimaID { get; set; }
        public int Quantidade { get; set; }
        public double PrecoUnit { get; set; }
        public int? PrazoEntrega { get; set; }
    }
}