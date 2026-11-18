using CobrancaService.DTO;
using CobrancaService.Infra;
using CobrancaService.Models;

namespace CobrancaService.Servicos
{
    public class ServCobrancas
    {
        private static readonly string[] StatusValidos = ["Pendente", "Pago", "Cancelado"];
        private readonly DataContext _dataContext;

        public ServCobrancas()
        {
            _dataContext = GeradorDeServicos.CarregarContexto();
        }

        public List<CobrancaDTO> Listar(string? statusPagamento = null, int? veiculoId = null, int? vagaId = null, int? operadorId = null)
        {
            var query = _dataContext.Cobrancas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(statusPagamento))
            {
                var filtroStatus = statusPagamento.Trim().ToLower();
                query = query.Where(c => c.StatusPagamento.ToLower() == filtroStatus);
            }

            if (veiculoId.HasValue)
            {
                query = query.Where(c => c.VeiculoId == veiculoId.Value);
            }

            if (vagaId.HasValue)
            {
                query = query.Where(c => c.VagaId == vagaId.Value);
            }

            if (operadorId.HasValue)
            {
                query = query.Where(c => c.OperadorId == operadorId.Value);
            }

            return query
                .OrderByDescending(c => c.DataHoraEntrada)
                .ToList()
                .Select(MapearParaDTO)
                .ToList();
        }

        public CobrancaDTO? ObterPorId(int id)
        {
            var cobranca = _dataContext.Cobrancas.Find(id);
            if (cobranca == null) return null;

            return MapearParaDTO(cobranca);
        }

        public SimulacaoTarifaDTO CalcularTarifa(int id)
        {
            var cobranca = _dataContext.Cobrancas.Find(id)
                ?? throw new KeyNotFoundException($"Cobrança com ID {id} não encontrada.");

            var dataCalculo = cobranca.DataHoraSaida ?? DateTime.UtcNow;
            var dataFinal = dataCalculo < cobranca.DataHoraEntrada ? cobranca.DataHoraEntrada : dataCalculo;
            var (minutos, horasCobradas, valorCalculado) = CalcularEstadia(cobranca.DataHoraEntrada, dataFinal, cobranca.ValorHora);

            return new SimulacaoTarifaDTO
            {
                CobrancaId = cobranca.Id,
                DataHoraEntrada = cobranca.DataHoraEntrada,
                DataHoraCalculo = dataFinal,
                TempoPermanenciaMinutos = minutos,
                HorasCobradas = horasCobradas,
                ValorHora = cobranca.ValorHora,
                ValorCalculado = valorCalculado,
                StatusPagamento = cobranca.StatusPagamento
            };
        }

        public CobrancaDTO Criar(CriarCobrancaDTO dto)
        {
            if (dto.VeiculoId <= 0)
                throw new ArgumentException("O ID do veículo deve ser maior que zero.");

            if (dto.VagaId <= 0)
                throw new ArgumentException("O ID da vaga deve ser maior que zero.");

            if (dto.OperadorId <= 0)
                throw new ArgumentException("O ID do operador deve ser maior que zero.");

            if (dto.ValorHora <= 0)
                throw new ArgumentException("O valor da hora deve ser maior que zero.");

            var dataEntrada = dto.DataHoraEntrada ?? DateTime.UtcNow;

            if (dto.DataHoraSaida.HasValue && dto.DataHoraSaida.Value < dataEntrada)
                throw new ArgumentException("A data/hora de saída não pode ser anterior à data/hora de entrada.");

            if (_dataContext.Cobrancas.Any(c => c.VeiculoId == dto.VeiculoId && c.StatusPagamento.ToLower() == "pendente"))
                throw new InvalidOperationException($"Já existe uma cobrança pendente para o veículo ID {dto.VeiculoId}.");

            var dataCalculo = dto.DataHoraSaida ?? DateTime.UtcNow;
            var dataFinal = dataCalculo < dataEntrada ? dataEntrada : dataCalculo;
            var (_, _, valorTotal) = CalcularEstadia(dataEntrada, dataFinal, dto.ValorHora);

            var cobranca = new Cobranca
            {
                VeiculoId = dto.VeiculoId,
                VagaId = dto.VagaId,
                OperadorId = dto.OperadorId,
                DataHoraEntrada = dataEntrada,
                DataHoraSaida = dto.DataHoraSaida,
                ValorHora = dto.ValorHora,
                ValorTotal = valorTotal,
                FormaPagamento = string.IsNullOrWhiteSpace(dto.FormaPagamento) ? null : dto.FormaPagamento.Trim(),
                StatusPagamento = "Pendente",
                AtualizadoEm = DateTime.UtcNow
            };

            _dataContext.Cobrancas.Add(cobranca);
            _dataContext.SaveChanges();

            return MapearParaDTO(cobranca);
        }

