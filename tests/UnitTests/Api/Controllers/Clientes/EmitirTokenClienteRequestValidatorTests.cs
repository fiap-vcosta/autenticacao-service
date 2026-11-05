using Api.Controllers.Clientes;

namespace UnitTests.Api.Controllers.Clientes;

public class EmitirTokenClienteRequestValidatorTests
{
    [Fact]
    public void Validate_WithDocumento_IsValid()
    {
        // Arrange
        var validator = new EmitirTokenClienteRequestValidator();

        // Act
        var result = validator.Validate(new EmitirTokenClienteRequest { Documento = "39053344705" });

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithoutDocumento_HasError()
    {
        // Arrange
        var validator = new EmitirTokenClienteRequestValidator();

        // Act
        var result = validator.Validate(new EmitirTokenClienteRequest());

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Documento", StringComparison.Ordinal));
    }
}
