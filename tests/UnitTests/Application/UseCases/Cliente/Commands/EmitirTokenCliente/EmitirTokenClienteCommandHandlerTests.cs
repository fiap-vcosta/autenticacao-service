using Application.Abstractions.Gateways;
using Application.Abstractions.Services;
using Application.UseCases.Cliente.Commands.EmitirTokenCliente;
using Domain.Exceptions;
using Moq;

namespace UnitTests.Application.UseCases.Cliente.Commands.EmitirTokenCliente;

public class EmitirTokenClienteCommandHandlerTests
{
    [Fact]
    public async Task Handle_ClienteFound_ReturnsToken()
    {
        // Arrange
        var adm = new Mock<IAdministrativoClienteGateway>();
        var jwt = new Mock<IJwtIssuer>();
        adm.Setup(a => a.ConsultarPorDocumentoAsync("39053344705", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdministrativoClienteLookupResult(AdministrativoClienteLookupStatus.Found, []));
        jwt.Setup(j => j.IssueClienteToken("39053344705")).Returns("cliente-jwt");

        var handler = new EmitirTokenClienteCommandHandler(adm.Object, jwt.Object);

        // Act
        var result = await handler.Handle(
            new EmitirTokenClienteCommand { Documento = "39053344705" },
            CancellationToken.None);

        // Assert
        Assert.Equal("cliente-jwt", result.Token);
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsDomainNotFound()
    {
        // Arrange
        var adm = new Mock<IAdministrativoClienteGateway>();
        var jwt = new Mock<IJwtIssuer>();
        adm.Setup(a => a.ConsultarPorDocumentoAsync("39053344705", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdministrativoClienteLookupResult(AdministrativoClienteLookupStatus.NotFound, []));

        var handler = new EmitirTokenClienteCommandHandler(adm.Object, jwt.Object);

        // Act / Assert
        await Assert.ThrowsAsync<DomainNotFoundException>(() =>
            handler.Handle(new EmitirTokenClienteCommand { Documento = "39053344705" }, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InvalidDocumento_ThrowsDocumentoInvalido()
    {
        // Arrange
        var adm = new Mock<IAdministrativoClienteGateway>();
        var jwt = new Mock<IJwtIssuer>();
        adm.Setup(a => a.ConsultarPorDocumentoAsync("x", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AdministrativoClienteLookupResult(
                AdministrativoClienteLookupStatus.InvalidDocumento,
                ["Documento inválido."]));

        var handler = new EmitirTokenClienteCommandHandler(adm.Object, jwt.Object);

        // Act
        var ex = await Assert.ThrowsAsync<DocumentoInvalidoException>(() =>
            handler.Handle(new EmitirTokenClienteCommand { Documento = "x" }, CancellationToken.None));

        // Assert
        Assert.Contains("Documento inválido.", ex.Errors);
    }
}
