namespace VeiculosService.Models
{
    public class Veiculo
    {
        public int Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        public string TipoVeiculo { get; set; } = "Carro";
        public string? NomeCondutor { get; set; }
        public string? TelefoneCondutor { get; set; }
        public DateTime DataHoraEntrada { get; set; } = DateTime.UtcNow;
        public DateTime? DataHoraSaida { get; set; }
        public bool AtivoNoPatio { get; set; } = true;
        public DateTime? AtualizadoEm { get; set; } = DateTime.UtcNow;
    }
}
