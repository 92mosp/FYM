import { afterEach, beforeEach, expect, test, vi } from 'vitest'
import { api, ApiError, getBaseUrl, setUnauthorizedHandler } from './api'
import { jsonResponse, makeSession, makeUser, roles } from './test/utils'

const fetchMock = vi.fn<typeof fetch>()

beforeEach(() => {
  vi.stubGlobal('fetch', fetchMock)
})

afterEach(() => {
  fetchMock.mockReset()
  vi.unstubAllGlobals()
  vi.unstubAllEnvs()
  setUnauthorizedHandler(null)
})

function lastCall() {
  const [url, init] = fetchMock.mock.calls.at(-1)!
  return { url: url as string, init: init!, headers: init!.headers as Record<string, string> }
}

test('login posts credentials without a bearer header and validates the session', async () => {
  const session = makeSession()
  fetchMock.mockResolvedValue(jsonResponse(session))

  await expect(api.login('admin', 'secret')).resolves.toEqual(session)

  const { url, init, headers } = lastCall()
  expect(url).toBe('http://api.test/api/auth/login')
  expect(init.method).toBe('POST')
  expect(init.body).toBe(JSON.stringify({ username: 'admin', password: 'secret' }))
  expect(headers.Authorization).toBeUndefined()
})

test('authenticated requests send the bearer token', async () => {
  fetchMock.mockResolvedValue(jsonResponse([makeUser()]))

  await api.users('token-abc')

  expect(lastCall().headers.Authorization).toBe('Bearer token-abc')
})

test('roles, createUser and assignRoles hit the expected endpoints', async () => {
  fetchMock.mockResolvedValueOnce(jsonResponse(roles))
  await expect(api.roles('t')).resolves.toEqual(roles)
  expect(lastCall().url).toBe('http://api.test/api/roles')

  const created = makeUser({ id: 7, username: 'new', roles: ['User'] })
  fetchMock.mockResolvedValueOnce(jsonResponse(created, 201))
  await expect(
    api.createUser('t', { username: 'new', password: 'Password-1234', roleIds: [2] }),
  ).resolves.toEqual(created)
  expect(lastCall().init.method).toBe('POST')
  expect(lastCall().init.body).toBe(
    JSON.stringify({ username: 'new', password: 'Password-1234', roleIds: [2] }),
  )

  fetchMock.mockResolvedValueOnce(jsonResponse(created))
  await api.assignRoles('t', 7, [1, 2])
  expect(lastCall().url).toBe('http://api.test/api/users/7/roles')
  expect(lastCall().init.method).toBe('PUT')
  expect(lastCall().init.body).toBe(JSON.stringify({ roleIds: [1, 2] }))
})

test('errors use the ProblemDetails title', async () => {
  fetchMock.mockResolvedValue(jsonResponse({ title: 'Username is already registered.' }, 409))

  const error = await api.users('t').catch((reason: unknown) => reason)

  expect(error).toBeInstanceOf(ApiError)
  expect((error as ApiError).message).toBe('Username is already registered.')
  expect((error as ApiError).status).toBe(409)
})

test('errors without a body fall back to a generic message', async () => {
  fetchMock.mockResolvedValue(new Response(null, { status: 500 }))

  await expect(api.users('t')).rejects.toThrow('Request failed with status 500')
})

test('a 401 on an authenticated request triggers the unauthorized handler', async () => {
  const handler = vi.fn()
  setUnauthorizedHandler(handler)
  fetchMock.mockResolvedValue(new Response(null, { status: 401 }))

  await expect(api.users('expired')).rejects.toThrow()

  expect(handler).toHaveBeenCalledTimes(1)
})

test('a 401 on login does not trigger the unauthorized handler', async () => {
  const handler = vi.fn()
  setUnauthorizedHandler(handler)
  fetchMock.mockResolvedValue(jsonResponse({ title: 'Unauthorized' }, 401))

  await expect(api.login('admin', 'wrong')).rejects.toThrow('Unauthorized')

  expect(handler).not.toHaveBeenCalled()
})

test('responses that do not match the schema are rejected', async () => {
  fetchMock.mockResolvedValue(jsonResponse([{ id: 'not-a-number', username: 'x' }]))

  await expect(api.users('t')).rejects.toThrow()
})

test('a missing VITE_API_URL fails with a clear message', () => {
  vi.stubEnv('VITE_API_URL', '')

  expect(() => getBaseUrl()).toThrow('VITE_API_URL is not configured')
})

test('trailing slashes in VITE_API_URL are removed', () => {
  vi.stubEnv('VITE_API_URL', 'https://example.test/api/')

  expect(getBaseUrl()).toBe('https://example.test/api')
})
