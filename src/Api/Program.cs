using Api.Extensions;
using Application.Abstractions.Gateways;
using BuildingBlocks.Http.Logging;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    if (builder.Environment.IsDevelopment())
    {
        TryLoadLocalJwtSigningKey(builder);
    }

    builder.Host.UseSerilog(DatadogLogging.Configure);
    builder.Services.AddApiServices(builder.Configuration);
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.CustomSchemaIds(type => type.FullName?.Replace("+", ".") ?? type.Name);
    });

    var app = builder.Build();
    app.UseApiConfiguration();

    using (var scope = app.Services.CreateScope())
    {
        var usuarios = scope.ServiceProvider.GetRequiredService<IUsuarioGateway>();
        await usuarios.EnsureAdminSeedAsync();
    }

    app.MapGet("/autenticacao/health", () => Results.Ok("OK"));

    await app.RunAsync();

    static void TryLoadLocalJwtSigningKey(WebApplicationBuilder webApplicationBuilder)
    {
        var privateKeyPath = Path.GetFullPath(
            Path.Combine(webApplicationBuilder.Environment.ContentRootPath, "..", "..", ".local", "jwt", "private.pem"));
        if (!File.Exists(privateKeyPath))
        {
            return;
        }

        var privatePem = File.ReadAllText(privateKeyPath);
        webApplicationBuilder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:SigningKeyPem"] = privatePem
        });
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação encerrada inesperadamente");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
