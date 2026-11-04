# API reference

Interactive Swagger documentation is available at `/swagger` in the Development environment.

| Method | Path | Authorization | Purpose |
|---|---|---|---|
| POST | `/api/auth/login` | Anonymous | Authenticate and obtain a JWT |
| GET | `/api/users` | Authenticated | List users and roles |
| GET | `/api/users/{id}` | Authenticated | Get one user |
| POST | `/api/users` | SuperAdmin | Create a user |
| PUT | `/api/users/{id}/roles` | SuperAdmin | Replace assigned roles |
| GET | `/api/roles` | Authenticated | List roles |
| GET | `/health` | Anonymous | Health probe |

## Login example

```json
{
  "username": "admin",
  "password": "a-strong-password"
}
```

Use the resulting token as `Authorization: Bearer <token>`.
