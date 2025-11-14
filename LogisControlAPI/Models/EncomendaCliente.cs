using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models
{
    
    public partial class EncomendaCliente
    {
        
        public int EncomendaClienteId { get; set; }

        
        public DateTime DataEncomenda { get; set; }

        
        public string Estado { get; set; } = null!;

        
        public int ClienteClienteId { get; set; }

        
        public virtual Cliente ClienteCliente { get; set; } = null!;
       

        public virtual ICollection<EncomendaItens> EncomendasItem { get; set; } = new List<EncomendaItens>();

        
        public virtual ICollection<OrdemProducao> OrdensProducao { get; set; } = new List<OrdemProducao>();
    }
}