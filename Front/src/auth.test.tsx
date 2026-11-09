import { act, render, renderHook, screen, waitFor } from '@testing-library/react'
import { QueryClientProvider, type QueryClient } from '@tanstack/react-query'
import type { PropsWithChildren } from 'react'
import { afterEach, beforeEach, expect, test, vi } from 'vitest'
import { api } from './api'
import { AuthProvider, storageKey } from './auth'
import { useAuth } from './auth-context'
import { createTestQueryClient, jsonResponse, makeSession } from './test/utils'

const fetchMock = vi.fn<typeof fetch>()
let queryClient: QueryClient

function wrapper({ children }: PropsWithChildren) {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>{children}</AuthProvider>
    </QueryClientProvider>
  )
}

beforeEach(() => {
  queryClient = createTestQueryClient()
  vi.stubGlobal('fetch', fetchMock)
})

afterEach(() => {
  fetchMock.mockReset()
  vi.unstubAllGlobals()
  vi.useRealTimers()
  sessionStorage.clear()
})

test('hydrates a valid session from sessionStorage', () => {
  const session = makeSession()
  sessionStorage.setItem(storageKey, JSON.stringify(session))

  const { result } = renderHook(() => useAuth(), { wrapper })

  expect(result.current.session).toEqual(session)
})

test('ignores and removes an expired stored session', () => {
  const session = makeSession({ expiresAtUtc: new Date(Date.now() - 1000).toISOString() })
  sessionStorage.setItem(storageKey, JSON.stringify(session))

  const { result } = renderHook(() => useAuth(), { wrapper })

  expect(result.current.session).toBeNull()
  expect(sessionStorage.getItem(storageKey)).toBeNull()
})

test('ignores corrupt JSON in sessionStorage', () => {
  sessionStorage.setItem(storageKey, '{not json')

  const { result } = renderHook(() => useAuth(), { wrapper })

  expect(result.current.session).toBeNull()
})

test('ignores a stored value with the wrong shape', () => {
  sessionStorage.setItem(storageKey, JSON.stringify({ accessToken: 1 }))

  const { result } = renderHook(() => useAuth(), { wrapper })

  expect(result.current.session).toBeNull()
})

test('login stores the session', async () => {
  const session = makeSession()
  fetchMock.mockResolvedValue(jsonResponse(session))
  const { result } = renderHook(() => useAuth(), { wrapper })

  await act(() => result.current.login('admin', 'secret'))

  expect(result.current.session).toEqual(session)
  expect(JSON.parse(sessionStorage.getItem(storageKey)!)).toEqual(session)
})

test('logout clears storage and the query cache', () => {
  sessionStorage.setItem(storageKey, JSON.stringify(makeSession()))
  queryClient.setQueryData(['users'], [{ id: 1 }])
  const { result } = renderHook(() => useAuth(), { wrapper })

  act(() => result.current.logout())

  expect(result.current.session).toBeNull()
  expect(sessionStorage.getItem(storageKey)).toBeNull()
  expect(queryClient.getQueryData(['users'])).toBeUndefined()
})

test('logs out automatically when the token expires', () => {
  vi.useFakeTimers()
  sessionStorage.setItem(
    storageKey,
    JSON.stringify(makeSession({ expiresAtUtc: new Date(Date.now() + 5_000).toISOString() })),
  )
  queryClient.setQueryData(['users'], [{ id: 1 }])
  const { result } = renderHook(() => useAuth(), { wrapper })

  act(() => {
    vi.advanceTimersByTime(4_999)
  })
  expect(result.current.session).not.toBeNull()

  act(() => {
    vi.advanceTimersByTime(1)
  })
  expect(result.current.session).toBeNull()
  expect(queryClient.getQueryData(['users'])).toBeUndefined()
})

test('logs out when an authenticated request returns 401', async () => {
  sessionStorage.setItem(storageKey, JSON.stringify(makeSession()))
  fetchMock.mockResolvedValue(new Response(null, { status: 401 }))
  const { result } = renderHook(() => useAuth(), { wrapper })

  await act(async () => {
    await api.users('token-123').catch(() => undefined)
  })

  await waitFor(() => expect(result.current.session).toBeNull())
  expect(sessionStorage.getItem(storageKey)).toBeNull()
})

test('useAuth throws outside the provider', () => {
  vi.spyOn(console, 'error').mockImplementation(() => undefined)
  function Orphan() {
    useAuth()
    return null
  }

  expect(() => render(<Orphan />)).toThrow('useAuth must be used within AuthProvider')
  expect(screen.queryByText('anything')).toBeNull()
})
