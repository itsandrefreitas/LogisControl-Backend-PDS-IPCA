namespace LogisControlAPI.DTO
{
    /// <summary>
    /// DTO que representa um fornecedor para listagem/leitura.
    /// </summary>
    public class FornecedorDTO
    {
        public int FornecedorId { get; set; }
        public string Nome { get; set; } = null!;
        public int? Telefone { get; set; }
        public string? Email { get; set; }
    }
}