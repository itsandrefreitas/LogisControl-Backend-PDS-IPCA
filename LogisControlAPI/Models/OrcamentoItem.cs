using System.ComponentModel.DataAnnotations.Schema;

namespace LogisControlAPI.Models
{
    public partial class OrcamentoItem
    {
        public int OrcamentoItemID { get; set; }

        // Quantidade orçada
        public int Quantidade { get; set; }

        // Preço unitário acordado
        public double PrecoUnit { get; set; }

        // Prazo de entrega (dias) — conforme ERD está NULLABLE
        public int? PrazoEntrega { get; set; }

        // FK para Orcamento
        public int OrcamentoOrcamentoID { get; set; }
        [ForeignKey(nameof(OrcamentoOrcamentoID))]
        public virtual Orcamento Orcamento { get; set; } = null!;

        // FK para Matéria-Prima
        public int MateriaPrimaID { get; set; }

        [ForeignKey(nameof(MateriaPrimaID))]
        public virtual MateriaPrima MateriaPrima { get; set; } = null!;

    }
}