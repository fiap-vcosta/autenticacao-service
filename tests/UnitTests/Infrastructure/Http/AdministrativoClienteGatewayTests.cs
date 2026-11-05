using System.Net;
using System.Text;
using Application.Abstractions.Gateways;
using Infrastructure.Http;

namespace UnitTests.Infrastructure.Http;

public class AdministrativoClienteGatewayTests
{
    [Fact]
    public async Task ConsultarPorDocumentoAsync_Ok_ReturnsFound()
    {
        // Arrange
        var gateway = CreateGateway(HttpStatusCode.OK, "{}");

        // Act
        var result = await gateway.ConsultarPorDocumentoAsync("39053344705");

        // Assert
        Assert.Equal(AdministrativoClienteLookupStatus.Found, result.Status);
    }

    [Fact]
    public async Task ConsultarPorDocumentoAsync_NotFound_ReturnsNotFound()
    {
        // Arrange
        var gateway = CreateGateway(HttpStatusCode.NotFound, string.Empty);

        // Act
        var result = await gateway.ConsultarPorDocumentoAsync("39053344705");

        // Assert
        Assert.Equal(AdministrativoClienteLookupStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task ConsultarPorDocumentoAsync_BadRequest_ReturnsInvalidDocumento()
    {
        // Arrange
        var gateway = CreateGateway(HttpStatusCode.BadRequest, """{"errors":["Documento inválido."]}""");

        // Act
        var result = await gateway.ConsultarPorDocumentoAsync("x");

        // Assert
        Assert.Equal(AdministrativoClienteLookupStatus.InvalidDocumento, result.Status);
        Assert.Contains("Documento inválido.", result.Errors);
    }

    [Fact]
    public async Task ConsultarPorDocumentoAsync_BadRequestWithoutErrors_UsesFallbackMessage()
    {
        // Arrange
        var gateway = CreateGateway(HttpStatusCode.BadRequest, "{}");

        // Act
        var result = await gateway.ConsultarPorDocumentoAsync("x");

        // Assert
        Assert.Equal(AdministrativoClienteLookupStatus.InvalidDocumento, result.Status);
        Assert.Contains("Documento inválido.", result.Errors);
    }

    [Fact]
    public async Task ConsultarPorDocumentoAsync_UnexpectedStatus_Throws()
    {
        // Arrange
        var gateway = CreateGateway(HttpStatusCode.InternalServerError, "err");

        // Act / Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            gateway.ConsultarPorDocumentoAsync("39053344705"));
    }

    private static AdministrativoClienteGateway CreateGateway(HttpStatusCode status, string body)
    {
        var handler = new StubHandler(status, body);
        var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        return new AdministrativoClienteGateway(client);
    }

    private sealed class StubHandler(HttpStatusCode status, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
