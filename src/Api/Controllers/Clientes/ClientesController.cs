using Api.Contracts.Validation;
using Api.Presenters;
using Application.UseCases.Cliente.Commands.EmitirTokenCliente;
using Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Clientes;

[ApiController]
[Route("autenticacao/clientes")]
public class ClientesController(
    IMediator mediator,
    TokenPresenter presenter,
    IValidator<EmitirTokenClienteRequest> validator) : ControllerBase
{
    [HttpPost("token")]
    public async Task<IActionResult> EmitirToken(
        [FromBody] EmitirTokenClienteRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors });
        }

        try
        {
            var response = await mediator.Send(
                new EmitirTokenClienteCommand { Documento = request.Documento },
                cancellationToken);
            return Ok(presenter.Present(response));
        }
        catch (DocumentoInvalidoException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
    }
}
