# Gestión de usuarios - FYM

Prueba técnica de Michael Lugo Ospina para FYM: una REST API en ASP.NET Core con JWT, Swagger, SQL Server, usuarios y roles, y un cliente en React.

## Qué incluye

- API en ASP.NET Core 8 con EF Core, migraciones y SQL Server.
- Autenticación JWT y autorización por roles (`SuperAdmin`, `User`).
- `SuperAdmin` creado desde configuración (*seed* idempotente).
- Creación de usuarios con uno o varios roles, consulta de usuarios y roles, y asignación de roles.
- Validación de entrada con `ValidationProblemDetails`, errores RFC 7807, *rate limiting* global y específico del login.
- `GET /health` con comprobación de la base de datos.
- Cliente en React 18 + TypeScript + TanStack Query, con validación de respuestas con zod.
- Tests unitarios y de integración (xUnit + `WebApplicationFactory` + SQLite en memoria) y tests del cliente (Vitest + Testing Library).

## Requisitos

- SDK de .NET 8 (`global.json` fija la 8.0.400 con `rollForward: latestFeature`).
- Node.js 20 o superior.
- SQL Server local (por defecto `localhost\SQLEXPRESS` con autenticación de Windows).

## Configuración local

### API

`appsettings.json` no contiene secretos. `appsettings.Development.json` solo trae valores no sensibles para local: la cadena de conexión a `localhost\SQLEXPRESS`, el origen CORS de Vite (`http://localhost:5173`) y `Database:ApplyMigrationsOnStartup=true`.

Los secretos se guardan con `dotnet user-secrets` (fuera del repositorio):

```bash
dotnet tool restore
dotnet user-secrets set "Jwt:Key" "<valor aleatorio de al menos 32 bytes>" --project API/FYMTechnology.Api
dotnet user-secrets set "Seed:SuperAdminUsername" "admin" --project API/FYMTechnology.Api
dotnet user-secrets set "Seed:SuperAdminPassword" "<contraseña del SuperAdmin>" --project API/FYMTechnology.Api
```

Para generar una clave en PowerShell:

```powershell
[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
```

Si tu SQL Server es distinto, sobrescribe la cadena de conexión también con user-secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<cadena de conexión>" --project API/FYMTechnology.Api
```

Si falta `Jwt:Key`, la API no arranca y el mensaje explica cómo configurarla. Si faltan las credenciales del *seed* en `Development`, la API arranca sin crear el SuperAdmin y lo avisa en el log; en cualquier otro entorno no arranca.

### Cliente

```bash
cd Front
cp .env.example .env.local
```

`VITE_API_URL` es obligatoria (por defecto `https://localhost:7080/api`). Si falta, el cliente muestra un error que lo indica.

## Arranque

```bash
dotnet dev-certs https --trust
dotnet run --project API/FYMTechnology.Api --launch-profile https
```

En `Development` las migraciones se aplican al arrancar. Swagger queda en `https://localhost:7080/swagger`.

> Si tienes una base `FYMTechnologyDb` creada con la versión anterior (`EnsureCreated`), bórrala una vez: no tiene historial de migraciones.

```bash
cd Front
npm ci
npm run dev
```

Abre `http://localhost:5173` y entra con las credenciales del SuperAdmin que pusiste en user-secrets (`Seed:SuperAdminUsername` / `Seed:SuperAdminPassword`). Para probar el cierre de sesión automático, arranca la API con un token corto: `dotnet user-secrets set "Jwt:ExpiryMinutes" "1" --project API/FYMTechnology.Api`.

## Tests y checks

```bash
dotnet format FYM.sln --verify-no-changes
dotnet build FYM.sln -c Release
dotnet test FYM.sln -c Release --collect:"XPlat Code Coverage"
cd Front
npm ci
npm run format:check
npm run lint
npm test
npm run build
```

- La cobertura del backend se escribe en `API/FYMTechnology.Api.Tests/TestResults/<guid>/coverage.cobertura.xml`. `coverage.runsettings` (cargado automáticamente por el proyecto de tests) excluye `Program.cs` y `Data/Migrations`.
- `npm test` ejecuta Vitest con cobertura y falla por debajo de 80 % en líneas, funciones y sentencias, o de 70 % en ramas.

## Migraciones

```bash
dotnet tool restore
dotnet ef migrations add <Nombre> --project API/FYMTechnology.Api --output-dir Data/Migrations
```

`dotnet ef` construye la aplicación, así que necesita `Jwt:Key` configurada (user-secrets) y `ASPNETCORE_ENVIRONMENT=Development`.

## Documentación

- [Arquitectura y modelo ER](docs/ARCHITECTURE.md)
- [Referencia de la API](docs/API.md)
- [Operaciones](docs/OPERATIONS.md)
- [Despliegue en Azure](docs/DEPLOYMENT.md)
- [Política de seguridad](SECURITY.md)
- [Guía de contribución](CONTRIBUTING.md)
