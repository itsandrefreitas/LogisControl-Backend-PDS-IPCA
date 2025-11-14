using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models
{
    
    public partial class AssistenciaExterna
    {
        
        public int AssistenteId { get; set; }

       
        public string Nome { get; set; } = null!;

        
        public int Nif { get; set; }

        
        public string Morada { get; set; } = null!;

        
        public int Telefone { get; set; }

        
        public virtual ICollection<Maquina> Maquinas { get; set; } = new List<Maquina>();

        
        public virtual ICollection<RegistoManutencao> RegistosManutencao { get; set; } = new List<RegistoManutencao>();
    }
}