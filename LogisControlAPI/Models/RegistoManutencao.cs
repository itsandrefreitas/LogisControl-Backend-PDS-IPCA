using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models;

public partial class RegistoManutencao
{
    public int RegistoManutencaoId { get; set; }

    public string Descricao { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public int PedidoManutencaoPedidoManutId { get; set; }

    public int UtilizadorUtilizadorId { get; set; }

    public int? AssistenciaExternaAssistenteId { get; set; }

    public virtual AssistenciaExterna? AssistenciaExternaAssistente { get; set; } = null!;

    public virtual PedidoManutencao PedidoManutencaoPedidoManut { get; set; } = null!;

    public virtual Utilizador UtilizadorUtilizador { get; set; } = null!;
}
