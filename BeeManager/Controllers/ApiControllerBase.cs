using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BeeManager.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

    protected string[] CurrentRoles =>
        User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();
}
