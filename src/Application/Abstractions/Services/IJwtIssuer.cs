namespace Application.Abstractions.Services;

public interface IJwtIssuer
{
    string IssueStaffToken(string login, string papel, string userId);
    string IssueClienteToken(string documento);
    object GetJwks();
}
