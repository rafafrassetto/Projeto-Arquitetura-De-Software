using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using VagasService.DTO;
using VagasService.Infra;
using VagasService.Models;

namespace VagasService.Servicos
{
    public class ServVagas
    {
        private readonly DataContext _dataContext;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ServicoIa _servicoIa;
        private readonly ILogger _logger;

        public ServVagas()
        {
            _dataContext = GeradorDeServicos.CarregarContexto();
            _httpClientFactory = GeradorDeServicos.CarregarHttpClientFactory();
            _configuration = GeradorDeServicos.CarregarConfiguration();
            _servicoIa = new ServicoIa(_configuration);
            _logger = GeradorDeServicos.CarregarLogger<ServVagas>();
        }

        public List<VagaDTO> Listar(string? status = null, string? setor = null)
        {
            var query = _dataContext.Vagas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(v => v.Status.ToLower() == status.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(setor))
            {
                query = query.Where(v => v.Setor.ToLower().Contains(setor.ToLower()));
            }

            return query
                .OrderBy(v => v.Codigo)
                .Select(v => new VagaDTO
                {
                    Id = v.Id,
                    Codigo = v.Codigo,
                    Setor = v.Setor,
                    TipoVaga = v.TipoVaga,
                    Status = v.Status,
                    VeiculoId = v.VeiculoId,
                    AtualizadoEm = v.AtualizadoEm
                })
                .ToList();
        }

        public async Task<VagaDTO?> ObterPorId(int id, bool buscarVeiculo = true)
        {
            var vaga = await _dataContext.Vagas.FindAsync(id);
            if (vaga == null) return null;

            var dto = new VagaDTO
            {
                Id = vaga.Id,
                Codigo = vaga.Codigo,
                Setor = vaga.Setor,
                TipoVaga = vaga.TipoVaga,
                Status = vaga.Status,
                VeiculoId = vaga.VeiculoId,
                AtualizadoEm = vaga.AtualizadoEm
            };

            // 1ª Busca de dados: Se a vaga estiver ocupada e tiver VeiculoId, busca dados no veiculos-service
            if (buscarVeiculo && vaga.VeiculoId.HasValue)
            {
                dto.VeiculoOcupante = await ConsultarVeiculoOcupante(vaga.VeiculoId.Value);
            }

            return dto;
        }

        public VagaDTO Criar(CriarVagaDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Codigo))
                throw new ArgumentException("O código da vaga é obrigatório.");

            if (_dataContext.Vagas.Any(v => v.Codigo.ToLower() == dto.Codigo.ToLower()))
                throw new InvalidOperationException($"Já existe uma vaga cadastrada com o código '{dto.Codigo}'.");

            var vaga = new Vaga
            {
                Codigo = dto.Codigo.Trim().ToUpper(),
                Setor = dto.Setor.Trim(),
                TipoVaga = string.IsNullOrWhiteSpace(dto.TipoVaga) ? "Carro" : dto.TipoVaga.Trim(),
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Liberada" : dto.Status.Trim(),
                AtualizadoEm = DateTime.UtcNow
            };

            _dataContext.Vagas.Add(vaga);
            _dataContext.SaveChanges();

