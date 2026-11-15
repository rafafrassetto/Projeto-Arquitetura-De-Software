using Microsoft.Extensions.DependencyInjection;

namespace OperadoresService.Infra
{
    public static class GeradorDeServicos
    {
        public static ServiceProvider ServiceProvider { get; set; } = null!;

        public static DataContext CarregarContexto()
        {
            return ServiceProvider.GetRequiredService<DataContext>();
        }
    }
}
