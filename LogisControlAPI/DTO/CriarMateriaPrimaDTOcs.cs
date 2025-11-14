using System.ComponentModel.DataAnnotations;

namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO de input para criar nova matéria-prima.
    /// </summary>
    public class CriarMateriaPrimaDTO
    {
        [Required]
        public string Nome { get; set; } = null!;

        [Required]
        public int Quantidade { get; set; }

        [Required]
        public string Descricao { get; set; } = null!;

        [Required]
        public string Categoria { get; set; } = null!;

        [Required]
        public string CodInterno { get; set; } = null!;

        [Required]
        public double Preco { get; set; }
    }
}
