using System.Text.Json.Serialization;
using Api.Contracts.Validation;
using Api.Controllers.Clientes;
using Api.Controllers.Login;
using Api.Presenters;
using BuildingBlocks.Http.ProblemDetails;
using Domain.Exceptions;
using Infrastructure.DependencyInjection;

namespace Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddScoped<TokenPresenter>();
        services.AddSingleton<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddSingleton<IValidator<EmitirTokenClienteRequest>, EmitirTokenClienteRequestValidator>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(Application.UseCases.Usuario.Commands.Login.LoginCommand).Assembly));

        services.AddBuildingBlocksProblemDetails(options =>
        {
            options.Map<DomainNotFoundException>(StatusCodes.Status404NotFound, "Not Found");
            options.Map<KeyNotFoundException>(StatusCodes.Status404NotFound, "Not Found");
            options.Map<DocumentoInvalidoException>(StatusCodes.Status400BadRequest, "Bad Request");
            options.Map<BusinessRuleException>(StatusCodes.Status400BadRequest, "Business Rule Violation");
            options.Map<ArgumentException>(StatusCodes.Status400BadRequest, "Bad Request");
            options.Map<UnauthorizedAccessException>(StatusCodes.Status401Unauthorized, "Unauthorized");
        });

        services.AddControllers(options =>
            {
                options.Filters.Add<ProblemDetailsExceptionFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
    }
}
