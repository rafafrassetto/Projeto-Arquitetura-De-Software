using Anthropic;
using Anthropic.Models.Beta.Messages;

namespace VagasService.Servicos
{
    /// <summary>
    /// Integração com IA generativa (Claude, da Anthropic) para gerar uma análise da ocupação do pátio.
    /// A chave é lida de ANTHROPIC_API_KEY (ou IA:ApiKey). Sem chave, ou em caso de falha,
    /// o vagas-service continua funcionando com a heurística local.
    /// </summary>
    public class ServicoIa
    {
        public const string ModeloPadrao = "claude-opus-5-5";

        private const string InstrucoesSistema =
            "Você é o assistente de operação de um estacionamento rotativo. " +
            "Você recebe um JSON com a situação atual das vagas do pátio e a vaga pré-selecionada para o próximo veículo. " +
            "Escreva em português do Brasil, em no máximo 3 frases curtas, uma orientação para o atendente: " +
            "como está a ocupação, por que a vaga sugerida é adequada e um alerta operacional se houver (pátio quase cheio, " +
            "falta de vagas do tipo pedido, vagas em manutenção). Use apenas os dados recebidos. " +
            "Responda somente com o texto, sem markdown e sem listas.";

        private readonly IConfiguration _configuration;

        public ServicoIa(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Modelo => string.IsNullOrWhiteSpace(_configuration["IA:Modelo"]) ? ModeloPadrao : _configuration["IA:Modelo"]!;

        public bool Habilitada => !string.IsNullOrWhiteSpace(ObterChave());

        private string? ObterChave()
        {
            var chave = _configuration["IA:ApiKey"];
            return string.IsNullOrWhiteSpace(chave) ? _configuration["ANTHROPIC_API_KEY"] : chave;
        }

        /// <summary>Retorna a análise gerada pela IA, ou null se a IA não estiver disponível.</summary>
        public async Task<string?> GerarAnaliseOcupacao(string dadosOcupacaoJson)
        {
            if (!Habilitada) return null;

            using var cancelamento = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            AnthropicClient client = new() { ApiKey = ObterChave() };

            var resposta = await client.Beta.Messages.Create(new MessageCreateParams
            {
                Model = Modelo,
                MaxTokens = 4096,
                System = InstrucoesSistema,
                OutputConfig = new BetaOutputConfig { Effort = Effort.Low },
                // Se o modelo principal recusar a requisição, o servidor tenta automaticamente um modelo alternativo
                Betas = ["server-side-fallback-2026-07-01"],
                Fallbacks = new Default(),
                Messages = [new BetaUserMessageParam { Content = dadosOcupacaoJson }],
            }, cancelamento.Token);

            if (resposta.StopReason == "refusal")
                return null;

            var texto = string.Concat(resposta.Content.Select(b => b.Value).OfType<BetaTextBlock>().Select(t => t.Text)).Trim();
            return string.IsNullOrWhiteSpace(texto) ? null : texto;
        }
    }
}
