using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.Abstractions.Gateways;

namespace Infrastructure.Http;

public sealed class AdministrativoClienteGateway(HttpClient httpClient) : IAdministrativoClienteGateway
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<AdministrativoClienteLookupResult> ConsultarPorDocumentoAsync(
        string documento,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync(
            $"system/administrativo/clientes/por-documento/{Uri.EscapeDataString(documento)}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return new AdministrativoClienteLookupResult(
                AdministrativoClienteLookupStatus.Found,
                []);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new AdministrativoClienteLookupResult(
                AdministrativoClienteLookupStatus.NotFound,
                []);
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var body = await response.Content.ReadFromJsonAsync<AdministrativoErrorsBody>(
                JsonOptions,
                cancellationToken);
            var errors = body?.Errors ?? ["Documento inválido."];
            return new AdministrativoClienteLookupResult(
                AdministrativoClienteLookupStatus.InvalidDocumento,
                errors);
        }

        response.EnsureSuccessStatusCode();
        return new AdministrativoClienteLookupResult(
            AdministrativoClienteLookupStatus.Found,
            []);
    }

    private sealed class AdministrativoErrorsBody
    {
        public List<string>? Errors { get; init; }
    }
}
