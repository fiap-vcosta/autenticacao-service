using Application.Abstractions.Gateways;
using Application.Abstractions.Services;
using Application.UseCases.Usuario.Commands.Login;
using Domain.Entities;
using Moq;

namespace UnitTests.Application.UseCases.Usuario.Commands.Login;

public class LoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var gateway = new Mock<IUsuarioGateway>();
        var hasher = new Mock<IPasswordHasher>();
        var jwt = new Mock<IJwtIssuer>();

        gateway.Setup(g => g.GetByLoginAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Usuario
            {
                Id = "1",
                Login = "admin",
                SenhaHash = "hash",
                TipoUsuario = TipoUsuario.Admin
            });
        hasher.Setup(h => h.HashPassword("admin")).Returns("hash");
        jwt.Setup(j => j.IssueStaffToken("admin", "Admin", "1")).Returns("jwt-token");

        var handler = new LoginCommandHandler(gateway.Object, hasher.Object, jwt.Object);

        // Act
        var result = await handler.Handle(new LoginCommand { Login = "admin", Senha = "admin" }, CancellationToken.None);

        // Assert
        Assert.Equal("jwt-token", result.Token);
    }

    [Fact]
    public async Task Handle_InvalidPassword_ThrowsUnauthorized()
    {
        // Arrange
        var gateway = new Mock<IUsuarioGateway>();
        var hasher = new Mock<IPasswordHasher>();
        var jwt = new Mock<IJwtIssuer>();

        gateway.Setup(g => g.GetByLoginAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Domain.Entities.Usuario
            {
                Id = "1",
                Login = "admin",
                SenhaHash = "hash",
                TipoUsuario = TipoUsuario.Admin
            });
        hasher.Setup(h => h.HashPassword("wrong")).Returns("other");

        var handler = new LoginCommandHandler(gateway.Object, hasher.Object, jwt.Object);

        // Act / Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new LoginCommand { Login = "admin", Senha = "wrong" }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UnknownUser_ThrowsUnauthorized()
    {
        // Arrange
        var gateway = new Mock<IUsuarioGateway>();
        var hasher = new Mock<IPasswordHasher>();
        var jwt = new Mock<IJwtIssuer>();
        gateway.Setup(g => g.GetByLoginAsync("ghost", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Usuario?)null);
        hasher.Setup(h => h.HashPassword("x")).Returns("hash");

        var handler = new LoginCommandHandler(gateway.Object, hasher.Object, jwt.Object);

        // Act / Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new LoginCommand { Login = "ghost", Senha = "x" }, CancellationToken.None));
    }
}
