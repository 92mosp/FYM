# Operaciones

## Configuración

Toda la configuración se puede sobrescribir con variables de entorno o *App Settings* usando la notación de doble guion bajo de ASP.NET Core (`Jwt__Key` equivale a `Jwt:Key`). La lista completa, con su origen recomendado, está en [DEPLOYMENT.md](DEPLOYMENT.md#variables-y-secretos).

En local, los secretos (`Jwt:Key`, `Seed:*`) se guardan con `dotnet user-secrets`; ver el [README](../README.md#configuración-local).

La clave JWT debe tener al menos 32 bytes. Rótala desde el almacén de secretos; cambiar una clave de firma simétrica invalida los tokens emitidos.

> La clave que estuvo versionada en `appsettings.json` en commits anteriores debe considerarse comprometida: no la uses en ningún entorno.

## Base de datos

El esquema se gestiona con migraciones de EF Core (`API/FYMTechnology.Api/Data/Migrations`).

- `Database:ApplyMigrationsOnStartup=true` (solo `Development`): la API aplica las migraciones al arrancar.
- En producción vale `false`: el pipeline aplica las migraciones con un *migrations bundle* antes de desplegar la API (ver [DEPLOYMENT.md](DEPLOYMENT.md#migraciones)). Así la identidad de ejecución no necesita permisos de cambio de esquema.

Al arrancar, la API crea los roles `SuperAdmin` y `User` y el SuperAdmin configurado si no existen. Es idempotente: ejecutarlo varias veces no duplica nada.

## Health check

`GET /health` es anónimo, no cuenta para el *rate limiting* y comprueba la conexión a la base de datos. Devuelve `200 Healthy` o `503 Unhealthy`.

## Rate limiting

- Global: 100 peticiones por minuto por IP (`RateLimiting:GlobalPermitLimit`).
- Login: 5 intentos por minuto por IP (`RateLimiting:LoginPermitLimit`).
- Ventana: `RateLimiting:WindowSeconds` (60).

Detrás de un proxy, la IP del cliente solo es correcta si están activados los *forwarded headers* (ver [DEPLOYMENT.md](DEPLOYMENT.md#https-detrás-del-proxy-de-azure)).
