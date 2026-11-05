using Application.Abstractions.Gateways;
using Application.Abstractions.Services;
using Google.Cloud.Firestore;
using Infrastructure.Firestore;
using Infrastructure.Http;
using Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtIssuerOptions>(configuration.GetSection(JwtIssuerOptions.SectionName));
        services.Configure<FirestoreOptions>(configuration.GetSection(FirestoreOptions.SectionName));
        services.Configure<AdministrativoOptions>(configuration.GetSection(AdministrativoOptions.SectionName));

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtIssuer, JwtIssuer>();

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<FirestoreOptions>>().Value;
            if (string.IsNullOrWhiteSpace(options.ProjectId))
            {
                throw new InvalidOperationException("Firestore:ProjectId é obrigatório.");
            }

            var builder = new FirestoreDbBuilder
            {
                ProjectId = options.ProjectId,
                DatabaseId = options.DatabaseId
            };
            return builder.Build();
        });

        services.AddScoped<IUsuarioRecordStore, FirestoreUsuarioRecordStore>();
        services.AddScoped<IUsuarioGateway, FirestoreUsuarioGateway>();

        var administrativoBaseUrl = configuration[$"{AdministrativoOptions.SectionName}:BaseUrl"]
            ?? throw new InvalidOperationException("Administrativo:BaseUrl é obrigatório.");
        var serviceAuthKey = configuration["ServiceAuth:Key"]
            ?? throw new InvalidOperationException("ServiceAuth:Key é obrigatório.");

        services.AddHttpClient<IAdministrativoClienteGateway, AdministrativoClienteGateway>(client =>
        {
            client.BaseAddress = new Uri(administrativoBaseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Add("X-Service-Key", serviceAuthKey);
        });

        return services;
    }
}
