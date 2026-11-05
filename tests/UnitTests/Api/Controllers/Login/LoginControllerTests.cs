using Api.Contracts.Validation;
using Api.Controllers.Login;
using Api.Presenters;
using Application.UseCases.Usuario.Commands.Login;
using Application.UseCases.Usuario.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Api.Controllers.Login;

public class LoginControllerTests
{
    [Fact]
    public async Task Login_InvalidRequest_ReturnsBadRequestWithErrors()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        var validator = new LoginRequestValidator();
        var controller = new LoginController(mediator.Object, new TokenPresenter(), validator);

        // Act
        var result = await controller.Login(new LoginRequest(), CancellationToken.None);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequest.Value);
    }

    [Fact]
    public async Task Login_ValidRequest_ReturnsOkWithToken()
    {
        // Arrange
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoginResponse { Token = "abc" });
        var controller = new LoginController(mediator.Object, new TokenPresenter(), new LoginRequestValidator());

        // Act
        var result = await controller.Login(
            new LoginRequest { Login = "admin", Senha = "admin" },
            CancellationToken.None);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }
}