        public CobrancaDTO Atualizar(int id, AtualizarCobrancaDTO dto)
        {
            var cobranca = _dataContext.Cobrancas.Find(id)
                ?? throw new KeyNotFoundException($"Cobrança com ID {id} não encontrada.");

            if (dto.OperadorId.HasValue)
            {
                if (dto.OperadorId.Value <= 0)
                    throw new ArgumentException("O ID do operador deve ser maior que zero.");
                cobranca.OperadorId = dto.OperadorId.Value;
            }

            if (dto.ValorHora.HasValue)
            {
                if (dto.ValorHora.Value <= 0)
                    throw new ArgumentException("O valor da hora deve ser maior que zero.");
                cobranca.ValorHora = dto.ValorHora.Value;
            }

            if (dto.DataHoraEntrada.HasValue)
                cobranca.DataHoraEntrada = dto.DataHoraEntrada.Value;

            if (dto.DataHoraSaida.HasValue)
                cobranca.DataHoraSaida = dto.DataHoraSaida.Value;

            if (cobranca.DataHoraSaida.HasValue && cobranca.DataHoraSaida.Value < cobranca.DataHoraEntrada)
                throw new ArgumentException("A data/hora de saída não pode ser anterior à data/hora de entrada.");

            if (dto.FormaPagamento != null)
                cobranca.FormaPagamento = string.IsNullOrWhiteSpace(dto.FormaPagamento) ? null : dto.FormaPagamento.Trim();

            if (!string.IsNullOrWhiteSpace(dto.StatusPagamento))
            {
                var statusNormalizado = StatusValidos.FirstOrDefault(s => s.Equals(dto.StatusPagamento.Trim(), StringComparison.OrdinalIgnoreCase))
                    ?? throw new ArgumentException("Status de pagamento inválido. Utilize: Pendente, Pago ou Cancelado.");
                cobranca.StatusPagamento = statusNormalizado;
            }

            var dataCalculo = cobranca.DataHoraSaida ?? DateTime.UtcNow;
            var dataFinal = dataCalculo < cobranca.DataHoraEntrada ? cobranca.DataHoraEntrada : dataCalculo;
            var (_, _, valorTotal) = CalcularEstadia(cobranca.DataHoraEntrada, dataFinal, cobranca.ValorHora);

            cobranca.ValorTotal = valorTotal;
            cobranca.AtualizadoEm = DateTime.UtcNow;

            _dataContext.SaveChanges();

            return MapearParaDTO(cobranca);
        }

        public CobrancaDTO ConfirmarPagamento(int id, ConfirmarPagamentoDTO? dto = null)
        {
            var cobranca = _dataContext.Cobrancas.Find(id)
                ?? throw new KeyNotFoundException($"Cobrança com ID {id} não encontrada.");

            if (cobranca.StatusPagamento.Equals("Pago", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"A cobrança ID {id} já foi paga.");

            if (cobranca.StatusPagamento.Equals("Cancelado", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Não é possível pagar a cobrança ID {id} pois ela está cancelada.");

            var dataSaida = dto?.DataHoraSaida ?? cobranca.DataHoraSaida ?? DateTime.UtcNow;

            if (dataSaida < cobranca.DataHoraEntrada)
                throw new ArgumentException("A data/hora de saída não pode ser anterior à data/hora de entrada.");

            var (_, _, valorTotal) = CalcularEstadia(cobranca.DataHoraEntrada, dataSaida, cobranca.ValorHora);

            var formaInformada = dto?.FormaPagamento;

            cobranca.DataHoraSaida = dataSaida;
            cobranca.ValorTotal = valorTotal;
            cobranca.FormaPagamento = string.IsNullOrWhiteSpace(formaInformada)
                ? (cobranca.FormaPagamento ?? "Pix")
                : formaInformada.Trim();
            cobranca.StatusPagamento = "Pago";
            cobranca.AtualizadoEm = DateTime.UtcNow;

            _dataContext.SaveChanges();

            return MapearParaDTO(cobranca);
        }

        public CobrancaDTO Cancelar(int id)
        {
            var cobranca = _dataContext.Cobrancas.Find(id)
                ?? throw new KeyNotFoundException($"Cobrança com ID {id} não encontrada.");

            if (cobranca.StatusPagamento.Equals("Pago", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Não é possível cancelar a cobrança ID {id} pois ela já foi paga.");

            if (cobranca.StatusPagamento.Equals("Cancelado", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"A cobrança ID {id} já está cancelada.");

            cobranca.StatusPagamento = "Cancelado";
            cobranca.AtualizadoEm = DateTime.UtcNow;

            _dataContext.SaveChanges();

            return MapearParaDTO(cobranca);
        }

        public void Deletar(int id)
        {
            var cobranca = _dataContext.Cobrancas.Find(id)
                ?? throw new KeyNotFoundException($"Cobrança com ID {id} não encontrada.");

            _dataContext.Cobrancas.Remove(cobranca);
            _dataContext.SaveChanges();
        }

        private static (int Minutos, int HorasCobradas, decimal ValorTotal) CalcularEstadia(DateTime dataEntrada, DateTime dataSaida, decimal valorHora)
        {
            var duracao = dataSaida - dataEntrada;
            var minutos = Math.Max(0, (int)Math.Floor(duracao.TotalMinutes));
            var horasCobradas = minutos == 0 ? 1 : (int)Math.Ceiling(minutos / 60.0);
            var valorTotal = Math.Round(horasCobradas * valorHora, 2);

            return (minutos, horasCobradas, valorTotal);
        }

        private static CobrancaDTO MapearParaDTO(Cobranca c)
        {
            var dataReferencia = c.DataHoraSaida ?? DateTime.UtcNow;
            var minutos = dataReferencia > c.DataHoraEntrada
                ? (int)Math.Floor((dataReferencia - c.DataHoraEntrada).TotalMinutes)
                : 0;

            return new CobrancaDTO
            {
                Id = c.Id,
                VeiculoId = c.VeiculoId,
                VagaId = c.VagaId,
                OperadorId = c.OperadorId,
                DataHoraEntrada = c.DataHoraEntrada,
                DataHoraSaida = c.DataHoraSaida,
                TempoPermanenciaMinutos = minutos,
                ValorHora = c.ValorHora,
                ValorTotal = c.ValorTotal,
                FormaPagamento = c.FormaPagamento,
                StatusPagamento = c.StatusPagamento,
                AtualizadoEm = c.AtualizadoEm
            };
        }
    }
}
