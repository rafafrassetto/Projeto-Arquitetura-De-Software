using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CobrancaService.Infra
{
    /// <summary>
    /// Guarda o Correlation ID da requisição atual. Ele é recebido/gerado no middleware,
    /// devolvido no header da resposta e repassado nas chamadas HTTP para os outros microsserviços,
    /// permitindo rastrear um fluxo completo nos logs de todos os serviços.
    /// </summary>
    public static class Correlacao
    {
        public const string Header = "X-Correlation-Id";
        private static readonly AsyncLocal<string?> _atual = new();

        public static string? Atual
        {
            get => _atual.Value;
            set => _atual.Value = value;
        }
    }

    /// <summary>Repassa o Correlation ID em toda chamada feita via IHttpClientFactory.</summary>
    public class CorrelacaoHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var correlationId = Correlacao.Atual;
            if (!string.IsNullOrWhiteSpace(correlationId) && !request.Headers.Contains(Correlacao.Header))
                request.Headers.Add(Correlacao.Header, correlationId);

            return base.SendAsync(request, cancellationToken);
        }
    }

    /// <summary>Métricas simples em memória, expostas em GET /metricas.</summary>
    public class MetricasServico
    {
        private readonly DateTime _iniciadoEm = DateTime.UtcNow;
        private readonly ConcurrentDictionary<string, long> _porEndpoint = new();
        private long _total;
        private long _erros4xx;
        private long _erros5xx;
        private long _duracaoTotalMs;
        private long _duracaoMaximaMs;

        public void Registrar(string endpoint, int statusCode, long duracaoMs)
        {
            Interlocked.Increment(ref _total);
            Interlocked.Add(ref _duracaoTotalMs, duracaoMs);
            _porEndpoint.AddOrUpdate(endpoint, 1, (_, atual) => atual + 1);

            if (statusCode >= 500) Interlocked.Increment(ref _erros5xx);
            else if (statusCode >= 400) Interlocked.Increment(ref _erros4xx);

            long maximaAtual;
            do
            {
                maximaAtual = Interlocked.Read(ref _duracaoMaximaMs);
                if (duracaoMs <= maximaAtual) break;
            } while (Interlocked.CompareExchange(ref _duracaoMaximaMs, duracaoMs, maximaAtual) != maximaAtual);
        }

        public object Snapshot(string servico)
        {
            var total = Interlocked.Read(ref _total);
            return new
            {
                servico,
                iniciadoEm = _iniciadoEm,
                tempoAtivoSegundos = (long)(DateTime.UtcNow - _iniciadoEm).TotalSeconds,
                requisicoes = new
                {
                    total,
                    erros4xx = Interlocked.Read(ref _erros4xx),
                    erros5xx = Interlocked.Read(ref _erros5xx),
                    duracaoMediaMs = total == 0 ? 0 : Math.Round((double)Interlocked.Read(ref _duracaoTotalMs) / total, 1),
                    duracaoMaximaMs = Interlocked.Read(ref _duracaoMaximaMs)
                },
                porEndpoint = _porEndpoint.OrderByDescending(e => e.Value).ToDictionary(e => e.Key, e => e.Value)
            };
        }
    }

    /// <summary>Health check que valida a conexão com o banco SQLite do serviço.</summary>
    public class BancoDeDadosHealthCheck : IHealthCheck
    {
        private readonly DataContext _dataContext;

        public BancoDeDadosHealthCheck(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dataContext.Database.CanConnectAsync(cancellationToken)
                    ? HealthCheckResult.Healthy("Banco SQLite acessível.")
                    : HealthCheckResult.Unhealthy("Não foi possível conectar ao banco SQLite.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Erro ao acessar o banco SQLite.", ex);
            }
        }
    }

    public static class ObservabilidadeExtensions
    {
        /// <summary>Registra logs estruturados (JSON), métricas, health checks e propagação de Correlation ID.</summary>
        public static WebApplicationBuilder AdicionarObservabilidade(this WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddJsonConsole(options =>
            {
                options.IncludeScopes = true;
                options.UseUtcTimestamp = true;
                options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
            });

            builder.Services.AddSingleton<MetricasServico>();
            builder.Services.AddTransient<CorrelacaoHandler>();
            builder.Services.ConfigureHttpClientDefaults(http => http.AddHttpMessageHandler<CorrelacaoHandler>());
            builder.Services.AddHealthChecks().AddCheck<BancoDeDadosHealthCheck>("sqlite");

            return builder;
        }

        /// <summary>Adiciona o middleware de log/métricas por requisição e os endpoints /health e /metricas.</summary>
        public static WebApplication UsarObservabilidade(this WebApplication app, string nomeServico)
        {
            var metricas = app.Services.GetRequiredService<MetricasServico>();
            var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Requisicoes");

            app.Use(async (context, next) =>
            {
                var correlationId = context.Request.Headers[Correlacao.Header].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(correlationId))
                    correlationId = Guid.NewGuid().ToString("N");

                Correlacao.Atual = correlationId;
                context.Response.Headers[Correlacao.Header] = correlationId;

                using var escopo = logger.BeginScope(new Dictionary<string, object>
                {
                    ["Servico"] = nomeServico,
                    ["CorrelationId"] = correlationId
                });

                var cronometro = Stopwatch.StartNew();
                var statusCode = 500;
                try
                {
                    await next();
                    statusCode = context.Response.StatusCode;
                }
                finally
                {
                    cronometro.Stop();
                    var endpoint = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? context.Request.Path.Value ?? "/";
                    var chave = $"{context.Request.Method} /{endpoint.TrimStart('/')}";
                    metricas.Registrar(chave, statusCode, cronometro.ElapsedMilliseconds);

                    var nivel = statusCode >= 500 ? LogLevel.Error : statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
                    logger.Log(nivel, "HTTP {Metodo} {Caminho} respondeu {StatusCode} em {DuracaoMs} ms",
                        context.Request.Method, context.Request.Path.Value, statusCode, cronometro.ElapsedMilliseconds);
                }
            });

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, relatorio) =>
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    var resposta = new
                    {
                        servico = nomeServico,
                        status = relatorio.Status.ToString(),
                        horario = DateTime.UtcNow,
                        duracaoMs = Math.Round(relatorio.TotalDuration.TotalMilliseconds, 1),
                        verificacoes = relatorio.Entries.ToDictionary(
                            e => e.Key,
                            e => new { status = e.Value.Status.ToString(), descricao = e.Value.Description })
                    };
                    await context.Response.WriteAsync(JsonSerializer.Serialize(resposta, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
                }
            });

            app.MapGet("/metricas", () => Results.Ok(metricas.Snapshot(nomeServico)));

            return app;
        }
    }
}
