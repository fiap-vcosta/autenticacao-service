namespace Application.UseCases.Usuario.Responses;

public record LoginResponse
{
    public required string Token { get; init; }
}
