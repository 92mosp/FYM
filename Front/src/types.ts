export type Role = { id: number; name: string }
export type User = { id: number; username: string; roles: string[]; createdAtUtc: string }
export type Session = { accessToken: string; expiresAtUtc: string; user: User }
export type ProblemDetails = { title?: string; detail?: string; status?: number; traceId?: string }
