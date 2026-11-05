using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Services;

namespace Infrastructure.Services;

public sealed class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = SHA256.HashData(bytes);
        return string.Concat(hashBytes.Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
    }
}
