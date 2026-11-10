namespace VeiculosService.DTO
{
    public class VeiculoDTO
    {
        public int Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        public string TipoVeiculo { get; set; } = string.Empty;
        public string? NomeCondutor { get; set; }
        public string? TelefoneCondutor { get; set; }
        public DateTime DataHoraEntrada { get; set; }
        public DateTime? DataHoraSaida { get; set; }
        public bool AtivoNoPatio { get; set; }
        public DateTime? AtualizadoEm { get; set; }
    }

    public class CriarVeiculoDTO
    {
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        public string TipoVeiculo { get; set; } = "Carro";
        public string? NomeCondutor { get; set; }
        public string? TelefoneCondutor { get; set; }
        public bool AtivoNoPatio { get; set; } = true;
    }

    public class AtualizarVeiculoDTO
    {
        public string? Placa { get; set; }
        public string? Modelo { get; set; }
        public string? Cor { get; set; }
        public string? TipoVeiculo { get; set; }
        public string? NomeCondutor { get; set; }
        public string? TelefoneCondutor { get; set; }
        public bool? AtivoNoPatio { get; set; }
    }
}
