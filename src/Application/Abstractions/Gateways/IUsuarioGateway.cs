using Domain.Entities;

namespace Application.Abstractions.Gateways;

public interface IUsuarioGateway
{
    Task<Usuario?> GetByLoginAsync(string login, CancellationToken cancellationToken = default);
    Task EnsureAdminSeedAsync(CancellationToken cancellationToken = default);
}
