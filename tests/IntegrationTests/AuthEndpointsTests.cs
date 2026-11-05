using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace IntegrationTests;

[Collection(nameof(IntegrationFixture))]
public class AuthEndpointsTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task Health_ReturnsOk()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/autenticacao/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Jwks_ReturnsRsaKey()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/autenticacao/.well-known/jwks.json");
        var json = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("RSA", doc.RootElement.GetProperty("keys")[0].GetProperty("kty").GetString());
    }

    [Fact]
    public async Task Login_Admin_ReturnsToken()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/autenticacao/login", new { login = "admin", senha = "admin" });
        var body = await response.Content.ReadFromJsonAsync<TokenBody>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(body?.Token));
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/autenticacao/login", new { login = "admin", senha = "x" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ClientesToken_WhenClienteExists_ReturnsToken()
    {
        // Arrange
        factory.Administrativo.DocumentosExistentes.Add("39053344705");
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/autenticacao/clientes/token",
            new { documento = "39053344705" });
        var body = await response.Content.ReadFromJsonAsync<TokenBody>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(body?.Token));
    }

    [Fact]
    public async Task ClientesToken_WhenMissing_ReturnsNotFound()
    {
        // Arrange
        var client = factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync(
            "/autenticacao/clientes/token",
            new { documento = "39053344700" });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private sealed class TokenBody
    {
        public string? Token { get; init; }
    }
}
