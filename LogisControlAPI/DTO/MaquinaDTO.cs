using LogisControlAPI.Models;

namespace LogisControlAPI.DTO
{
    public class MaquinaDTO
    {
        public int MaquinaId { get; set; }

        public string Nome { get; set; } = null!;

        public int LinhaProd { get; set; }

        public int AssistenciaExternaAssistenteId { get; set; }

    }
}
