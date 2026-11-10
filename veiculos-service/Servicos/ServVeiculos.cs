using VeiculosService.DTO;
using VeiculosService.Infra;
using VeiculosService.Models;

namespace VeiculosService.Servicos
{
    public class ServVeiculos
    {
        private readonly DataContext _dataContext;

        public ServVeiculos()
        {
            _dataContext = GeradorDeServicos.CarregarContexto();
        }

        public List<VeiculoDTO> Listar(bool? ativoNoPatio = null, string? placa = null)
        {
            var query = _dataContext.Veiculos.AsQueryable();

            if (ativoNoPatio.HasValue)
            {
                query = query.Where(v => v.AtivoNoPatio == ativoNoPatio.Value);
            }

            if (!string.IsNullOrWhiteSpace(placa))
            {
                var filtroPlaca = NormalizarPlaca(placa);
                query = query.Where(v => v.Placa.ToUpper().Contains(filtroPlaca));
            }

            return query
                .OrderByDescending(v => v.DataHoraEntrada)
                .Select(v => MapearParaDTO(v))
                .ToList();
        }

        public VeiculoDTO? ObterPorId(int id)
        {
            var veiculo = _dataContext.Veiculos.Find(id);
            if (veiculo == null) return null;

            return MapearParaDTO(veiculo);
        }

        public VeiculoDTO? ObterPorPlaca(string placa)
        {
            if (string.IsNullOrWhiteSpace(placa)) return null;

            var placaNormalizada = NormalizarPlaca(placa);
            var veiculo = _dataContext.Veiculos
                .FirstOrDefault(v => v.Placa.ToUpper() == placaNormalizada);

            if (veiculo == null) return null;

            return MapearParaDTO(veiculo);
        }

        public VeiculoDTO Criar(CriarVeiculoDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Placa))
                throw new ArgumentException("A placa do veículo é obrigatória.");

            if (string.IsNullOrWhiteSpace(dto.Modelo))
                throw new ArgumentException("O modelo do veículo é obrigatório.");

            var placaNormalizada = NormalizarPlaca(dto.Placa);

            if (_dataContext.Veiculos.Any(v => v.Placa.ToUpper() == placaNormalizada))
                throw new InvalidOperationException($"Já existe um veículo cadastrado com a placa '{placaNormalizada}'.");

            var veiculo = new Veiculo
            {
                Placa = placaNormalizada,
                Modelo = dto.Modelo.Trim(),
                Cor = string.IsNullOrWhiteSpace(dto.Cor) ? "Não informada" : dto.Cor.Trim(),
                TipoVeiculo = string.IsNullOrWhiteSpace(dto.TipoVeiculo) ? "Carro" : dto.TipoVeiculo.Trim(),
                NomeCondutor = string.IsNullOrWhiteSpace(dto.NomeCondutor) ? null : dto.NomeCondutor.Trim(),
                TelefoneCondutor = string.IsNullOrWhiteSpace(dto.TelefoneCondutor) ? null : dto.TelefoneCondutor.Trim(),
                DataHoraEntrada = DateTime.UtcNow,
                DataHoraSaida = dto.AtivoNoPatio ? null : DateTime.UtcNow,
                AtivoNoPatio = dto.AtivoNoPatio,
                AtualizadoEm = DateTime.UtcNow
            };

            _dataContext.Veiculos.Add(veiculo);
            _dataContext.SaveChanges();

            return MapearParaDTO(veiculo);
        }

        public VeiculoDTO Atualizar(int id, AtualizarVeiculoDTO dto)
        {
            var veiculo = _dataContext.Veiculos.Find(id)
                ?? throw new KeyNotFoundException($"Veículo com ID {id} não encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.Placa))
            {
                var novaPlaca = NormalizarPlaca(dto.Placa);
                if (novaPlaca != veiculo.Placa.ToUpper())
                {
                    if (_dataContext.Veiculos.Any(v => v.Id != id && v.Placa.ToUpper() == novaPlaca))
                        throw new InvalidOperationException($"Já existe outro veículo cadastrado com a placa '{novaPlaca}'.");

                    veiculo.Placa = novaPlaca;
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Modelo)) veiculo.Modelo = dto.Modelo.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Cor)) veiculo.Cor = dto.Cor.Trim();
            if (!string.IsNullOrWhiteSpace(dto.TipoVeiculo)) veiculo.TipoVeiculo = dto.TipoVeiculo.Trim();
            if (dto.NomeCondutor != null) veiculo.NomeCondutor = string.IsNullOrWhiteSpace(dto.NomeCondutor) ? null : dto.NomeCondutor.Trim();
            if (dto.TelefoneCondutor != null) veiculo.TelefoneCondutor = string.IsNullOrWhiteSpace(dto.TelefoneCondutor) ? null : dto.TelefoneCondutor.Trim();

            if (dto.AtivoNoPatio.HasValue && dto.AtivoNoPatio.Value != veiculo.AtivoNoPatio)
            {
                veiculo.AtivoNoPatio = dto.AtivoNoPatio.Value;
                if (veiculo.AtivoNoPatio)
                {
                    veiculo.DataHoraEntrada = DateTime.UtcNow;
                    veiculo.DataHoraSaida = null;
                }
                else
                {
                    veiculo.DataHoraSaida = DateTime.UtcNow;
                }
            }

            veiculo.AtualizadoEm = DateTime.UtcNow;
            _dataContext.SaveChanges();

            return MapearParaDTO(veiculo);
        }

        public VeiculoDTO RegistrarEntrada(int id)
        {
            var veiculo = _dataContext.Veiculos.Find(id)
                ?? throw new KeyNotFoundException($"Veículo com ID {id} não encontrado.");

            veiculo.AtivoNoPatio = true;
            veiculo.DataHoraEntrada = DateTime.UtcNow;
            veiculo.DataHoraSaida = null;
            veiculo.AtualizadoEm = DateTime.UtcNow;

            _dataContext.SaveChanges();

            return MapearParaDTO(veiculo);
        }

        public VeiculoDTO RegistrarSaida(int id)
        {
            var veiculo = _dataContext.Veiculos.Find(id)
                ?? throw new KeyNotFoundException($"Veículo com ID {id} não encontrado.");

            veiculo.AtivoNoPatio = false;
            veiculo.DataHoraSaida = DateTime.UtcNow;
            veiculo.AtualizadoEm = DateTime.UtcNow;

            _dataContext.SaveChanges();

            return MapearParaDTO(veiculo);
        }

        public void Deletar(int id)
        {
            var veiculo = _dataContext.Veiculos.Find(id)
                ?? throw new KeyNotFoundException($"Veículo com ID {id} não encontrado.");

            _dataContext.Veiculos.Remove(veiculo);
            _dataContext.SaveChanges();
        }

        private static string NormalizarPlaca(string placa)
        {
            return placa.Replace("-", "").Trim().ToUpper();
        }

        private static VeiculoDTO MapearParaDTO(Veiculo v)
        {
            return new VeiculoDTO
            {
                Id = v.Id,
                Placa = v.Placa,
                Modelo = v.Modelo,
                Cor = v.Cor,
                TipoVeiculo = v.TipoVeiculo,
                NomeCondutor = v.NomeCondutor,
                TelefoneCondutor = v.TelefoneCondutor,
                DataHoraEntrada = v.DataHoraEntrada,
                DataHoraSaida = v.DataHoraSaida,
                AtivoNoPatio = v.AtivoNoPatio,
                AtualizadoEm = v.AtualizadoEm
            };
        }
    }
}
