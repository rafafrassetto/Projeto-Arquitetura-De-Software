using CobrancaService.Models;
using Microsoft.EntityFrameworkCore;

namespace CobrancaService
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Cobranca> Cobrancas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cobranca>().HasKey(c => c.Id);
            modelBuilder.Entity<Cobranca>().Property(c => c.VeiculoId).IsRequired();
            modelBuilder.Entity<Cobranca>().Property(c => c.VagaId).IsRequired();
            modelBuilder.Entity<Cobranca>().Property(c => c.OperadorId).IsRequired();
            modelBuilder.Entity<Cobranca>().Property(c => c.ValorHora).HasPrecision(10, 2);
            modelBuilder.Entity<Cobranca>().Property(c => c.ValorTotal).HasPrecision(10, 2);
            modelBuilder.Entity<Cobranca>().Property(c => c.FormaPagamento).HasMaxLength(40);
            modelBuilder.Entity<Cobranca>().Property(c => c.StatusPagamento).IsRequired().HasMaxLength(30);

            modelBuilder.Entity<Cobranca>().HasData(
                new Cobranca
                {
                    Id = 1,
                    VeiculoId = 1,
                    VagaId = 2,
                    OperadorId = 1,
                    DataHoraEntrada = DateTime.UtcNow.AddHours(-2),
                    DataHoraSaida = null,
                    ValorHora = 10.00m,
                    ValorTotal = 20.00m,
                    FormaPagamento = null,
                    StatusPagamento = "Pendente",
                    AtualizadoEm = DateTime.UtcNow
                },
                new Cobranca
                {
                    Id = 2,
                    VeiculoId = 3,
                    VagaId = 3,
                    OperadorId = 1,
                    DataHoraEntrada = DateTime.UtcNow.AddHours(-5),
                    DataHoraSaida = DateTime.UtcNow.AddHours(-3),
                    ValorHora = 6.00m,
                    ValorTotal = 12.00m,
                    FormaPagamento = "Pix",
                    StatusPagamento = "Pago",
                    AtualizadoEm = DateTime.UtcNow
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
