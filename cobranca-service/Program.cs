using CobrancaService;
using CobrancaService.Infra;
using CobrancaService.Servicos;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.AdicionarObservabilidade();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

// Comunicação assíncrona: a MESMA instância da fila é compartilhada entre o provider do template
// (GeradorDeServicos) e o host da aplicação, onde roda o worker ProcessadorEventos.
var filaEventos = new FilaEventos();
builder.Services.AddSingleton(filaEventos);
builder.Services.AddHostedService<ProcessadorEventos>();

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
);

GeradorDeServicos.ServiceProvider = builder.Services.BuildServiceProvider();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dataContext.Database.EnsureCreated();
}

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                       ForwardedHeaders.XForwardedProto
};

forwardedHeadersOptions.KnownIPNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

app.UsarObservabilidade("cobranca-service");

app.MapOpenApi();
app.MapScalarApiReference();

app.UseCors();
app.UseAuthorization();
app.MapControllers();

app.Run();
