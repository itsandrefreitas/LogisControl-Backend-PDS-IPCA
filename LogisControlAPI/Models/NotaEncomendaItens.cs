using System.ComponentModel.DataAnnotations.Schema;

namespace LogisControlAPI.Models
{
    public class NotaEncomendaItens
    {
        public int NotaEncomendaItensId { get; set; }

        public int NotaEncomendaId { get; set; }
        public virtual NotaEncomenda NotaEncomenda { get; set; } = null!;

        [Column("MateriaPrimaMateriaPrimaID")]
        public int MateriaPrimaId { get; set; }
        public virtual MateriaPrima MateriaPrima { get; set; } = null!;

        public int Quantidade { get; set; }
        public double PrecoUnit { get; set; }
    }
}