            return new VagaDTO
            {
                Id = vaga.Id,
                Codigo = vaga.Codigo,
                Setor = vaga.Setor,
                TipoVaga = vaga.TipoVaga,
                Status = vaga.Status,
                VeiculoId = vaga.VeiculoId,
                AtualizadoEm = vaga.AtualizadoEm
            };
        }

        public VagaDTO Atualizar(int id, AtualizarVagaDTO dto)
        {
            var vaga = _dataContext.Vagas.Find(id)
                ?? throw new KeyNotFoundException($"Vaga com ID {id} não encontrada.");

            if (!string.IsNullOrWhiteSpace(dto.Codigo) && dto.Codigo.ToUpper() != vaga.Codigo)
            {
                if (_dataContext.Vagas.Any(v => v.Id != id && v.Codigo.ToLower() == dto.Codigo.ToLower()))
                    throw new InvalidOperationException($"Já existe outra vaga com o código '{dto.Codigo}'.");
                vaga.Codigo = dto.Codigo.Trim().ToUpper();
            }

            if (!string.IsNullOrWhiteSpace(dto.Setor)) vaga.Setor = dto.Setor.Trim();
            if (!string.IsNullOrWhiteSpace(dto.TipoVaga)) vaga.TipoVaga = dto.TipoVaga.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Status)) vaga.Status = dto.Status.Trim();

            vaga.AtualizadoEm = DateTime.UtcNow;
            _dataContext.SaveChanges();

            return new VagaDTO
            {
                Id = vaga.Id,
                Codigo = vaga.Codigo,
                Setor = vaga.Setor,
                TipoVaga = vaga.TipoVaga,
                Status = vaga.Status,
                VeiculoId = vaga.VeiculoId,
                AtualizadoEm = vaga.AtualizadoEm
            };
        }

        public VagaDTO Alocar(int id, int veiculoId)
        {
            var vaga = _dataContext.Vagas.Find(id)
                ?? throw new KeyNotFoundException($"Vaga com ID {id} não encontrada.");

            if (vaga.Status.Equals("Ocupada", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"A vaga {vaga.Codigo} já está ocupada pelo veículo ID {vaga.VeiculoId}.");

            vaga.Status = "Ocupada";
            vaga.VeiculoId = veiculoId;
            vaga.AtualizadoEm = DateTime.UtcNow;

            _dataContext.SaveChanges();

            return new VagaDTO
            {
                Id = vaga.Id,
                Codigo = vaga.Codigo,
                Setor = vaga.Setor,
                TipoVaga = vaga.TipoVaga,
                Status = vaga.Status,
                VeiculoId = vaga.VeiculoId,
                AtualizadoEm = vaga.AtualizadoEm
            };
        }

        public VagaDTO Liberar(int id)
        {
            var vaga = _dataContext.Vagas.Find(id)
                ?? throw new KeyNotFoundException($"Vaga com ID {id} não encontrada.");

            vaga.Status = "Liberada";
            vaga.VeiculoId = null;
            vaga.AtualizadoEm = DateTime.UtcNow;

            _dataContext.SaveChanges();

            return new VagaDTO
            {
                Id = vaga.Id,
                Codigo = vaga.Codigo,
                Setor = vaga.Setor,
                TipoVaga = vaga.TipoVaga,
                Status = vaga.Status,
                VeiculoId = null,
                AtualizadoEm = vaga.AtualizadoEm
            };
        }

        public void Deletar(int id)
        {
            var vaga = _dataContext.Vagas.Find(id)
                ?? throw new KeyNotFoundException($"Vaga com ID {id} não encontrada.");

            _dataContext.Vagas.Remove(vaga);
            _dataContext.SaveChanges();
        }

        // Funcionalidade criativa + Integração com IA (Issue #11):
        // 1) a vaga é escolhida por uma regra determinística (tipo compatível, ordem de setor/código);
        // 2) a análise da ocupação é gerada pelo Claude (Anthropic) quando ANTHROPIC_API_KEY está configurada,
        //    com fallback para uma mensagem heurística local.
        public async Task<SugestaoVagaDTO> SugerirVagaIa(string? tipoVeiculo)
        {
            var todas = _dataContext.Vagas.ToList();
            var totalVagas = todas.Count;
            var ocupadas = todas.Count(v => v.Status.Equals("Ocupada", StringComparison.OrdinalIgnoreCase));
            var emManutencao = todas.Count(v => v.Status.Equals("Manutencao", StringComparison.OrdinalIgnoreCase));
            var livres = todas.Where(v => v.Status.Equals("Liberada", StringComparison.OrdinalIgnoreCase)).ToList();

            var taxaOcupacao = totalVagas > 0 ? Math.Round((double)ocupadas / totalVagas * 100, 1) : 0;

            var candidatas = livres;
            var tipoAtendido = true;
            if (!string.IsNullOrWhiteSpace(tipoVeiculo))
            {
                var filtradas = livres.Where(v => v.TipoVaga.Equals(tipoVeiculo, StringComparison.OrdinalIgnoreCase)).ToList();
                if (filtradas.Any()) candidatas = filtradas;
                else tipoAtendido = false;
            }

            var selecionada = candidatas.OrderBy(v => v.Setor).ThenBy(v => v.Codigo).FirstOrDefault();

            string motivo;
            if (selecionada == null)
                motivo = "Nenhuma vaga livre no momento.";
            else if (!tipoAtendido)
                motivo = $"Não há vaga livre do tipo '{tipoVeiculo}'. Vaga {selecionada.Codigo} ({selecionada.TipoVaga}, {selecionada.Setor}) sugerida como alternativa.";
            else
                motivo = $"Vaga {selecionada.Codigo} ({selecionada.TipoVaga}, {selecionada.Setor}) é a primeira vaga livre compatível, por ordem de setor e código.";

            var mensagemHeuristica = taxaOcupacao >= 80
                ? $"Atenção: pátio com {taxaOcupacao}% de ocupação, próximo da capacidade máxima. Priorize saídas rápidas."
                : $"Fluxo regular: {ocupadas} de {totalVagas} vagas ocupadas ({taxaOcupacao}%).";

            var sugestao = new SugestaoVagaDTO
            {
                VagaSugerida = selecionada != null ? new VagaDTO
                {
                    Id = selecionada.Id,
                    Codigo = selecionada.Codigo,
                    Setor = selecionada.Setor,
                    TipoVaga = selecionada.TipoVaga,
                    Status = selecionada.Status,
                    VeiculoId = selecionada.VeiculoId,
                    AtualizadoEm = selecionada.AtualizadoEm
                } : null,
                MotivoRecomendacao = motivo,
                TaxaOcupacaoGeral = taxaOcupacao,
                TotalVagasLivres = livres.Count,
                MensagemIa = mensagemHeuristica,
                FonteAnalise = "Heurística local (configure ANTHROPIC_API_KEY para gerar a análise com IA)"
            };

            if (!_servicoIa.Habilitada)
                return sugestao;

            try
            {
                var dados = System.Text.Json.JsonSerializer.Serialize(new
                {
                    tipoVeiculoSolicitado = string.IsNullOrWhiteSpace(tipoVeiculo) ? "não informado" : tipoVeiculo,
                    totalVagas,
                    ocupadas,
                    emManutencao,
                    livres = livres.Count,
                    taxaOcupacaoPercentual = taxaOcupacao,
                    vagasLivresPorTipo = livres.GroupBy(v => v.TipoVaga).ToDictionary(g => g.Key, g => g.Count()),
                    vagaSugerida = selecionada == null ? null : new { selecionada.Codigo, selecionada.Setor, selecionada.TipoVaga },
                    motivoDaRegra = motivo
                });

                var analise = await _servicoIa.GerarAnaliseOcupacao(dados);
                if (!string.IsNullOrWhiteSpace(analise))
                {
                    sugestao.MensagemIa = analise;
                    sugestao.FonteAnalise = $"IA generativa ({_servicoIa.Modelo})";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao gerar análise com IA; usando heurística local.");
                sugestao.FonteAnalise = "Heurística local (a IA não respondeu)";
            }

            return sugestao;
        }

        private async Task<VeiculoResumoDTO?> ConsultarVeiculoOcupante(int veiculoId)
        {
            try
            {
                var baseUrl = UrlsServicos.Obter(_configuration, "VeiculosService", "http://localhost:5002");

                var client = _httpClientFactory.CreateClient();
                client.Timeout = UrlsServicos.Timeout(_configuration);

                var response = await client.GetAsync($"{baseUrl}/api/veiculos/{veiculoId}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<VeiculoResumoDTO>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AVISO] Não foi possível consultar veiculos-service para o veículo {veiculoId}: {ex.Message}");
            }

            return null;
        }
    }
}
