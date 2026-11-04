# Gestión de usuarios - FYM

Test Michael Lugo Ospina para FYM: una REST API en ASP.NET Core con JWT, Swagger, SQL Server, usuarios y roles, además de un cliente en React.

## Qué incluye

- ASP.NET Core 8 API con EF Core y SQL Server
- Autenticación JWT y autorización basada en roles (`role-based authorization`)
- `SuperAdmin` pre-creado desde la configuración del entorno (`environment configuration`)
- Creación de usuarios, consultas de usuarios, consultas de roles y asignación de roles
- Cliente en React 18 y TypeScript con TanStack Query
- Pruebas unitarias (`unit tests`), *linting*, formateo, cobertura (`coverage`), Docker, comprobaciones de estado (`health checks`) y errores estructurados
- GitHub Actions para CI, CodeQL, revisión de dependencias (`dependency review`), auditorías de NuGet y npm, Trivy y Gitleaks
- Documentación de arquitectura, modelo ER, operaciones, API, contribución y seguridad

## Docker

1. Copia `.env.example` a `.env` y reemplaza cada marcador de posición (`placeholder`).
2. Ejecuta `docker compose up --build`.
3. Abre `http://localhost:5173`.
4. La API está expuesta en `http://localhost:5080`.

El Docker local utiliza cifrado SQL con una excepción de certificado de desarrollo de confianza. No copies `TrustServerCertificate=True` a producción.

## Local

### API

```bash
dotnet restore FYM.sln
dotnet ef database update --project API/FYMTechnology.Api
dotnet run --project API/FYMTechnology.Api
```

### Front-end

```bash
cd Front
npm install
npm run dev
```

Crea `Front/.env.local` cuando la URL sea diferente a la predeterminada:

```text
VITE_API_URL=https://localhost:7080/api
```

## Testing

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

## Documentación

- [Arquitectura y modelo ER](docs/ARCHITECTURE.md)
- [Referencia de la API](docs/API.md)
- [Operaciones](docs/OPERATIONS.md)
- [Política de seguridad](SECURITY.md)
- [Guía de contribución](CONTRIBUTING.md)
