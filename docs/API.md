# API

Swagger está disponible en `/swagger` en el entorno de desarrollo (`Development`).

| Method | Path | Authorization | Purpose |
|---|---|---|---|
| POST | `/api/auth/login` | Anonimo | Autenticar y obtener JWT |
| GET | `/api/users` | Autenticado | Listar usuarios y roles |
| GET | `/api/users/{id}` | Autenticado | Obtener un usuario |
| POST | `/api/users` | SuperAdmin | Crear un usuario |
| PUT | `/api/users/{id}/roles` | SuperAdmin | Cambiar rol |
| GET | `/api/roles` | Autenticado | Listar roles |
| GET | `/health` | Anonimo | Health check |

## Ejemplo de login

```json
{
  "username": "admin",
  "password": "a-strong-password"
}
```

Usa el token resultante como `Authorization: Bearer <token>`.
