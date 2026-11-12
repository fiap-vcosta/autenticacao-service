using Domain.Entities;
using Infrastructure.Firestore;
using Infrastructure.Services;
using Moq;

namespace UnitTests.Infrastructure.Firestore;

public class FirestoreUsuarioGatewayTests
{
    [Fact]
    public async Task EnsureAdminSeed_WhenMissing_CreatesAdmin()
    {
        // Arrange
        var store = new Mock<IUsuarioRecordStore>();
        store.Setup(s => s.FindByLoginAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);
        var hasher = new PasswordHasher();
        var gateway = new FirestoreUsuarioGateway(store.Object, hasher);

        // Act
        await gateway.EnsureAdminSeedAsync();

        // Assert
        store.Verify(s => s.CreateAsync(
            It.Is<Usuario>(u =>
                u.Login == "admin" &&
                u.SenhaHash == hasher.HashPassword("admin") &&
                u.TipoUsuario == TipoUsuario.Admin),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnsureAdminSeed_WhenExists_DoesNotCreate()
    {
        // Arrange
        var store = new Mock<IUsuarioRecordStore>();
        store.Setup(s => s.FindByLoginAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Usuario
            {
                Id = "1",
                Login = "admin",
                SenhaHash = "x",
                TipoUsuario = TipoUsuario.Admin
            });
        var gateway = new FirestoreUsuarioGateway(store.Object, new PasswordHasher());

        // Act
        await gateway.EnsureAdminSeedAsync();

        // Assert
        store.Verify(s => s.CreateAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByLogin_DelegatesToStore()
    {
        // Arrange
        var expected = new Usuario
        {
            Id = "1",
            Login = "admin",
            SenhaHash = "h",
            TipoUsuario = TipoUsuario.Admin
        };
        var store = new Mock<IUsuarioRecordStore>();
        store.Setup(s => s.FindByLoginAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var gateway = new FirestoreUsuarioGateway(store.Object, new PasswordHasher());

        // Act
        var result = await gateway.GetByLoginAsync("admin");

        // Assert
        Assert.Same(expected, result);
    }
}
