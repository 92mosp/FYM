import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, expect, test, vi } from 'vitest'
import { api } from '../api'
import { makeUser, renderWithProviders, roles } from '../test/utils'
import { UsersTable } from './UsersTable'

vi.mock('../api', () => ({ api: { users: vi.fn(), assignRoles: vi.fn() } }))
const users = vi.mocked(api.users)

beforeEach(() => {
  users.mockReset()
})

const data = [
  makeUser({ id: 1, username: 'admin', roles: ['SuperAdmin'] }),
  makeUser({ id: 2, username: 'maria', roles: ['User'] }),
]

test('shows a loading state and then the users with their roles', async () => {
  users.mockResolvedValue(data)
  renderWithProviders(<UsersTable token="t" roles={roles} canEditRoles={false} />)

  expect(screen.getByText('Loading users...')).toBeInTheDocument()
  expect(await screen.findByText('maria')).toBeInTheDocument()
  expect(screen.getAllByText('User', { selector: '.badge' })).toHaveLength(1)
  expect(users).toHaveBeenCalledWith('t')
})

test('hides the role editor for users without SuperAdmin', async () => {
  users.mockResolvedValue(data)
  renderWithProviders(<UsersTable token="t" roles={roles} canEditRoles={false} />)

  await screen.findByText('maria')

  expect(screen.queryByRole('columnheader', { name: 'Edit roles' })).not.toBeInTheDocument()
  expect(screen.queryByRole('checkbox')).not.toBeInTheDocument()
})

test('shows a role editor per user for SuperAdmin', async () => {
  users.mockResolvedValue(data)
  renderWithProviders(<UsersTable token="t" roles={roles} canEditRoles />)

  await screen.findByText('maria')

  expect(screen.getByRole('columnheader', { name: 'Edit roles' })).toBeInTheDocument()
  expect(screen.getByRole('button', { name: 'Save roles for maria' })).toBeInTheDocument()
})

test('shows load errors and refetches on demand', async () => {
  users.mockRejectedValueOnce(new Error('Network down')).mockResolvedValueOnce(data)
  renderWithProviders(<UsersTable token="t" roles={roles} canEditRoles={false} />)

  expect(await screen.findByRole('alert')).toHaveTextContent('Network down')

  await userEvent.setup().click(screen.getByRole('button', { name: 'Refresh' }))

  expect(await screen.findByText('maria')).toBeInTheDocument()
})
