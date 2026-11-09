import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, expect, test, vi } from 'vitest'
import { api } from './api'
import { App } from './App'
import { makeSession, makeUser, renderWithProviders, roles } from './test/utils'

vi.mock('./api', () => ({
  api: { users: vi.fn(), roles: vi.fn(), createUser: vi.fn(), assignRoles: vi.fn() },
}))

beforeEach(() => {
  vi.mocked(api.users).mockResolvedValue([makeUser()])
  vi.mocked(api.roles).mockResolvedValue(roles)
})

test('shows the login page without a session', () => {
  renderWithProviders(<App />)

  expect(screen.getByRole('heading', { name: 'Sign in' })).toBeInTheDocument()
})

test('shows the dashboard with admin controls for a SuperAdmin', async () => {
  renderWithProviders(<App />, { auth: { session: makeSession() } })

  expect(await screen.findByRole('heading', { name: 'Users' })).toBeInTheDocument()
  expect(screen.getByRole('heading', { name: 'Create user' })).toBeInTheDocument()
  expect(await screen.findByRole('columnheader', { name: 'Edit roles' })).toBeInTheDocument()
})

test('hides admin controls for a regular user and signs out', async () => {
  const session = makeSession({ user: makeUser({ id: 9, username: 'maria', roles: ['User'] }) })
  const { auth } = renderWithProviders(<App />, { auth: { session } })

  expect(await screen.findByRole('heading', { name: 'Users' })).toBeInTheDocument()
  expect(screen.queryByRole('heading', { name: 'Create user' })).not.toBeInTheDocument()
  expect(screen.queryByRole('columnheader', { name: 'Edit roles' })).not.toBeInTheDocument()

  await userEvent.setup().click(screen.getByRole('button', { name: 'Sign out' }))

  expect(auth.logout).toHaveBeenCalled()
})
