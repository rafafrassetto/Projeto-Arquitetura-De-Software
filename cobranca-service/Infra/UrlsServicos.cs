using Microsoft.Extensions.Configuration;

namespace CobrancaService.Infra
{
    /// <summary>
    /// Resolve as URLs dos outros microsserviços a partir da configuração
    /// (appsettings.json ou variáveis de ambiente, ex.: ServicesUrls__VagasService).
    /// </summary>
    public static class UrlsServicos
    {
        public static string Obter(IConfiguration configuration, string nomeServico, string urlPadrao)
        {
            var url = configuration[$"ServicesUrls:{nomeServico}"];
            if (string.IsNullOrWhiteSpace(url))
                url = urlPadrao;

            url = url.Trim().TrimEnd('/');

            // Permite configurar apenas o host (ex.: "vagas-service.onrender.com") no Render
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = "https://" + url;
            }

            return url;
        }

        /// <summary>
        /// Timeout das chamadas entre serviços. O padrão é alto (60s) porque no plano Free do Render
        /// um serviço "dormindo" pode levar quase 1 minuto para acordar (cold start).
        /// </summary>
        public static TimeSpan Timeout(IConfiguration configuration)
        {
            var segundos = configuration.GetValue<int?>("Integracoes:TimeoutSegundos") ?? 60;
            return TimeSpan.FromSeconds(Math.Max(1, segundos));
        }
    }
}
