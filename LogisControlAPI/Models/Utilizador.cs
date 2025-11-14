using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models;

public partial class Utilizador
{
    public int UtilizadorId { get; set; }

    public string PrimeiroNome { get; set; } = null!;

    public string Sobrenome { get; set; } = null!;

    public int NumFuncionario { get; set; }

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool Estado { get; set; }

    public virtual ICollection<PedidoCompra> PedidosCompra { get; set; } = new List<PedidoCompra>();

    public virtual ICollection<PedidoManutencao> PedidosManutencao { get; set; } = new List<PedidoManutencao>();

    public virtual ICollection<RegistoManutencao> RegistosManutencao { get; set; } = new List<RegistoManutencao>();

    public virtual ICollection<RegistoProducao> RegistosProducao { get; set; } = new List<RegistoProducao>();
}
