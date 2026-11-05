using Api.Contracts.Validation;

namespace Api.Controllers.Clientes;

public class EmitirTokenClienteRequestValidator : IValidator<EmitirTokenClienteRequest>
{
    public ValidationResult Validate(EmitirTokenClienteRequest request)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.Documento))
        {
            result.Errors.Add("Documento é obrigatório.");
        }

        return result;
    }
}
