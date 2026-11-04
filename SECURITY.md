# Política de seguridad (`Security policy`)

Reporta las vulnerabilidades de forma privada a través de los avisos de seguridad de GitHub (*GitHub Security Advisories*). No crees incidencias públicas (*public issues*) para vulnerabilidades sospechosas.

Las versiones compatibles (*supported releases*) son la rama `main` actual y la última versión etiquetada (*tagged release*). Incluye los pasos de reproducción (*reproduction steps*), el componente afectado, el impacto y cualquier solución sugerida (*remediation*). El equipo debe acusar recibo de los reportes en un plazo de dos días hábiles y establecer un objetivo de resolución basado en la gravedad (*severity*).

Los secretos deben almacenarse en los secretos de GitHub Actions (*GitHub Actions secrets*) o en el almacén de secretos de la plataforma de despliegue (*deployment platform secret store*). Nunca deben incluirse en commits en el repositorio.
