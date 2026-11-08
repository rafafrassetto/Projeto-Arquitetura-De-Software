using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VagasService.Infra
{
    public static class GeradorDeServicos
    {
        public static ServiceProvider ServiceProvider { get; set; } = null!;

        public static DataContext CarregarContexto()
        {
            return ServiceProvider.GetRequiredService<DataContext>();
        }

        public static IHttpClientFactory CarregarHttpClientFactory()
        {
            return ServiceProvider.GetRequiredService<IHttpClientFactory>();
        }

        public static IConfiguration CarregarConfiguration()
        {
            return ServiceProvider.GetRequiredService<IConfiguration>();
        }
    }
}
