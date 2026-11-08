using Microsoft.EntityFrameworkCore;
using VagasService.Models;

namespace VagasService
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Vaga> Vagas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vaga>().HasKey(v => v.Id);
            modelBuilder.Entity<Vaga>().Property(v => v.Codigo).IsRequired().HasMaxLength(20);
            modelBuilder.Entity<Vaga>().Property(v => v.Setor).IsRequired().HasMaxLength(50);
            modelBuilder.Entity<Vaga>().Property(v => v.Status).IsRequired().HasMaxLength(30);

            // Seed inicial de vagas para testes imediatos
            modelBuilder.Entity<Vaga>().HasData(
                new Vaga { Id = 1, Codigo = "A-01", Setor = "Setor A (Principal)", TipoVaga = "Carro", Status = "Liberada", VeiculoId = null, AtualizadoEm = DateTime.UtcNow },
                new Vaga { Id = 2, Codigo = "A-02", Setor = "Setor A (Principal)", TipoVaga = "Carro", Status = "Ocupada", VeiculoId = 1, AtualizadoEm = DateTime.UtcNow },
                new Vaga { Id = 3, Codigo = "B-01", Setor = "Setor B (Motos)", TipoVaga = "Moto", Status = "Liberada", VeiculoId = null, AtualizadoEm = DateTime.UtcNow },
                new Vaga { Id = 4, Codigo = "C-01", Setor = "Setor C (Preferencial)", TipoVaga = "PCD", Status = "Liberada", VeiculoId = null, AtualizadoEm = DateTime.UtcNow }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
