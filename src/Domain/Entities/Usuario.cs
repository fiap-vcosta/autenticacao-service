namespace Domain.Entities;

public class Usuario
{
    public required string Id { get; init; }
    public required string Login { get; init; }
    public required string SenhaHash { get; init; }
    public required TipoUsuario TipoUsuario { get; init; }
}
