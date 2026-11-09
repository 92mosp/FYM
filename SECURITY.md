# Política de seguridad (`Security policy`)

Reporta las vulnerabilidades de forma privada a través de los avisos de seguridad de GitHub (*GitHub Security Advisories*). No crees incidencias públicas (*public issues*) para vulnerabilidades sospechosas.

Las versiones compatibles (*supported releases*) son la rama `main` actual y la última versión etiquetada (*tagged release*). Incluye los pasos de reproducción (*reproduction steps*), el componente afectado, el impacto y cualquier solución sugerida (*remediation*). El equipo debe acusar recibo de los reportes en un plazo de dos días hábiles y establecer un objetivo de resolución basado en la gravedad (*severity*).

Los secretos nunca se incluyen en el repositorio: en local se guardan con `dotnet user-secrets` y `Front/.env.local`, y en Azure en Key Vault (referenciados desde los *App Settings*) o en los secretos del pipeline. Ver [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md#variables-y-secretos).
