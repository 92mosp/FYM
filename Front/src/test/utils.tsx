import type { ReactElement } from 'react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render } from '@testing-library/react'
import { vi } from 'vitest'
import { AuthContext, type AuthContextValue } from '../auth-context'
import type { Role, Session, User } from '../types'

export const roles: Role[] = [
  { id: 1, name: 'SuperAdmin' },
  { id: 2, name: 'User' },
]

export function makeUser(overrides: Partial<User> = {}): User {
  return {
    id: 1,
    username: 'admin',
    roles: ['SuperAdmin'],
    createdAtUtc: '2026-01-01T00:00:00Z',
    ...overrides,
  }
}

export function makeSession(overrides: Partial<Session> = {}): Session {
  return {
    accessToken: 'token-123',
    expiresAtUtc: new Date(Date.now() + 60_000).toISOString(),
    user: makeUser(),
    ...overrides,
  }
}

export function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })
}

export function renderWithProviders(
  ui: ReactElement,
  options: { auth?: Partial<AuthContextValue>; queryClient?: QueryClient } = {},
) {
  const queryClient = options.queryClient ?? createTestQueryClient()
  const auth: AuthContextValue = {
    session: null,
    login: vi.fn(),
    logout: vi.fn(),
    ...options.auth,
  }
  const result = render(
    <QueryClientProvider client={queryClient}>
      <AuthContext.Provider value={auth}>{ui}</AuthContext.Provider>
    </QueryClientProvider>,
  )
  return { ...result, auth, queryClient }
}

export function jsonResponse(body: unknown, status = 200) {
  return new Response(JSON.stringify(body), {
    status,
    headers: { 'Content-Type': 'application/json' },
  })
}
