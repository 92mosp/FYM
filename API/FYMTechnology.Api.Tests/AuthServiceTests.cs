using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using FYMTechnology.Api.Contracts;
using FYMTechnology.Api.Options;
using FYMTechnology.Api.Services;
using Xunit;

namespace FYMTechnology.Api.Tests;

public sealed class AuthServiceTests : IDisposable
{
    private readonly TestDatabase database = new();
    private readonly JwtOptions jwtOptions = new()
    {
        Key = "unit-test-signing-key-with-at-least-32-bytes",
        Issuer = "test-issuer",
        Audience = "test-audience",
        ExpiryMinutes = 20
    };

    [Fact]
    public async Task LoginAsyncReturnsTokenForValidCredentials()
    {
        var (superAdmin, user) = await database.SeedRolesAsync();
        await database.SeedUserAsync("Michael", "Correct-Password-1", superAdmin, user);

        var result = await CreateService().LoginAsync(new LoginRequest("  michael ", "Correct-Password-1"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.User.Username.Should().Be("Michael");
        result.User.Roles.Should().Equal("SuperAdmin", "User");
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken);
        token.Issuer.Should().Be("test-issuer");
        token.Audiences.Should().ContainSingle().Which.Should().Be("test-audience");
        token.Claims.Where(x => x.Type is "role" or System.Security.Claims.ClaimTypes.Role).Select(x => x.Value).Should().BeEquivalentTo("SuperAdmin", "User");
        token.Claims.Should().Contain(x => x.Type == JwtRegisteredClaimNames.Sub && x.Value == result.User.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task LoginAsyncSetsExpirationFromOptions()
    {
        var (_, user) = await database.SeedRolesAsync();
        await database.SeedUserAsync("michael", "Correct-Password-1", user);
        var before = DateTimeOffset.UtcNow;

        var result = await CreateService().LoginAsync(new LoginRequest("michael", "Correct-Password-1"), CancellationToken.None);

        result!.ExpiresAtUtc.Should().BeCloseTo(before.AddMinutes(20), TimeSpan.FromSeconds(5));
        new JwtSecurityTokenHandler().ReadJwtToken(result.AccessToken).ValidTo
            .Should().BeCloseTo(result.ExpiresAtUtc.UtcDateTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task LoginAsyncReturnsNullForWrongPassword()
    {
        var (_, user) = await database.SeedRolesAsync();
        await database.SeedUserAsync("michael", "Correct-Password-1", user);

        var result = await CreateService().LoginAsync(new LoginRequest("michael", "Wrong-Password-1"), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsyncReturnsNullForUnknownUser()
    {
        var result = await CreateService().LoginAsync(new LoginRequest("ghost", "Any-Password-1"), CancellationToken.None);

        result.Should().BeNull();
    }

    private AuthService CreateService() => new(database.CreateContext(), Microsoft.Extensions.Options.Options.Create(jwtOptions));

    public void Dispose() => database.Dispose();
}
