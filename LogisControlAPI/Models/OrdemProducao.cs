using System;
using System.Collections.Generic;

namespace LogisControlAPI.Models;

public partial class OrdemProducao
{
    public int OrdemProdId { get; set; }

    public string Estado { get; set; } = null!;

    public int Quantidade { get; set; }

    public DateTime DataAbertura { get; set; }

    public DateTime? DataConclusao { get; set; }

    public int MaquinaMaquinaId { get; set; }

    public int EncomendaClienteEncomendaClienteId { get; set; }

    public virtual EncomendaCliente EncomendaClienteEncomendaCliente { get; set; } = null!;

    public virtual Maquina MaquinaMaquina { get; set; } = null!;

    public virtual ICollection<ProdMateriais> ProdMateriais { get; set; } = new List<ProdMateriais>();

    public virtual ICollection<Produto> Produtos { get; set; } = new List<Produto>();

    public virtual ICollection<RegistoProducao> RegistosProducao { get; set; } = new List<RegistoProducao>();
}
