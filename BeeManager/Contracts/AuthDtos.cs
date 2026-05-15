using System.ComponentModel.DataAnnotations;
using BeeManager.Models;

namespace BeeManager.Contracts;

public class RegisterRequest
{
    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(5)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public RequestedRole RequestedRole { get; set; }

    [StringLength(500)]
    public string? RegistrationNote { get; set; }
}

public class LoginRequest
{
    [Required]
    public string Login { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime AccessTokenExpiresAtUtc { get; set; }

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiresAtUtc { get; set; }

    public CurrentUserDto User { get; set; } = new();
}

public class CurrentUserDto
{
    public string Id { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public AccountStatus AccountStatus { get; set; }

    public RequestedRole RequestedRole { get; set; }

    public List<string> Roles { get; set; } = new();
}
