namespace Application.UseCases.Cliente.Responses;

public record EmitirTokenClienteResponse
{
    public required string Token { get; init; }
}
