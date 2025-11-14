namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO utilizado para apresentar os dados de matérias-primas usadas numa ordem de produção.
    /// </summary>
    public class ProdMaterialDTO
    {
        public int QuantidadeUtilizada { get; set; }
        public int OrdemProducaoOrdemProdId { get; set; }
        public int MateriaPrimaMateriaPrimaId { get; set; }

    }
}
