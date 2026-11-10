using Microsoft.EntityFrameworkCore;
using VeiculosService.Models;

namespace VeiculosService
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Veiculo> Veiculos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Veiculo>().HasKey(v => v.Id);
            modelBuilder.Entity<Veiculo>().Property(v => v.Placa).IsRequired().HasMaxLength(15);
            modelBuilder.Entity<Veiculo>().Property(v => v.Modelo).IsRequired().HasMaxLength(80);
            modelBuilder.Entity<Veiculo>().Property(v => v.Cor).IsRequired().HasMaxLength(40);
            modelBuilder.Entity<Veiculo>().Property(v => v.TipoVeiculo).IsRequired().HasMaxLength(30);

            modelBuilder.Entity<Veiculo>().HasData(
                new Veiculo
                {
                    Id = 1,
                    Placa = "ABC1D23",
                    Modelo = "Toyota Corolla",
                    Cor = "Prata",
                    TipoVeiculo = "Carro",
                    NomeCondutor = "Carlos Eduardo",
                    TelefoneCondutor = "(48) 99911-2233",
                    DataHoraEntrada = DateTime.UtcNow.AddHours(-2),
                    DataHoraSaida = null,
                    AtivoNoPatio = true,
                    AtualizadoEm = DateTime.UtcNow
                },
                new Veiculo
                {
                    Id = 2,
                    Placa = "QWE4R56",
                    Modelo = "Honda Civic",
                    Cor = "Preto",
                    TipoVeiculo = "Carro",
                    NomeCondutor = "Mariana Souza",
                    TelefoneCondutor = "(48) 99822-3344",
                    DataHoraEntrada = DateTime.UtcNow.AddHours(-1),
                    DataHoraSaida = null,
                    AtivoNoPatio = true,
                    AtualizadoEm = DateTime.UtcNow
                },
                new Veiculo
                {
                    Id = 3,
                    Placa = "XYZ9K88",
                    Modelo = "Yamaha Fazer 250",
                    Cor = "Azul",
                    TipoVeiculo = "Moto",
                    NomeCondutor = "Lucas Almeida",
                    TelefoneCondutor = "(48) 99733-4455",
                    DataHoraEntrada = DateTime.UtcNow.AddHours(-5),
                    DataHoraSaida = DateTime.UtcNow.AddHours(-3),
                    AtivoNoPatio = false,
                    AtualizadoEm = DateTime.UtcNow
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
