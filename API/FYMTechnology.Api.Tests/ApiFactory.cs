using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FYMTechnology.Api.Contracts;
using FYMTechnology.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FYMTechnology.Api.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    public const string AdminUsername = "admin";
    public const string AdminPassword = "Admin-Password-123!";
    private readonly SqliteConnection connection = new("DataSource=:memory:");

    protected virtual int LoginPermitLimit => 1000;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        connection.Open();
        using (var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options))
        {
            db.Database.EnsureCreated();
        }

        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Data Source=unused");
        builder.UseSetting("Jwt:Key", Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
        builder.UseSetting("Seed:SuperAdminUsername", AdminUsername);
        builder.UseSetting("Seed:SuperAdminPassword", AdminPassword);
        builder.UseSetting("RateLimiting:LoginPermitLimit", LoginPermitLimit.ToString(CultureInfo.InvariantCulture));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
        });
    }

    public async Task<HttpClient> CreateAuthenticatedClientAsync(string username = AdminUsername, string password = AdminPassword)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(username, password));
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.AccessToken);
        return client;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            connection.Dispose();
        }
    }
}

public class LoginRateLimitedApiFactory : ApiFactory
{
    public const int Limit = 2;

    protected override int LoginPermitLimit => Limit;
}
