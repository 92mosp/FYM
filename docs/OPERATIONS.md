# Operations

## Configuration

Use environment variables with ASP.NET Core double-underscore notation:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Seed__SuperAdminUsername`
- `Seed__SuperAdminPassword`
- `Cors__AllowedOrigins__0`

The JWT key must contain at least 32 bytes. Rotate it through the target platform secret store. Changing a symmetric signing key invalidates existing tokens.

## Database

The exercise uses `EnsureCreatedAsync` to keep first-run setup simple. Before evolving the schema in production, introduce EF Core migrations and execute them in a controlled deployment job rather than granting schema-change permissions to the runtime identity.

## Branch protection

Require pull requests, at least one approval, conversation resolution, signed commits where available, and the following checks: API CI, front-end CI, CodeQL, dependency review, Trivy, and Gitleaks. Enable GitHub secret scanning with push protection and private vulnerability reporting in repository settings.
