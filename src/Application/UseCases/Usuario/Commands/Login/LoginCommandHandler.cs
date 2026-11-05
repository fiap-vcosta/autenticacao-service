using Application.Abstractions.Gateways;
using Application.Abstractions.Services;
using Application.UseCases.Usuario.Responses;
using MediatR;

namespace Application.UseCases.Usuario.Commands.Login;

public class LoginCommandHandler(
    IUsuarioGateway usuarioGateway,
    IPasswordHasher passwordHasher,
    IJwtIssuer jwtIssuer) : IRequestHandler<LoginCommand, LoginResponse>
{
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var usuario = await usuarioGateway.GetByLoginAsync(request.Login, cancellationToken);
        var senhaHash = passwordHasher.HashPassword(request.Senha);

        if (usuario is null ||
            !string.Equals(usuario.SenhaHash, senhaHash, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Login ou senha inválidos.");
        }

        var token = jwtIssuer.IssueStaffToken(
            usuario.Login,
            usuario.TipoUsuario.ToString(),
            usuario.Id);

        return new LoginResponse { Token = token };
    }
}
