import { z } from 'zod'
import {
  roleSchema,
  sessionSchema,
  userSchema,
  type ProblemDetails,
  type Role,
  type Session,
  type User,
} from './types'

export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
  ) {
    super(message)
    this.name = 'ApiError'
  }
}

let unauthorizedHandler: (() => void) | null = null

export function setUnauthorizedHandler(handler: (() => void) | null) {
  unauthorizedHandler = handler
}

export function getBaseUrl(): string {
  const url = import.meta.env.VITE_API_URL
  if (!url) {
    throw new Error(
      'VITE_API_URL is not configured. Copy Front/.env.example to Front/.env.local and set the API URL.',
    )
  }
  return url.replace(/\/+$/, '')
}

async function request<T>(
  path: string,
  schema: z.ZodType<T>,
  init: RequestInit = {},
  token?: string,
): Promise<T> {
  const response = await fetch(`${getBaseUrl()}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init.headers,
    },
  })
  if (!response.ok) {
    if (response.status === 401 && token) {
      unauthorizedHandler?.()
    }
    const problem = (await response.json().catch(() => ({}))) as ProblemDetails
    throw new ApiError(
      problem.title ?? `Request failed with status ${response.status}`,
      response.status,
    )
  }
  return schema.parse(await response.json())
}

export const api = {
  login: (username: string, password: string): Promise<Session> =>
    request('/auth/login', sessionSchema, {
      method: 'POST',
      body: JSON.stringify({ username, password }),
    }),
  users: (token: string): Promise<User[]> => request('/users', z.array(userSchema), {}, token),
  roles: (token: string): Promise<Role[]> => request('/roles', z.array(roleSchema), {}, token),
  createUser: (
    token: string,
    input: { username: string; password: string; roleIds: number[] },
  ): Promise<User> =>
    request('/users', userSchema, { method: 'POST', body: JSON.stringify(input) }, token),
  assignRoles: (token: string, id: number, roleIds: number[]): Promise<User> =>
    request(
      `/users/${id}/roles`,
      userSchema,
      { method: 'PUT', body: JSON.stringify({ roleIds }) },
      token,
    ),
}
