using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models;

public partial class Maquina
{
    public int MaquinaId { get; set; }

    public string Nome { get; set; } = null!;

    public int LinhaProd { get; set; }

    public int AssistenciaExternaAssistenteId { get; set; }

    public virtual AssistenciaExterna AssistenciaExternaAssistente { get; set; } = null!;

    public virtual ICollection<OrdemProducao> OrdensProducao { get; set; } = new List<OrdemProducao>();

    public virtual ICollection<PedidoManutencao> PedidosManutencao { get; set; } = new List<PedidoManutencao>();
}
