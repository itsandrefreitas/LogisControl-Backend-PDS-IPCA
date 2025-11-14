using System;

namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO de leitura para matérias-primas.
    /// </summary>
    public class MateriaPrimaDTO
    {
        public int MateriaPrimaId { get; set; }
        public string Nome { get; set; } = null!;
        public int Quantidade { get; set; }
        public string Descricao { get; set; } = null!;
        public string Categoria { get; set; } = null!;
        public string CodInterno { get; set; } = null!;
        public double Preco { get; set; }
    }
}
