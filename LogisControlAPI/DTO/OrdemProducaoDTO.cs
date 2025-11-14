namespace LogisControlAPI.DTO
{
    public partial class OrdemProducaoDTO
    {
        public int OrdemProdId { get; set; }

        public string Estado { get; set; } = null!;

        public int Quantidade { get; set; }

        public DateTime DataAbertura { get; set; }

        public DateTime? DataConclusao { get; set; }

        public int MaquinaMaquinaId { get; set; }

        public int EncomendaClienteEncomendaClienteId { get; set; }
    }
}
