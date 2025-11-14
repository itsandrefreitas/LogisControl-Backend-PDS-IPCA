using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models
{
    
    public partial class Cliente
    {
        
        public int ClienteId { get; set; }

       
        public string Nome { get; set; } = null!;

        
        public int Nif { get; set; }

        
        public string Morada { get; set; } = null!;

        
        public virtual ICollection<EncomendaCliente> EncomendasCliente { get; set; } = new List<EncomendaCliente>();
    }
}