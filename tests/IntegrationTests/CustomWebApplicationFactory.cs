using System.Security.Cryptography;
using Application.Abstractions.Gateways;
using Application.Abstractions.Services;
using Google.Cloud.Firestore;
using Infrastructure.Firestore;
using Infrastructure.Services;
using IntegrationTests.Fakes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public StubAdministrativoClienteGateway Administrativo { get; } = new();

    private readonly string _privatePem;

    public CustomWebApplicationFactory()
    {
        using var rsa = RSA.Create(2048);
        _privatePem = rsa.ExportRSAPrivateKeyPem();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SigningKeyPem"] = _privatePem,
                ["Jwt:KeyId"] = "autenticacao-2026",
                ["Jwt:Funcionario:Issuer"] = "autenticacao-service",
                ["Jwt:Funcionario:Audience"] = "funcionario",
                ["Jwt:Cliente:Issuer"] = "autenticacao-service",
                ["Jwt:Cliente:Audience"] = "cliente",
                ["Firestore:ProjectId"] = "test-project",
                ["Firestore:DatabaseId"] = "autenticacao",
                ["Administrativo:BaseUrl"] = "http://localhost:9",
                ["ServiceAuth:Key"] = "test-service-key"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IUsuarioGateway>();
            services.RemoveAll<IUsuarioRecordStore>();
            services.RemoveAll<IAdministrativoClienteGateway>();
            services.RemoveAll<FirestoreDb>();

            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<IUsuarioGateway>(sp =>
                new InMemoryUsuarioGateway(sp.GetRequiredService<IPasswordHasher>()));
            services.AddSingleton<IAdministrativoClienteGateway>(Administrativo);
        });
    }
}
