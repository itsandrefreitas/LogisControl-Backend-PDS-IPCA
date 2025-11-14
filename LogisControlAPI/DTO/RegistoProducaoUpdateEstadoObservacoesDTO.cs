using LogisControlAPI.Models;

namespace LogisControlAPI.DTO
{
    public partial class RegistoProducaoUpdateEstadoObservacoesDTO
    {
        public string Estado { get; set; }
        public string? Observacoes { get; set; }
    }
}
