namespace OperadoresService.DTO
{
    public class OperadorDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string Turno { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class CriarOperadorDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string Cargo { get; set; } = "Operador de Caixa";
        public string Turno { get; set; } = "Manhã";
    }

    public class AtualizarOperadorDTO
    {
        public string? Nome { get; set; }
        public string? Cpf { get; set; }
        public string? Matricula { get; set; }
        public string? Cargo { get; set; }
        public string? Turno { get; set; }
        public bool? Ativo { get; set; }
    }

    public class AlterarStatusDTO
    {
        public bool Ativo { get; set; }
    }
}
