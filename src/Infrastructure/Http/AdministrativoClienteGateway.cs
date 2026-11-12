using System.Net;
using System.Text.Json;
using Application.Abstractions.Gateways;

namespace Infrastructure.Http;

public sealed class AdministrativoClienteGateway(HttpClient httpClient) : IAdministrativoClienteGateway
{
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
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var errors = ParseErrors(json);
            return new AdministrativoClienteLookupResult(
                AdministrativoClienteLookupStatus.InvalidDocumento,
                errors);
        }

        response.EnsureSuccessStatusCode();
        return new AdministrativoClienteLookupResult(
            AdministrativoClienteLookupStatus.Found,
            []);
    }

    private static List<string> ParseErrors(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return ["Documento inválido."];
        }

        using var document = JsonDocument.Parse(json);
        if (!TryGetErrorsProperty(document.RootElement, out var errorsElement) ||
            errorsElement.ValueKind != JsonValueKind.Array)
        {
            return ["Documento inválido."];
        }

        var errors = errorsElement.EnumerateArray()
            .Select(item => item.GetString())
            .Where(static value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToList();

        return errors.Count > 0 ? errors : ["Documento inválido."];
    }

    private static bool TryGetErrorsProperty(JsonElement root, out JsonElement errorsElement)
    {
        if (root.TryGetProperty("errors", out errorsElement))
        {
            return true;
        }

        return root.TryGetProperty("Errors", out errorsElement);
    }
}
