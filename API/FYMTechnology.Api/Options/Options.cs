namespace FYMTechnology.Api.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public required string Key { get; init; }
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public int ExpiryMinutes { get; init; } = 30;
}

public class SeedOptions
{
    public const string SectionName = "Seed";
    public required string SuperAdminUsername { get; init; }
    public required string SuperAdminPassword { get; init; }
}
