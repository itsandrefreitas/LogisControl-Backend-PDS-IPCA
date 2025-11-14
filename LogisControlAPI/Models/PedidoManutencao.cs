using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models;

public partial class PedidoManutencao
{
    public int PedidoManutId { get; set; }

    public string Descricao { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public DateTime DataAbertura { get; set; }

    public DateTime? DataConclusao { get; set; }

    public int MaquinaMaquinaId { get; set; }

    public int UtilizadorUtilizadorId { get; set; }

    public virtual Maquina MaquinaMaquina { get; set; } = null!;

    public virtual ICollection<RegistoManutencao> RegistosManutencao { get; set; } = new List<RegistoManutencao>();

    public virtual Utilizador UtilizadorUtilizador { get; set; } = null!;


}
