namespace Application.Abstractions.Gateways;

public interface IAdministrativoClienteGateway
{
    Task<AdministrativoClienteLookupResult> ConsultarPorDocumentoAsync(
        string documento,
        CancellationToken cancellationToken = default);
}

public enum AdministrativoClienteLookupStatus
{
    Found,
    NotFound,
    InvalidDocumento
}

public sealed record AdministrativoClienteLookupResult(
    AdministrativoClienteLookupStatus Status,
    IReadOnlyList<string> Errors);
