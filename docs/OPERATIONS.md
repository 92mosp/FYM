# Operaciones

## Configuración

Usa variables de entorno con la notación de doble guion bajo de ASP.NET Core:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Seed__SuperAdminUsername`
- `Seed__SuperAdminPassword`
- `Cors__AllowedOrigins__0`

La clave JWT debe contener al menos 32 bytes. Rotala a través del almacén de secretos (*secret store*) de la plataforma de destino. Cambiar una clave de firma simétrica invalida los tokens existentes.

## Base de datos

El ejercicio utiliza `EnsureCreatedAsync` para mantener simple la configuración de la primera ejecución. Antes de evolucionar el esquema en producción, introduce migraciones de EF Core y ejecútalas en un trabajo de despliegue controlad (*deployment job*) en lugar de otorgar permisos de cambio de esquema a la identidad de ejecución (*runtime identity*).

## Protección de ramas (`Branch protection`)

Requiere *pull requests*, al menos una aprobación, resolución de conversaciones, *commits* firmados cuando estén disponibles, y las siguientes comprobaciones: CI de la API, CI del *front-end*, CodeQL, *dependency review*, Trivy y Gitleaks. Habilita el *secret scanning* de GitHub con *push protection* y el reporte privado de vulnerabilidades en la configuración del repositorio.
