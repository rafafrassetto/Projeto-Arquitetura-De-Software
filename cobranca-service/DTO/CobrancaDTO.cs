namespace CobrancaService.DTO
{
    public class CobrancaDTO
    {
        public int Id { get; set; }
        public int VeiculoId { get; set; }
        public int VagaId { get; set; }
        public int OperadorId { get; set; }
        public DateTime DataHoraEntrada { get; set; }
        public DateTime? DataHoraSaida { get; set; }
        public int TempoPermanenciaMinutos { get; set; }
        public decimal ValorHora { get; set; }
        public decimal ValorTotal { get; set; }
        public string? FormaPagamento { get; set; }
        public string StatusPagamento { get; set; } = string.Empty;
        public DateTime? AtualizadoEm { get; set; }
    }

    public class CriarCobrancaDTO
    {
        public int VeiculoId { get; set; }
        public int VagaId { get; set; }
        public int OperadorId { get; set; }
        public DateTime? DataHoraEntrada { get; set; }
        public DateTime? DataHoraSaida { get; set; }
        public decimal ValorHora { get; set; } = 10.00m;
        public string? FormaPagamento { get; set; }
    }

    public class AtualizarCobrancaDTO
    {
        public int? OperadorId { get; set; }
        public DateTime? DataHoraEntrada { get; set; }
        public DateTime? DataHoraSaida { get; set; }
        public decimal? ValorHora { get; set; }
        public string? FormaPagamento { get; set; }
        public string? StatusPagamento { get; set; }
    }

    public class ConfirmarPagamentoDTO
    {
        public string? FormaPagamento { get; set; }
        public DateTime? DataHoraSaida { get; set; }
    }

    public class PagamentoConfirmadoDTO
    {
        public CobrancaDTO Cobranca { get; set; } = new();
        public bool VagaLiberada { get; set; }
        public string IntegracaoVagas { get; set; } = string.Empty;
    }

    public class SimulacaoTarifaDTO
    {
        public int CobrancaId { get; set; }
        public DateTime DataHoraEntrada { get; set; }
        public DateTime DataHoraCalculo { get; set; }
        public int TempoPermanenciaMinutos { get; set; }
        public int HorasCobradas { get; set; }
        public decimal ValorHora { get; set; }
        public decimal ValorCalculado { get; set; }
        public string StatusPagamento { get; set; } = string.Empty;
    }

    public class VeiculoConsultaDTO
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
    }
}
