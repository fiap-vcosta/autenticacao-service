using Api.Controllers.Clientes;
using Api.Presenters;
using Application.UseCases.Cliente.Commands.EmitirTokenCliente;
using Application.UseCases.Cliente.Responses;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Api.Controllers.Clientes;

public class ClientesControllerTests
{
    [Fact]
    public async Task EmitirToken_DocumentoInvalido_ReturnsErrorsPayload()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<EmitirTokenClienteCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DocumentoInvalidoException(["Documento inválido."]));
        var controller = new ClientesController(
            mediator.Object,
            new TokenPresenter(),
            new EmitirTokenClienteRequestValidator());

        // Act
        var result = await controller.EmitirToken(
            new EmitirTokenClienteRequest { Documento = "x" },
            CancellationToken.None);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task EmitirToken_Valid_ReturnsOk()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<EmitirTokenClienteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmitirTokenClienteResponse { Token = "tok" });
        var controller = new ClientesController(
            mediator.Object,
            new TokenPresenter(),
            new EmitirTokenClienteRequestValidator());

        // Act
        var result = await controller.EmitirToken(
            new EmitirTokenClienteRequest { Documento = "39053344705" },
            CancellationToken.None);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
