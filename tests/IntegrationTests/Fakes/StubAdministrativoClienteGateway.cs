using Application.Abstractions.Gateways;

namespace IntegrationTests.Fakes;

public sealed class StubAdministrativoClienteGateway : IAdministrativoClienteGateway
{
    public HashSet<string> DocumentosExistentes { get; } = new(StringComparer.Ordinal);
    public HashSet<string> DocumentosInvalidos { get; } = new(StringComparer.Ordinal);

    public Task<AdministrativoClienteLookupResult> ConsultarPorDocumentoAsync(
        string documento,
        CancellationToken cancellationToken = default)
    {
        if (DocumentosInvalidos.Contains(documento))
        {
            return Task.FromResult(new AdministrativoClienteLookupResult(
                AdministrativoClienteLookupStatus.InvalidDocumento,
                ["Documento inválido."]));
        }

        if (DocumentosExistentes.Contains(documento))
        {
            return Task.FromResult(new AdministrativoClienteLookupResult(
                AdministrativoClienteLookupStatus.Found,
                []));
        }

        return Task.FromResult(new AdministrativoClienteLookupResult(
            AdministrativoClienteLookupStatus.NotFound,
            []));
    }
}
