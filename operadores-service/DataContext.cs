using Microsoft.EntityFrameworkCore;
using OperadoresService.Models;

namespace OperadoresService
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Operador> Operadores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Operador>().HasKey(o => o.Id);
            modelBuilder.Entity<Operador>().Property(o => o.Nome).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Operador>().Property(o => o.Cpf).IsRequired().HasMaxLength(14);
            modelBuilder.Entity<Operador>().Property(o => o.Matricula).IsRequired().HasMaxLength(20);

            // Seed inicial de operadores
            modelBuilder.Entity<Operador>().HasData(
                new Operador
                {
                    Id = 1,
                    Nome = "Ana Paula Rodrigues",
                    Cpf = "111.222.333-44",
                    Matricula = "OP-1001",
                    Cargo = "Operador de Caixa",
                    Turno = "Manhã",
                    Ativo = true,
                    CriadoEm = DateTime.UtcNow
                },
                new Operador
                {
                    Id = 2,
                    Nome = "Lucas Oliveira Fernandes",
                    Cpf = "555.666.777-88",
                    Matricula = "OP-1002",
                    Cargo = "Manobrista",
                    Turno = "Tarde",
                    Ativo = true,
                    CriadoEm = DateTime.UtcNow
                },
                new Operador
                {
                    Id = 3,
                    Nome = "Juliana Costa Meneghel",
                    Cpf = "999.888.777-66",
                    Matricula = "ADM-001",
                    Cargo = "Administrador",
                    Turno = "Integral",
                    Ativo = true,
                    CriadoEm = DateTime.UtcNow
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
