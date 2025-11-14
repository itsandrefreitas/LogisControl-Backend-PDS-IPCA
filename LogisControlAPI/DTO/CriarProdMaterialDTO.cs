namespace LogisControlAPI.DTO
{

    /// <summary>
    /// DTO utilizado para criar uma associação entre matéria-prima e ordem de produção.
    /// </summary>
    public class CriarProdMaterialDTO
    {
        public int OrdemProducaoId { get; set; }
        public int MateriaPrimaId { get; set; }
        public int QuantidadeUtilizada { get; set; }
    }
}
