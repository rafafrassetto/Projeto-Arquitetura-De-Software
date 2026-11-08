namespace VagasService.Models
{
    public class Vaga
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Setor { get; set; } = string.Empty;
        public string TipoVaga { get; set; } = "Carro"; // Carro, Moto, PCD, Idoso
        public string Status { get; set; } = "Liberada"; // Liberada, Ocupada, Manutencao
        public int? VeiculoId { get; set; }
        public DateTime? AtualizadoEm { get; set; } = DateTime.UtcNow;
    }
}
