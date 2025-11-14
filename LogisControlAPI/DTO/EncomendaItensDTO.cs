namespace LogisControlAPI.DTO
{
    public class EncomendaItensDTO
    {
        public int EncomendaItensId { get; set; }

        public int? Quantidade { get; set; }

        public int ProdutoId { get; set; } 

        public int EncomendaClienteEncomendaClienteId { get; set; }
    }
}
