namespace OperadoresService.Models
{
    public class Operador
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string Cargo { get; set; } = "Operador de Caixa"; // Operador de Caixa, Manobrista, Administrador, Supervisor
        public string Turno { get; set; } = "Manhã"; // Manhã, Tarde, Noite, Integral
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
