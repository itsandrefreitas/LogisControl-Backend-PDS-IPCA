namespace LogisControlAPI.DTO
{
    public class ProdMaterialListar
    {
            /// <summary>
            /// ID do registo na tabela ProdMateriais.
            /// </summary>
            public int ProdMateriaisId { get; set; }

            /// <summary>
            /// Nome da matéria-prima utilizada.
            /// </summary>
            public string MateriaPrimaNome { get; set; } = null!;

            /// <summary>
            /// Quantidade da matéria-prima utilizada.
            /// </summary>
            public int QuantidadeUtilizada { get; set; }
        }
    }