import { z } from 'zod'
import type { ProblemDetails, Role, Session, User } from './types'

const baseUrl = import.meta.env.VITE_API_URL ?? 'https://localhost:7080/api'
const sessionSchema = z.object({ accessToken: z.string(), expiresAtUtc: z.string(), user: z.object({ id: z.number(), username: z.string(), roles: z.array(z.string()), createdAtUtc: z.string() }) })

async function request<T>(path: string, init: RequestInit = {}, token?: string): Promise<T> {
  const response = await fetch(`${baseUrl}${path}`, { ...init, headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}), ...init.headers } })
  if (!response.ok) {
    const problem = (await response.json().catch(() => ({}))) as ProblemDetails
    throw new Error(problem.title ?? `Request failed with status ${response.status}`)
  }
  return (await response.json()) as T
}

export const api = {
  async login(username: string, password: string): Promise<Session> {
    return sessionSchema.parse(await request<unknown>('/auth/login', { method: 'POST', body: JSON.stringify({ username, password }) }))
  },
  users(token: string): Promise<User[]> { return request('/users', {}, token) },
  roles(token: string): Promise<Role[]> { return request('/roles', {}, token) },
  createUser(token: string, input: { username: string; password: string; roleIds: number[] }): Promise<User> { return request('/users', { method: 'POST', body: JSON.stringify(input) }, token) },
  assignRoles(token: string, id: number, roleIds: number[]): Promise<User> { return request(`/users/${id}/roles`, { method: 'PUT', body: JSON.stringify({ roleIds }) }, token) }
}
