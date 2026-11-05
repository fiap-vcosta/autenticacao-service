using Api.Contracts.Validation;
using Api.Presenters;
using Application.UseCases.Usuario.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Login;

[ApiController]
[Route("autenticacao")]
public class LoginController(
    IMediator mediator,
    TokenPresenter presenter,
    IValidator<LoginRequest> validator) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors });
        }

        var response = await mediator.Send(
            new LoginCommand { Login = request.Login, Senha = request.Senha },
            cancellationToken);

        return Ok(presenter.Present(response));
    }
}
