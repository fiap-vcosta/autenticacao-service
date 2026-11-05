using Api.Controllers.Jwks;
using Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace UnitTests.Api.Controllers.Jwks;

public class JwksControllerTests
{
    [Fact]
    public void GetJwks_ReturnsOk()
    {
        // Arrange
        var issuer = new Mock<IJwtIssuer>();
        issuer.Setup(i => i.GetJwks()).Returns(new { keys = Array.Empty<object>() });
        var controller = new JwksController(issuer.Object);

        // Act
        var result = controller.GetJwks();

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }
}
