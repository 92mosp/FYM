# Despliegue en Azure

Este documento describe lo que el pipeline de CI/CD deberá hacer. El pipeline y la infraestructura no forman parte del repositorio.

Opciones razonables de hosting: la API en Azure App Service o Azure Container Apps; el cliente en Azure Static Web Apps o en Azure Storage con Azure Front Door; la base de datos en Azure SQL Database.

## Pasos del pipeline

### 1. Backend: build, tests y publish

```bash
dotnet tool restore
dotnet format FYM.sln --verify-no-changes
dotnet build FYM.sln -c Release
dotnet test FYM.sln -c Release --no-build --collect:"XPlat Code Coverage"
dotnet publish API/FYMTechnology.Api/FYMTechnology.Api.csproj -c Release -o artifacts/api
```

La cobertura queda en `API/FYMTechnology.Api.Tests/TestResults/**/coverage.cobertura.xml` (formato Cobertura) para publicarla en el pipeline.

### 2. Frontend: checks y build

```bash
cd Front
npm ci
npm run format:check
npm run lint
npm test
VITE_API_URL=https://<api-host>/api npm run build
```

`VITE_API_URL` se incrusta en el bundle durante el build: hace falta un build por entorno. El resultado está en `Front/dist`.

### 3. Migraciones

Genera un *migrations bundle* autocontenido y ejecútalo contra la base de datos antes de desplegar la API:

```bash
dotnet ef migrations bundle --project API/FYMTechnology.Api --configuration Release --self-contained -r linux-x64 -o artifacts/efbundle
./artifacts/efbundle --connection "<cadena de conexión con permisos de cambio de esquema>"
```

`dotnet ef` construye la aplicación para descubrir el `DbContext`: en el agente define `ASPNETCORE_ENVIRONMENT=Development` y un `Jwt__Key` cualquiera de al menos 32 bytes solo para ese paso. El bundle es idempotente: aplica únicamente las migraciones pendientes. Usa `-r win-x64` si el agente es Windows.

### 4. Despliegue

1. Despliega `artifacts/api` con las variables de la tabla siguiente y `Database__ApplyMigrationsOnStartup=false`.
2. Comprueba `GET https://<api-host>/health` (debe devolver `200`).
3. Publica `Front/dist` en el hosting estático.

## Variables y secretos

| Nombre | Descripción | Origen recomendado |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | App Setting |
| `ConnectionStrings__DefaultConnection` | Cadena de conexión a Azure SQL con `Encrypt=True` (mejor con identidad administrada: `Authentication=Active Directory Managed Identity`) | Key Vault reference |
| `Jwt__Key` | Clave de firma HMAC, mínimo 32 bytes, aleatoria | Key Vault reference |
| `Jwt__Issuer` | Emisor del token (por defecto `FYMTechnology.Api`) | App Setting |
| `Jwt__Audience` | Audiencia del token (por defecto `FYMTechnology.Client`) | App Setting |
| `Jwt__ExpiryMinutes` | Vida del token (15 en `Production`) | App Setting |
| `Seed__SuperAdminUsername` | Usuario SuperAdmin inicial | Key Vault reference |
| `Seed__SuperAdminPassword` | Contraseña del SuperAdmin inicial | Key Vault reference |
| `Cors__AllowedOrigins__0` | Origen del cliente (`https://<front-host>`); `__1`, `__2`… para más | App Setting |
| `Database__ApplyMigrationsOnStartup` | `false` en producción | App Setting |
| `RateLimiting__GlobalPermitLimit` / `RateLimiting__LoginPermitLimit` | Opcionales (100 y 5) | App Setting |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | `true` (ver siguiente sección) | App Setting |
| `VITE_API_URL` | URL de la API para el build del cliente | Variable del pipeline |
| Conexión de migraciones | Cadena con permisos de DDL para el bundle | Secreto del pipeline o Key Vault |

Las *Key Vault references* (`@Microsoft.KeyVault(SecretUri=...)`) las resuelve la plataforma; la API no necesita el SDK de Key Vault.

## HTTPS detrás del proxy de Azure

App Service y Container Apps terminan TLS en su proxy y reenvían la petición por HTTP con `X-Forwarded-For` y `X-Forwarded-Proto`. Sin procesar esas cabeceras, la API cree que la petición es HTTP y que viene de la IP del proxy:

- `UseHttpsRedirection` redirige peticiones que ya eran HTTPS y `UseHsts` no envía la cabecera HSTS (solo la añade en peticiones HTTPS).
- El *rate limiting* por IP agrupa a todos los clientes en la IP del proxy, y el límite de 5 logins por minuto se compartiría entre todos los usuarios.

La API no llama a `UseForwardedHeaders` en código. Se activa con `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`: ASP.NET Core añade entonces el middleware para `X-Forwarded-For` y `X-Forwarded-Proto` aceptando cualquier proxy. App Service en Linux ya la define; en Container Apps y App Service en Windows hay que añadirla. Solo es seguro si la API no es accesible directamente, sin pasar por el proxy de Azure.

## Requisitos del hosting del cliente

- **Fallback SPA**: cualquier ruta que no sea un archivo existente debe servir `index.html` con `200`.
- **Cabeceras de seguridad** en todas las respuestas:
  - `X-Content-Type-Options: nosniff`
  - `X-Frame-Options: DENY`
  - `Referrer-Policy: strict-origin-when-cross-origin`
- HTTPS obligatorio.

En Static Web Apps ambas cosas se configuran en `staticwebapp.config.json` (`navigationFallback` y `globalHeaders`); en Front Door, con una regla de reescritura y un *rule set* de cabeceras.

## Health check

Configura las sondas (App Service Health check, sondas de Container Apps o de Front Door) contra `GET /health`. Es anónimo, no tiene *rate limiting* y devuelve `503` si la base de datos no responde.
