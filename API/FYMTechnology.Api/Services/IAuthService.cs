using FYMTechnology.Api.Contracts;

namespace FYMTechnology.Api.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
