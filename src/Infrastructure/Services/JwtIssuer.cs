using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Application.Abstractions.Services;
using BuildingBlocks.Http.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services;

public sealed class JwtIssuer : IJwtIssuer, IDisposable
{
    private readonly JwtIssuerOptions _options;
    private readonly RSA _rsa;
    private readonly SigningCredentials _signingCredentials;

    public JwtIssuer(IOptions<JwtIssuerOptions> options)
    {
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.SigningKeyPem))
        {
            throw new InvalidOperationException("Jwt:SigningKeyPem é obrigatório.");
        }

        _rsa = RSA.Create();
        var pem = _options.SigningKeyPem
            .Replace("\\r\\n", "\n", StringComparison.Ordinal)
            .Replace("\\n", "\n", StringComparison.Ordinal);
        _rsa.ImportFromPem(pem);
        _signingCredentials = new SigningCredentials(
            new RsaSecurityKey(_rsa) { KeyId = _options.KeyId },
            SecurityAlgorithms.RsaSha256);
    }

    public string IssueStaffToken(string login, string papel, string userId)
    {
        var claims = new Claim[]
        {
            new(ClaimTypes.Name, login),
            new(ClaimTypes.Role, papel),
            new("userId", userId)
        };

        return CreateToken(
            claims,
            _options.Funcionario.Issuer,
            _options.Funcionario.Audience,
            TimeSpan.FromMinutes(30));
    }

    public string IssueClienteToken(string documento)
    {
        var claims = new Claim[]
        {
            new(ClienteJwtClaims.Documento, documento)
        };

        return CreateToken(
            claims,
            _options.Cliente.Issuer,
            _options.Cliente.Audience,
            TimeSpan.FromSeconds(1800));
    }

    public object GetJwks()
    {
        var parameters = _rsa.ExportParameters(false);
        var jwk = new JsonWebKey
        {
            Kty = "RSA",
            Use = "sig",
            Kid = _options.KeyId,
            Alg = SecurityAlgorithms.RsaSha256,
            N = Base64UrlEncoder.Encode(parameters.Modulus!),
            E = Base64UrlEncoder.Encode(parameters.Exponent!)
        };

        return new { keys = new[] { jwk } };
    }

    public void Dispose() => _rsa.Dispose();

    private string CreateToken(
        IEnumerable<Claim> claims,
        string issuer,
        string audience,
        TimeSpan lifetime)
    {
        var handler = new JwtSecurityTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(lifetime),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = _signingCredentials
        };

        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}
