using System.Collections.Concurrent;
using System.Threading.Channels;

namespace CobrancaService.Infra
{
    public static class TiposEvento
    {
        /// <summary>Pagamento confirmado: registrar a saída do veículo no veiculos-service.</summary>
        public const string PagamentoConfirmado = "PagamentoConfirmado";

        /// <summary>A liberação síncrona da vaga falhou: tentar novamente em segundo plano.</summary>
        public const string LiberacaoVagaPendente = "LiberacaoVagaPendente";
    }

    public class EventoIntegracao
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Tipo { get; init; } = string.Empty;
        public int CobrancaId { get; init; }
        public int RecursoId { get; init; }
        public string? CorrelationId { get; init; }
        public DateTime CriadoEm { get; init; } = DateTime.UtcNow;
        public int Tentativas { get; set; }
        public string Status { get; set; } = "Pendente";
        public string? UltimaMensagem { get; set; }
        public DateTime? ProcessadoEm { get; set; }
    }

    /// <summary>
    /// Fila de eventos em memória (System.Threading.Channels) usada para a comunicação assíncrona
    /// entre o cobranca-service e os demais microsserviços. Quem publica não espera o processamento:
    /// o ProcessadorEventos (BackgroundService) consome a fila e faz as chamadas com retentativas.
    /// </summary>
    public class FilaEventos
    {
        private const int TamanhoHistorico = 100;

        private readonly Channel<EventoIntegracao> _canal = Channel.CreateUnbounded<EventoIntegracao>(
            new UnboundedChannelOptions { SingleReader = true });

        private readonly ConcurrentDictionary<Guid, EventoIntegracao> _historico = new();

        public ChannelReader<EventoIntegracao> Leitor => _canal.Reader;

        public void Publicar(EventoIntegracao evento)
        {
            _historico[evento.Id] = evento;
            LimparHistorico();
            _canal.Writer.TryWrite(evento);
        }

        public void Reenfileirar(EventoIntegracao evento)
        {
            _canal.Writer.TryWrite(evento);
        }

        public IEnumerable<EventoIntegracao> Historico(string? status = null)
        {
            var eventos = _historico.Values.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(status))
                eventos = eventos.Where(e => e.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

            return eventos.OrderByDescending(e => e.CriadoEm).ToList();
        }

        private void LimparHistorico()
        {
            if (_historico.Count <= TamanhoHistorico) return;

            foreach (var antigo in _historico.Values.OrderBy(e => e.CriadoEm).Take(_historico.Count - TamanhoHistorico))
                _historico.TryRemove(antigo.Id, out _);
        }
    }
}
