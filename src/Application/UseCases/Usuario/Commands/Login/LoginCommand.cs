using Application.UseCases.Usuario.Responses;
using MediatR;

namespace Application.UseCases.Usuario.Commands.Login;

public class LoginCommand : IRequest<LoginResponse>
{
    public string Login { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}
