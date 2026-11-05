using Domain.Entities;
using Google.Cloud.Firestore;

namespace Infrastructure.Firestore;

public sealed class FirestoreUsuarioRecordStore(FirestoreDb firestoreDb) : IUsuarioRecordStore
{
    private const string CollectionName = "usuarios";

    public async Task<Usuario?> FindByLoginAsync(string login, CancellationToken cancellationToken = default)
    {
        var snapshot = await firestoreDb
            .Collection(CollectionName)
            .Document(login.ToLowerInvariant())
            .GetSnapshotAsync(cancellationToken);

        if (!snapshot.Exists)
        {
            return null;
        }

        var tipo = Enum.Parse<TipoUsuario>(snapshot.GetValue<string>("tipoUsuario"));
        return new Usuario
        {
            Id = snapshot.GetValue<string>("id"),
            Login = snapshot.GetValue<string>("login"),
            SenhaHash = snapshot.GetValue<string>("senhaHash"),
            TipoUsuario = tipo
        };
    }

    public Task CreateAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        var docRef = firestoreDb.Collection(CollectionName).Document(usuario.Login.ToLowerInvariant());
        return docRef.SetAsync(
            new Dictionary<string, object>
            {
                ["id"] = usuario.Id,
                ["login"] = usuario.Login,
                ["senhaHash"] = usuario.SenhaHash,
                ["tipoUsuario"] = usuario.TipoUsuario.ToString()
            },
            cancellationToken: cancellationToken);
    }
}
