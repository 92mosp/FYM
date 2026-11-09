namespace FYMTechnology.Api.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinimumKeyBytes = 32;
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiryMinutes { get; init; } = 30;
}

public class SeedOptions
{
    public const string SectionName = "Seed";
    public string SuperAdminUsername { get; init; } = string.Empty;
    public string SuperAdminPassword { get; init; } = string.Empty;
}

public class DatabaseOptions
{
    public const string SectionName = "Database";
    public bool ApplyMigrationsOnStartup { get; init; }
}

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";
    public const string LoginPolicy = "login";
    public int GlobalPermitLimit { get; init; } = 100;
    public int LoginPermitLimit { get; init; } = 5;
    public int WindowSeconds { get; init; } = 60;
}
