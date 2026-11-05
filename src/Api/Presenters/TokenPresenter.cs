using Api.ViewModels;
using Application.UseCases.Cliente.Responses;
using Application.UseCases.Usuario.Responses;

namespace Api.Presenters;

public class TokenPresenter
{
    public TokenViewModel Present(LoginResponse response) => new() { Token = response.Token };

    public TokenViewModel Present(EmitirTokenClienteResponse response) => new() { Token = response.Token };
}
