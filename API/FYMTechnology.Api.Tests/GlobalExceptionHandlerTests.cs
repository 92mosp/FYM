using System.Text.Json;
using FluentAssertions;
using FYMTechnology.Api.Middleware;
using FYMTechnology.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FYMTechnology.Api.Tests;

public class GlobalExceptionHandlerTests
{
    [Theory]
    [InlineData(typeof(ConflictException), 409)]
    [InlineData(typeof(ValidationException), 400)]
    public async Task MapsDomainExceptionsToStatusAndMessage(Type exceptionType, int expectedStatus)
    {
        var exception = (Exception)Activator.CreateInstance(exceptionType, "Domain message")!;

        var (status, body) = await HandleAsync(exception);

        status.Should().Be(expectedStatus);
        body.GetProperty("status").GetInt32().Should().Be(expectedStatus);
        body.GetProperty("title").GetString().Should().Be("Domain message");
        body.GetProperty("instance").GetString().Should().Be("/api/test");
        body.TryGetProperty("traceId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task MapsUnexpectedExceptionsTo500WithoutLeakingMessage()
    {
        var (status, body) = await HandleAsync(new InvalidOperationException("Sensitive connection detail"));

        status.Should().Be(500);
        body.GetProperty("title").GetString().Should().Be("An unexpected error occurred.");
        body.GetRawText().Should().NotContain("Sensitive connection detail");
    }

    private static async Task<(int Status, JsonElement Body)> HandleAsync(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/test";
        context.Response.Body = new MemoryStream();
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        handled.Should().BeTrue();
        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        return (context.Response.StatusCode, document.RootElement.Clone());
    }
}
