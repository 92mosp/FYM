import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, expect, test, vi } from 'vitest'
import { api } from '../api'
import { makeUser, renderWithProviders, roles } from '../test/utils'
import { CreateUserForm } from './CreateUserForm'

vi.mock('../api', () => ({ api: { createUser: vi.fn() } }))
const createUser = vi.mocked(api.createUser)

beforeEach(() => {
  createUser.mockReset()
})

async function fillForm() {
  const user = userEvent.setup()
  await user.type(screen.getByLabelText('Username'), 'newuser')
  await user.type(screen.getByLabelText('Temporary password'), 'Password-12345')
  await user.click(screen.getByLabelText('SuperAdmin'))
  await user.click(screen.getByLabelText('User'))
  return user
}

test('requires at least one role before submitting', async () => {
  renderWithProviders(<CreateUserForm token="t" roles={roles} />)
  const button = screen.getByRole('button', { name: 'Create user' })
  expect(button).toBeDisabled()

  const user = userEvent.setup()
  await user.click(screen.getByLabelText('User'))
  expect(button).toBeEnabled()
  await user.click(screen.getByLabelText('User'))
  expect(button).toBeDisabled()
})

test('creates a user with several roles, resets the form and refreshes users', async () => {
  createUser.mockResolvedValue(
    makeUser({ id: 5, username: 'newuser', roles: ['SuperAdmin', 'User'] }),
  )
  const { queryClient } = renderWithProviders(<CreateUserForm token="t" roles={roles} />)
  const invalidate = vi.spyOn(queryClient, 'invalidateQueries')

  const user = await fillForm()
  await user.click(screen.getByRole('button', { name: 'Create user' }))

  expect(await screen.findByRole('status')).toHaveTextContent('User newuser created.')
  expect(createUser).toHaveBeenCalledWith('t', {
    username: 'newuser',
    password: 'Password-12345',
    roleIds: [1, 2],
  })
  expect(screen.getByLabelText('Username')).toHaveValue('')
  expect(invalidate).toHaveBeenCalledWith({ queryKey: ['users'] })
})

test('shows a pending state while saving', async () => {
  createUser.mockReturnValue(new Promise(() => undefined))
  renderWithProviders(<CreateUserForm token="t" roles={roles} />)

  const user = await fillForm()
  await user.click(screen.getByRole('button', { name: 'Create user' }))

  expect(await screen.findByRole('button', { name: 'Creating...' })).toBeDisabled()
})

test('shows the API error', async () => {
  createUser.mockRejectedValue(new Error('Username is already registered.'))
  renderWithProviders(<CreateUserForm token="t" roles={roles} />)

  const user = await fillForm()
  await user.click(screen.getByRole('button', { name: 'Create user' }))

  expect(await screen.findByRole('alert')).toHaveTextContent('Username is already registered.')
})
