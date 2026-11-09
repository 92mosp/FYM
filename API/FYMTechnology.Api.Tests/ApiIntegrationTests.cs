using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using FYMTechnology.Api.Contracts;
using Xunit;

namespace FYMTechnology.Api.Tests;

public class ApiIntegrationTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string UserPassword = "User-Password-123!";

    [Fact]
    public async Task HealthReturnsOkAnonymously()
    {
        var response = await factory.CreateClient().GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UsersRequireAuthentication()
    {
        var response = await factory.CreateClient().GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LoginWithWrongPasswordReturnsUnauthorized()
    {
        var response = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest(ApiFactory.AdminUsername, "wrong"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SuperAdminCanCreateListAndAssignRoles()
    {
        var admin = await factory.CreateAuthenticatedClientAsync();
        var roles = await GetRolesAsync(admin);
        var username = UniqueName();

        var created = await admin.PostAsJsonAsync("/api/users", new CreateUserRequest(username, UserPassword, roles.Values.ToArray()));

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        created.Headers.Location.Should().NotBeNull();
        var user = await created.Content.ReadFromJsonAsync<UserResponse>();
        user!.Roles.Should().Equal("SuperAdmin", "User");

        var users = await admin.GetFromJsonAsync<UserResponse[]>("/api/users");
        users!.Should().Contain(x => x.Username == username);

        var assigned = await admin.PutAsJsonAsync($"/api/users/{user.Id}/roles", new AssignRolesRequest([roles["User"]]));

        assigned.StatusCode.Should().Be(HttpStatusCode.OK);
        var reloaded = await admin.GetFromJsonAsync<UserResponse>($"/api/users/{user.Id}");
        reloaded!.Roles.Should().Equal("User");
    }

    [Fact]
    public async Task UserRoleCannotCreateOrAssign()
    {
        var admin = await factory.CreateAuthenticatedClientAsync();
        var roles = await GetRolesAsync(admin);
        var username = UniqueName();
        var created = await admin.PostAsJsonAsync("/api/users", new CreateUserRequest(username, UserPassword, [roles["User"]]));
        var user = await created.Content.ReadFromJsonAsync<UserResponse>();
        var client = await factory.CreateAuthenticatedClientAsync(username, UserPassword);

        var create = await client.PostAsJsonAsync("/api/users", new CreateUserRequest(UniqueName(), UserPassword, [roles["User"]]));
        var assign = await client.PutAsJsonAsync($"/api/users/{user!.Id}/roles", new AssignRolesRequest([roles["SuperAdmin"]]));
        var list = await client.GetAsync("/api/users");

        create.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        assign.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        list.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task InvalidInputReturnsValidationProblemDetails()
    {
        var admin = await factory.CreateAuthenticatedClientAsync();

        var response = await admin.PostAsJsonAsync("/api/users", new CreateUserRequest(new string('a', 101), "short", []));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = body.RootElement.GetProperty("errors");
        errors.TryGetProperty("Username", out _).Should().BeTrue();
        errors.TryGetProperty("Password", out _).Should().BeTrue();
        errors.TryGetProperty("RoleIds", out _).Should().BeTrue();
    }

    [Fact]
    public async Task SuperAdminCannotRemoveOwnSuperAdminRole()
    {
        var admin = await factory.CreateAuthenticatedClientAsync();
        var roles = await GetRolesAsync(admin);
        var users = await admin.GetFromJsonAsync<UserResponse[]>("/api/users");
        var self = users!.Single(x => x.Username == ApiFactory.AdminUsername);

        var response = await admin.PutAsJsonAsync($"/api/users/{self.Id}/roles", new AssignRolesRequest([roles["User"]]));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AssignRolesToUnknownUserReturnsNotFound()
    {
        var admin = await factory.CreateAuthenticatedClientAsync();
        var roles = await GetRolesAsync(admin);

        var response = await admin.PutAsJsonAsync("/api/users/99999/roles", new AssignRolesRequest([roles["User"]]));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private static async Task<Dictionary<string, int>> GetRolesAsync(HttpClient client)
    {
        var roles = await client.GetFromJsonAsync<RoleResponse[]>("/api/roles");
        return roles!.ToDictionary(x => x.Name, x => x.Id);
    }

    private static string UniqueName() => $"user{Guid.NewGuid():N}"[..20];
}

public class LoginRateLimitTests(LoginRateLimitedApiFactory factory) : IClassFixture<LoginRateLimitedApiFactory>
{
    [Fact]
    public async Task LoginIsRateLimitedPerClient()
    {
        var client = factory.CreateClient();
        var statuses = new List<HttpStatusCode>();

        for (var attempt = 0; attempt <= LoginRateLimitedApiFactory.Limit; attempt++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("nobody", "wrong"));
            statuses.Add(response.StatusCode);
        }

        statuses.Take(LoginRateLimitedApiFactory.Limit).Should().AllSatisfy(x => x.Should().Be(HttpStatusCode.Unauthorized));
        statuses.Last().Should().Be(HttpStatusCode.TooManyRequests);
        (await client.GetAsync("/health")).StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
