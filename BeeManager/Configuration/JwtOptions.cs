namespace BeeManager.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "BeeManager";

    public string Audience { get; set; } = "BeeManagerFrontend";

    public string Key { get; set; } = "super-secret-key-change-me-please-123456789";

    public int AccessTokenMinutes { get; set; } = 30;

    public int RefreshTokenDays { get; set; } = 14;
}
