namespace Application.Abstractions.Services;

public interface IPasswordHasher
{
    string HashPassword(string password);
}
