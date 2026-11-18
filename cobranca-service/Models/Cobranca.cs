namespace CobrancaService.Models
{
    public class Cobranca
    {
        public int Id { get; set; }
        public int VeiculoId { get; set; }
        public int VagaId { get; set; }
        public int OperadorId { get; set; }
        public DateTime DataHoraEntrada { get; set; } = DateTime.UtcNow;
        public DateTime? DataHoraSaida { get; set; }
        public decimal ValorHora { get; set; } = 10.00m;
        public decimal ValorTotal { get; set; }
        public string? FormaPagamento { get; set; }
        public string StatusPagamento { get; set; } = "Pendente";
        public DateTime? AtualizadoEm { get; set; } = DateTime.UtcNow;
    }
}
