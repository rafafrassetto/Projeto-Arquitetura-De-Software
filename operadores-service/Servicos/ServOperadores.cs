using OperadoresService.DTO;
using OperadoresService.Infra;
using OperadoresService.Models;

namespace OperadoresService.Servicos
{
    public class ServOperadores
    {
        private readonly DataContext _dataContext;

        public ServOperadores()
        {
            _dataContext = GeradorDeServicos.CarregarContexto();
        }

        public List<OperadorDTO> Listar(string? turno = null, string? cargo = null, bool? ativo = null)
        {
            var query = _dataContext.Operadores.AsQueryable();

            if (!string.IsNullOrWhiteSpace(turno))
            {
                query = query.Where(o => o.Turno.ToLower() == turno.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(cargo))
            {
                query = query.Where(o => o.Cargo.ToLower().Contains(cargo.ToLower()));
            }

            if (ativo.HasValue)
            {
                query = query.Where(o => o.Ativo == ativo.Value);
            }

            return query
                .OrderBy(o => o.Nome)
                .Select(o => new OperadorDTO
                {
                    Id = o.Id,
                    Nome = o.Nome,
                    Cpf = o.Cpf,
                    Matricula = o.Matricula,
                    Cargo = o.Cargo,
                    Turno = o.Turno,
                    Ativo = o.Ativo,
                    CriadoEm = o.CriadoEm
                })
                .ToList();
        }

        public OperadorDTO? ObterPorId(int id)
        {
            var o = _dataContext.Operadores.Find(id);
            if (o == null) return null;

            return new OperadorDTO
            {
                Id = o.Id,
                Nome = o.Nome,
                Cpf = o.Cpf,
                Matricula = o.Matricula,
                Cargo = o.Cargo,
                Turno = o.Turno,
                Ativo = o.Ativo,
                CriadoEm = o.CriadoEm
            };
        }

        public OperadorDTO? ObterPorMatricula(string matricula)
        {
            var o = _dataContext.Operadores
                .FirstOrDefault(x => x.Matricula.ToUpper() == matricula.Trim().ToUpper());
            if (o == null) return null;

            return new OperadorDTO
            {
                Id = o.Id,
                Nome = o.Nome,
                Cpf = o.Cpf,
                Matricula = o.Matricula,
                Cargo = o.Cargo,
                Turno = o.Turno,
                Ativo = o.Ativo,
                CriadoEm = o.CriadoEm
            };
        }

        public OperadorDTO Criar(CriarOperadorDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nome))
                throw new ArgumentException("O nome do operador é obrigatório.");

            if (string.IsNullOrWhiteSpace(dto.Matricula))
                throw new ArgumentException("A matrícula do operador é obrigatória.");

            var matriculaFormatada = dto.Matricula.Trim().ToUpper();

            if (_dataContext.Operadores.Any(o => o.Matricula.ToUpper() == matriculaFormatada))
                throw new InvalidOperationException($"Já existe um operador cadastrado com a matrícula '{matriculaFormatada}'.");

            var operador = new Operador
            {
                Nome = dto.Nome.Trim(),
                Cpf = dto.Cpf?.Trim() ?? string.Empty,
                Matricula = matriculaFormatada,
                Cargo = string.IsNullOrWhiteSpace(dto.Cargo) ? "Operador de Caixa" : dto.Cargo.Trim(),
                Turno = string.IsNullOrWhiteSpace(dto.Turno) ? "Manhã" : dto.Turno.Trim(),
                Ativo = true,
                CriadoEm = DateTime.UtcNow
            };

            _dataContext.Operadores.Add(operador);
            _dataContext.SaveChanges();

            return new OperadorDTO
            {
                Id = operador.Id,
                Nome = operador.Nome,
                Cpf = operador.Cpf,
                Matricula = operador.Matricula,
                Cargo = operador.Cargo,
                Turno = operador.Turno,
                Ativo = operador.Ativo,
                CriadoEm = operador.CriadoEm
            };
        }

        public OperadorDTO Atualizar(int id, AtualizarOperadorDTO dto)
        {
            var operador = _dataContext.Operadores.Find(id)
                ?? throw new KeyNotFoundException($"Operador com ID {id} não encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.Matricula) && dto.Matricula.ToUpper() != operador.Matricula)
            {
                var novaMatricula = dto.Matricula.Trim().ToUpper();
                if (_dataContext.Operadores.Any(o => o.Id != id && o.Matricula.ToUpper() == novaMatricula))
                    throw new InvalidOperationException($"Já existe outro operador com a matrícula '{novaMatricula}'.");
                operador.Matricula = novaMatricula;
            }

            if (!string.IsNullOrWhiteSpace(dto.Nome)) operador.Nome = dto.Nome.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Cpf)) operador.Cpf = dto.Cpf.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Cargo)) operador.Cargo = dto.Cargo.Trim();
            if (!string.IsNullOrWhiteSpace(dto.Turno)) operador.Turno = dto.Turno.Trim();
            if (dto.Ativo.HasValue) operador.Ativo = dto.Ativo.Value;

            _dataContext.SaveChanges();

            return new OperadorDTO
            {
                Id = operador.Id,
                Nome = operador.Nome,
                Cpf = operador.Cpf,
                Matricula = operador.Matricula,
                Cargo = operador.Cargo,
                Turno = operador.Turno,
                Ativo = operador.Ativo,
                CriadoEm = operador.CriadoEm
            };
        }

        public OperadorDTO AlternarStatus(int id, bool ativo)
        {
            var operador = _dataContext.Operadores.Find(id)
                ?? throw new KeyNotFoundException($"Operador com ID {id} não encontrado.");

            operador.Ativo = ativo;
            _dataContext.SaveChanges();

            return new OperadorDTO
            {
                Id = operador.Id,
                Nome = operador.Nome,
                Cpf = operador.Cpf,
                Matricula = operador.Matricula,
                Cargo = operador.Cargo,
                Turno = operador.Turno,
                Ativo = operador.Ativo,
                CriadoEm = operador.CriadoEm
            };
        }

        public void Deletar(int id)
        {
            var operador = _dataContext.Operadores.Find(id)
                ?? throw new KeyNotFoundException($"Operador com ID {id} não encontrado.");

            _dataContext.Operadores.Remove(operador);
            _dataContext.SaveChanges();
        }
    }
}
