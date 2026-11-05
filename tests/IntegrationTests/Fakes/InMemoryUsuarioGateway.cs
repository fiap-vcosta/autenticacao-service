using Application.Abstractions.Gateways;
using Application.Abstractions.Services;
using Domain.Entities;

namespace IntegrationTests.Fakes;

public sealed class InMemoryUsuarioGateway(IPasswordHasher passwordHasher) : IUsuarioGateway
{
    private readonly Dictionary<string, Usuario> _usuarios = new(StringComparer.OrdinalIgnoreCase);

    public Task<Usuario?> GetByLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        _usuarios.TryGetValue(login, out var usuario);
        return Task.FromResult(usuario);
    }

    public Task EnsureAdminSeedAsync(CancellationToken cancellationToken = default)
    {
        if (_usuarios.ContainsKey("admin"))
        {
            return Task.CompletedTask;
        }

        _usuarios["admin"] = new Usuario
        {
            Id = "00000000-0000-0000-0000-000000000001",
            Login = "admin",
            SenhaHash = passwordHasher.HashPassword("admin"),
            TipoUsuario = TipoUsuario.Admin
        };
        return Task.CompletedTask;
    }
}
