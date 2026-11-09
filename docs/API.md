# API

Swagger está disponible en `/swagger` en el entorno de desarrollo (`Development`).

| Método | Ruta | Autorización | Propósito |
|---|---|---|---|
| POST | `/api/auth/login` | Anónimo (máx. 5 por minuto por IP) | Autenticar y obtener JWT |
| GET | `/api/users` | Autenticado | Listar usuarios y roles |
| GET | `/api/users/{id}` | Autenticado | Obtener un usuario |
| POST | `/api/users` | SuperAdmin | Crear un usuario con uno o varios roles |
| PUT | `/api/users/{id}/roles` | SuperAdmin | Reemplazar los roles de un usuario |
| GET | `/api/roles` | Autenticado | Listar roles |
| GET | `/health` | Anónimo, sin *rate limiting* | Estado de la API y de la base de datos |

## Login

```json
{
  "username": "admin",
  "password": "<contraseña>"
}
```

Respuesta `200`:

```json
{
  "accessToken": "<jwt>",
  "expiresAtUtc": "2026-11-09T12:30:00+00:00",
  "user": { "id": 1, "username": "admin", "roles": ["SuperAdmin"], "createdAtUtc": "..." }
}
```

Usa el token como `Authorization: Bearer <token>`. El login no aplica la política de contraseñas; solo exige ambos campos (máximo 100 y 128 caracteres). Credenciales incorrectas o usuario inexistente devuelven `401` sin distinguir el caso (también en tiempo de respuesta). Superado el límite, `429`.

## Crear usuario

```json
{
  "username": "maria",
  "password": "una-contraseña-larga",
  "roleIds": [2]
}
```

Reglas: `username` de 3 a 100 caracteres, único sin distinguir mayúsculas; `password` de 12 a 128 caracteres; `roleIds` con al menos un rol existente. Respuesta `201` con el usuario y cabecera `Location`.

## Asignar roles

```json
{ "roleIds": [1, 2] }
```

Reemplaza los roles del usuario. Un usuario no puede quedarse sin roles, y un SuperAdmin no puede quitarse a sí mismo el rol `SuperAdmin`.

## Errores

Todas las respuestas de error usan `application/problem+json`.

| Código | Cuándo |
|---|---|
| 400 | Entrada inválida (`ValidationProblemDetails` con `errors` por campo) o rol inexistente |
| 401 | Sin token, token inválido o expirado, o credenciales incorrectas |
| 403 | El usuario no tiene el rol requerido |
| 404 | Usuario no encontrado |
| 409 | Usuario duplicado o intento de quitarse el propio rol `SuperAdmin` |
| 429 | Límite de peticiones superado |
| 500 | Error inesperado; el mensaje interno no se expone, solo `traceId` |
