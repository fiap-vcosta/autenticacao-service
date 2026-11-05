namespace Infrastructure.Services;

public sealed class JwtIssuerOptions
{
    public const string SectionName = "Jwt";

    public string SigningKeyPem { get; set; } = string.Empty;
    public string KeyId { get; set; } = "autenticacao-2026";
    public JwtAudienceOptions Funcionario { get; set; } = new();
    public JwtAudienceOptions Cliente { get; set; } = new();
}

public sealed class JwtAudienceOptions
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
