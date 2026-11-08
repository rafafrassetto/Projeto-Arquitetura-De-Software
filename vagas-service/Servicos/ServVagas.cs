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

        public ServVagas()
        {
            _dataContext = GeradorDeServicos.CarregarContexto();
            _httpClientFactory = GeradorDeServicos.CarregarHttpClientFactory();
            _configuration = GeradorDeServicos.CarregarConfiguration();
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

        public SugestaoVagaDTO SugerirVagaIa(string? tipoVeiculo)
        {
            var todas = _dataContext.Vagas.ToList();
            var totalVagas = todas.Count;
            var ocupadas = todas.Count(v => v.Status.Equals("Ocupada", StringComparison.OrdinalIgnoreCase));
            var livres = todas.Where(v => v.Status.Equals("Liberada", StringComparison.OrdinalIgnoreCase)).ToList();

            var taxaOcupacao = totalVagas > 0 ? Math.Round((double)ocupadas / totalVagas * 100, 1) : 0;

            var candidatas = livres;
            if (!string.IsNullOrWhiteSpace(tipoVeiculo))
            {
                var filtradas = livres.Where(v => v.TipoVaga.Equals(tipoVeiculo, StringComparison.OrdinalIgnoreCase)).ToList();
                if (filtradas.Any()) candidatas = filtradas;
            }

            var selecionada = candidatas.OrderBy(v => v.Setor).ThenBy(v => v.Codigo).FirstOrDefault();

            return new SugestaoVagaDTO
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
                MotivoRecomendacao = selecionada != null 
                    ? $"Algoritmo IA: Vaga {selecionada.Codigo} ({selecionada.Setor}) recomendada para otimizar rotatividade com ocupação geral em {taxaOcupacao}%."
                    : "Nenhuma vaga livre encontrada para o tipo solicitado.",
                TaxaOcupacaoGeral = taxaOcupacao,
                TotalVagasLivres = livres.Count,
                MensagemIa = taxaOcupacao >= 80 
                    ? "Atenção IA: Pátio próximo da capacidade máxima. Direcionando para vagas de evasão rápida."
                    : "Fluxo regular de pátio com boa distribuição de vagas."
            };
        }

        private async Task<VeiculoResumoDTO?> ConsultarVeiculoOcupante(int veiculoId)
        {
            try
            {
                var baseUrl = _configuration["ServicesUrls:VeiculosService"] 
                           ?? _configuration["ServicosUrls:VeiculosService"] 
                           ?? "http://localhost:5002";

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                var response = await client.GetAsync($"{baseUrl.TrimEnd('/')}/api/veiculos/{veiculoId}");
                if (!response.IsSuccessStatusCode)
                {
                    response = await client.GetAsync($"{baseUrl.TrimEnd('/')}/veiculos/{veiculoId}");
                }

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
