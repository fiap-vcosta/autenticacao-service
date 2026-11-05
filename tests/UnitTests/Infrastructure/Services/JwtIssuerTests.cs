using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using BuildingBlocks.Http.Auth;
using Infrastructure.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace UnitTests.Infrastructure.Services;

public class JwtIssuerTests
{
    [Fact]
    public void IssueStaffToken_ContainsExpectedClaimsAndAudience()
    {
        // Arrange
        var issuer = CreateIssuer(out var publicKey);

        // Act
        var token = issuer.IssueStaffToken("admin", "Admin", "user-1");

        // Assert
        var principal = Validate(token, publicKey, "autenticacao-service", "funcionario");
        Assert.Equal("admin", principal.FindFirst(ClaimTypes.Name)?.Value);
        Assert.Equal("Admin", principal.FindFirst(ClaimTypes.Role)?.Value);
        Assert.Equal("user-1", principal.FindFirst("userId")?.Value);
    }

    [Fact]
    public void IssueClienteToken_ContainsDocumentoClaim()
    {
        // Arrange
        var issuer = CreateIssuer(out var publicKey);

        // Act
        var token = issuer.IssueClienteToken("39053344705");

        // Assert
        var principal = Validate(token, publicKey, "autenticacao-service", "cliente");
        Assert.Equal("39053344705", principal.FindFirst(ClienteJwtClaims.Documento)?.Value);
    }

    [Fact]
    public void GetJwks_ReturnsRsaKeyWithKid()
    {
        // Arrange
        var issuer = CreateIssuer(out _);

        // Act
        var jwks = issuer.GetJwks();
        var json = JsonSerializer.Serialize(jwks);
        using var doc = JsonDocument.Parse(json);

        // Assert
        var key = doc.RootElement.GetProperty("keys")[0];
        Assert.Equal("RSA", key.GetProperty("kty").GetString());
        Assert.Equal("autenticacao-2026", key.GetProperty("kid").GetString());
        Assert.False(string.IsNullOrWhiteSpace(key.GetProperty("n").GetString()));
    }

    [Fact]
    public void Constructor_MissingSigningKey_Throws()
    {
        // Arrange / Act / Assert
        Assert.Throws<InvalidOperationException>(() =>
            new JwtIssuer(Options.Create(new JwtIssuerOptions())));
    }

    [Fact]
    public void Constructor_AcceptsEscapedNewlinesInPem()
    {
        // Arrange
        using var rsa = RSA.Create(2048);
        var escaped = rsa.ExportRSAPrivateKeyPem().Replace("\n", "\\n", StringComparison.Ordinal);

        // Act
        using var issuer = new JwtIssuer(Options.Create(new JwtIssuerOptions
        {
            SigningKeyPem = escaped,
            KeyId = "autenticacao-2026",
            Funcionario = new JwtAudienceOptions
            {
                Issuer = "autenticacao-service",
                Audience = "funcionario"
            },
            Cliente = new JwtAudienceOptions
            {
                Issuer = "autenticacao-service",
                Audience = "cliente"
            }
        }));

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(issuer.IssueStaffToken("admin", "Admin", "1")));
    }

    private static JwtIssuer CreateIssuer(out RsaSecurityKey publicKey)
    {
        using var rsa = RSA.Create(2048);
        var privatePem = rsa.ExportRSAPrivateKeyPem();
        publicKey = new RsaSecurityKey(RSA.Create(rsa.ExportParameters(false)))
        {
            KeyId = "autenticacao-2026"
        };

        return new JwtIssuer(Options.Create(new JwtIssuerOptions
        {
            SigningKeyPem = privatePem,
            KeyId = "autenticacao-2026",
            Funcionario = new JwtAudienceOptions
            {
                Issuer = "autenticacao-service",
                Audience = "funcionario"
            },
            Cliente = new JwtAudienceOptions
            {
                Issuer = "autenticacao-service",
                Audience = "cliente"
            }
        }));
    }

    private static ClaimsPrincipal Validate(
        string token,
        RsaSecurityKey publicKey,
        string issuer,
        string audience)
    {
        var handler = new JwtSecurityTokenHandler();
        return handler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = publicKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        }, out _);
    }
}
