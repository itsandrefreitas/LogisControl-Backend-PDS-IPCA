using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogisControlAPI.Models
{
    /// <summary>
    /// Representa a nota de encomenda gerada após aprovação de um orçamento.
    /// </summary>
    public class NotaEncomenda
    {
        public int NotaEncomendaId { get; set; }
        public DateTime DataEmissao { get; set; }
        public string Estado { get; set; } = null!;
        public double ValorTotal { get; set; }

        public int OrcamentoId { get; set; }                  // ← Chave FK
        public virtual Orcamento Orcamento { get; set; } = null!;

        public virtual ICollection<NotaEncomendaItens> Itens { get; set; }
            = new List<NotaEncomendaItens>();
    }
}
