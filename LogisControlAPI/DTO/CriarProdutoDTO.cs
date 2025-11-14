namespace LogisControlAPI.DTO
{
    public class CriarProdutoDTO
    {
        public int? ProdutoId { get; set; }  // Opcional, só necessário na edição

        public string Nome { get; set; } =string.Empty;
        public int Quantidade { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string CodInterno { get; set; } = string.Empty;
        public double Preco { get; set; }

        public List<MateriaPrimaProdutoCriacaoDTO> MateriasPrimas { get; set; } = new();

    }
    public class MateriaPrimaProdutoCriacaoDTO
    {
        public int MateriaPrimaId { get; set; }
        public int QuantidadeNec { get; set; }
    }


}
