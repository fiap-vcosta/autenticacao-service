using Infrastructure.Services;

namespace UnitTests.Infrastructure.Services;

public class PasswordHasherTests
{
    [Fact]
    public void HashPassword_Admin_MatchesLegacySha256Hex()
    {
        // Arrange
        var hasher = new PasswordHasher();

        // Act
        var hash = hasher.HashPassword("admin");

        // Assert
        Assert.Equal("8c6976e5b5410415bde908bd4dee15dfb167a9c873fc4bb8a81f6f2ab448a918", hash);
    }
}
