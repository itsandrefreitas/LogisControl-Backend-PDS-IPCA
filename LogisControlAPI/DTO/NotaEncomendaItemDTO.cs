namespace LogisControlAPI.DTO
{
    public class NotaEncomendaItemDTO
    {
        public int MateriaPrimaId { get; set; }
        public string MateriaPrimaNome { get; set; }
        public int Quantidade { get; set; }
        public double PrecoUnit { get; set; }
    }
}