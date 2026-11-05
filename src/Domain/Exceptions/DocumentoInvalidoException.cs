namespace Domain.Exceptions;

public class DocumentoInvalidoException : BusinessRuleException
{
    public DocumentoInvalidoException(IReadOnlyList<string> errors)
        : base(string.Join(" ", errors))
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}
