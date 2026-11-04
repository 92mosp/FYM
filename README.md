# FYM Technology user management

A production-oriented implementation of the FYM senior engineering exercise: an ASP.NET Core REST API with JWT, Swagger, SQL Server, users and roles, plus a React client.

## What is included

- ASP.NET Core 8 API with EF Core and SQL Server
- JWT authentication and role-based authorization
- Pre-created `SuperAdmin` from environment configuration
- User creation, user queries, role queries, and role assignment
- React 18 and TypeScript client with TanStack Query
- Unit tests, linting, formatting, coverage, Docker, health checks, and structured errors
- GitHub Actions for CI, CodeQL, dependency review, NuGet and npm audits, Trivy, and Gitleaks
- Architecture, ER model, operations, API, contribution, and security documentation

## Quick start with Docker

1. Copy `.env.example` to `.env` and replace every placeholder.
2. Run `docker compose up --build`.
3. Open `http://localhost:5173`.
4. The API is exposed at `http://localhost:5080`.

Local Docker uses SQL encryption with a trusted development certificate exception. Do not copy `TrustServerCertificate=True` into production.

## Local development

### API

```bash
dotnet restore FYM.sln
dotnet ef database update --project API/FYMTechnology.Api
dotnet run --project API/FYMTechnology.Api
```

### Front end

```bash
cd Front
npm install
npm run dev
```

Create `Front/.env.local` when the API URL differs from the default:

```text
VITE_API_URL=https://localhost:7080/api
```

## Quality checks

```bash
dotnet format FYM.sln --verify-no-changes
dotnet build FYM.sln -c Release
dotnet test FYM.sln -c Release --collect:"XPlat Code Coverage"
cd Front
npm run format:check
npm run lint
npm test
npm run build
```

## Documentation

- [Architecture and ER model](docs/ARCHITECTURE.md)
- [API reference](docs/API.md)
- [Operations](docs/OPERATIONS.md)
- [Security policy](SECURITY.md)
- [Contribution guide](CONTRIBUTING.md)

## Important production notes

Use Azure Key Vault or the deployment platform secret store. Use a valid SQL Server certificate and keep `Encrypt=True`. Replace symmetric local JWT issuance with Microsoft Entra ID, Auth0, or another OpenID Connect provider when identity lifecycle, MFA, password reset, account lockout, and audit requirements exceed this exercise.
