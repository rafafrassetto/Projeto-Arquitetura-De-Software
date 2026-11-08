namespace VagasService.DTO
{
    public class VagaDTO
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Setor { get; set; } = string.Empty;
        public string TipoVaga { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? VeiculoId { get; set; }
        public DateTime? AtualizadoEm { get; set; }
        public VeiculoResumoDTO? VeiculoOcupante { get; set; }
    }

    public class CriarVagaDTO
    {
        public string Codigo { get; set; } = string.Empty;
        public string Setor { get; set; } = string.Empty;
        public string TipoVaga { get; set; } = "Carro";
        public string Status { get; set; } = "Liberada";
    }

    public class AtualizarVagaDTO
    {
        public string? Codigo { get; set; }
        public string? Setor { get; set; }
        public string? TipoVaga { get; set; }
        public string? Status { get; set; }
    }

    public class AlocarVagaDTO
    {
        public int VeiculoId { get; set; }
    }

    public class VeiculoResumoDTO
    {
        public int Id { get; set; }
        public string Placa { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        public string? NomeCondutor { get; set; }
        public string? TelefoneCondutor { get; set; }
        public DateTime DataHoraEntrada { get; set; }
        public bool AtivoNoPatio { get; set; }
    }

    public class SugestaoVagaDTO
    {
        public VagaDTO? VagaSugerida { get; set; }
        public string MotivoRecomendacao { get; set; } = string.Empty;
        public double TaxaOcupacaoGeral { get; set; }
        public int TotalVagasLivres { get; set; }
        public string MensagemIa { get; set; } = string.Empty;
    }
}
