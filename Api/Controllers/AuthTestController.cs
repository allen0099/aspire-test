using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthTestController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public IActionResult Get()
    {
        var userName = User.Identity?.Name;
        var claims = User.Claims.Select(c => new { c.Type, c.Value });
        return Ok(new { Message = "已通過 Keycloak 驗證", UserName = userName, Claims = claims });
    }
}