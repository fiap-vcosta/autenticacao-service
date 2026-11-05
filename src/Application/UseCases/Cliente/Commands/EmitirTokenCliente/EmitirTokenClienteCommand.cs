using Application.UseCases.Cliente.Responses;
using MediatR;

namespace Application.UseCases.Cliente.Commands.EmitirTokenCliente;

public class EmitirTokenClienteCommand : IRequest<EmitirTokenClienteResponse>
{
    public string Documento { get; init; } = string.Empty;
}
