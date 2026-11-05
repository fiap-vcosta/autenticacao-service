using Application.Abstractions.Services;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Jwks;

[ApiController]
[Route("autenticacao/.well-known")]
public class JwksController(IJwtIssuer jwtIssuer) : ControllerBase
{
    [HttpGet("jwks.json")]
    public IActionResult GetJwks() => Ok(jwtIssuer.GetJwks());
}
