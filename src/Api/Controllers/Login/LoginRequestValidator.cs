using Api.Contracts.Validation;

namespace Api.Controllers.Login;

public class LoginRequestValidator : IValidator<LoginRequest>
{
    public ValidationResult Validate(LoginRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.Login))
        {
            result.Errors.Add("Login não pode estar vazio.");
        }

        if (string.IsNullOrWhiteSpace(request.Senha))
        {
            result.Errors.Add("Senha não pode estar vazia.");
        }

        return result;
    }
}
