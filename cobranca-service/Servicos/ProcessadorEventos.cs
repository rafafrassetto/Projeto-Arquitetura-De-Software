using CobrancaService.Infra;

namespace CobrancaService.Servicos
{
    /// <summary>
    /// Worker em segundo plano que consome a FilaEventos e executa as integrações assíncronas:
    /// - PagamentoConfirmado     -> PATCH /api/veiculos/{id}/saida   (veiculos-service)
    /// - LiberacaoVagaPendente   -> PATCH /api/vagas/{id}/liberar    (vagas-service)
    /// Em caso de falha, o evento é reenfileirado com backoff exponencial (2s, 4s, 8s, 16s...).
    /// </summary>
    public class ProcessadorEventos : BackgroundService
    {
        private const int MaximoTentativas = 5;

        private readonly FilaEventos _fila;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProcessadorEventos> _logger;

        public ProcessadorEventos(FilaEventos fila, IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<ProcessadorEventos> logger)
        {
            _fila = fila;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var evento in _fila.Leitor.ReadAllAsync(stoppingToken))
            {
                Correlacao.Atual = evento.CorrelationId;
                using var escopo = _logger.BeginScope(new Dictionary<string, object>
                {
                    ["Servico"] = "cobranca-service",
                    ["CorrelationId"] = evento.CorrelationId ?? string.Empty,
                    ["EventoId"] = evento.Id
                });

                evento.Tentativas++;
                evento.Status = "Processando";

                var (sucesso, mensagem) = await Processar(evento, stoppingToken);
                evento.UltimaMensagem = mensagem;

                if (sucesso)
                {
                    evento.Status = "Concluido";
                    evento.ProcessadoEm = DateTime.UtcNow;
                    _logger.LogInformation("Evento {Tipo} da cobrança {CobrancaId} processado na tentativa {Tentativa}: {Mensagem}",
                        evento.Tipo, evento.CobrancaId, evento.Tentativas, mensagem);
                    continue;
                }

                if (evento.Tentativas >= MaximoTentativas)
                {
                    evento.Status = "Falhou";
                    evento.ProcessadoEm = DateTime.UtcNow;
                    _logger.LogError("Evento {Tipo} da cobrança {CobrancaId} descartado após {Tentativas} tentativas: {Mensagem}",
                        evento.Tipo, evento.CobrancaId, evento.Tentativas, mensagem);
                    continue;
                }

                var espera = TimeSpan.FromSeconds(Math.Pow(2, evento.Tentativas));
                evento.Status = "AguardandoNovaTentativa";
                _logger.LogWarning("Evento {Tipo} da cobrança {CobrancaId} falhou (tentativa {Tentativa}); nova tentativa em {EsperaSegundos}s: {Mensagem}",
                    evento.Tipo, evento.CobrancaId, evento.Tentativas, espera.TotalSeconds, mensagem);

                // Reenfileira sem bloquear o processamento dos demais eventos
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(espera, stoppingToken);
                        _fila.Reenfileirar(evento);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }, stoppingToken);
            }
        }

        private async Task<(bool Sucesso, string Mensagem)> Processar(EventoIntegracao evento, CancellationToken cancellationToken)
        {
            string url;
            switch (evento.Tipo)
            {
                case TiposEvento.PagamentoConfirmado:
                    url = $"{UrlsServicos.Obter(_configuration, "VeiculosService", "http://localhost:5002")}/api/veiculos/{evento.RecursoId}/saida";
                    break;
                case TiposEvento.LiberacaoVagaPendente:
                    url = $"{UrlsServicos.Obter(_configuration, "VagasService", "http://localhost:5001")}/api/vagas/{evento.RecursoId}/liberar";
                    break;
                default:
                    return (true, $"Tipo de evento desconhecido '{evento.Tipo}' ignorado.");
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = UrlsServicos.Timeout(_configuration);

                var response = await client.PatchAsync(url, content: null, cancellationToken);

                if (response.IsSuccessStatusCode)
                    return (true, $"PATCH {url} respondeu {(int)response.StatusCode}.");

                // 404 não adianta tentar de novo: o recurso não existe no outro serviço
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return (true, $"PATCH {url} respondeu 404 (recurso inexistente); evento encerrado sem nova tentativa.");

                return (false, $"PATCH {url} respondeu {(int)response.StatusCode}.");
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !cancellationToken.IsCancellationRequested)
            {
                return (false, $"Falha de comunicação em PATCH {url}: {ex.Message}");
            }
        }
    }
}
