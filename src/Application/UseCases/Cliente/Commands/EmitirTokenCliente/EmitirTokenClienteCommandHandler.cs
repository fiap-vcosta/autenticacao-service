using Application.Abstractions.Gateways;
using Application.Abstractions.Services;
using Application.UseCases.Cliente.Responses;
using Domain.Exceptions;
using MediatR;

namespace Application.UseCases.Cliente.Commands.EmitirTokenCliente;

public class EmitirTokenClienteCommandHandler(
    IAdministrativoClienteGateway administrativoClienteGateway,
    IJwtIssuer jwtIssuer) : IRequestHandler<EmitirTokenClienteCommand, EmitirTokenClienteResponse>
{
    public async Task<EmitirTokenClienteResponse> Handle(
        EmitirTokenClienteCommand request,
        CancellationToken cancellationToken)
    {
        var lookup = await administrativoClienteGateway.ConsultarPorDocumentoAsync(
            request.Documento,
            cancellationToken);

        if (lookup.Status == AdministrativoClienteLookupStatus.InvalidDocumento)
        {
            throw new DocumentoInvalidoException(lookup.Errors);
        }

        if (lookup.Status == AdministrativoClienteLookupStatus.NotFound)
        {
            throw new DomainNotFoundException("Cliente não encontrado.");
        }

        var token = jwtIssuer.IssueClienteToken(request.Documento);
        return new EmitirTokenClienteResponse { Token = token };
    }
}
