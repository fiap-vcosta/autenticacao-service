using Application.Abstractions.Gateways;
using Application.Abstractions.Services;
using Domain.Entities;

namespace Infrastructure.Firestore;

public sealed class FirestoreUsuarioGateway(
    IUsuarioRecordStore recordStore,
    IPasswordHasher passwordHasher) : IUsuarioGateway
{
    private const string AdminLogin = "admin";
    private const string AdminId = "00000000-0000-0000-0000-000000000001";

    public Task<Usuario?> GetByLoginAsync(string login, CancellationToken cancellationToken = default)
        => recordStore.FindByLoginAsync(login, cancellationToken);

    public async Task EnsureAdminSeedAsync(CancellationToken cancellationToken = default)
    {
        var existing = await recordStore.FindByLoginAsync(AdminLogin, cancellationToken);
        if (existing is not null)
        {
            return;
        }

        await recordStore.CreateAsync(
            new Usuario
            {
                Id = AdminId,
                Login = AdminLogin,
                SenhaHash = passwordHasher.HashPassword("admin"),
                TipoUsuario = TipoUsuario.Admin
            },
            cancellationToken);
    }
}
