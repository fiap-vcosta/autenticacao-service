using Domain.Entities;

namespace Infrastructure.Firestore;

public interface IUsuarioRecordStore
{
    Task<Usuario?> FindByLoginAsync(string login, CancellationToken cancellationToken = default);
    Task CreateAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
