import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { expect, test, vi } from 'vitest'
import { LoginPage } from './LoginPage'
import { renderWithProviders } from './test/utils'

test('submits the credentials without enforcing the password policy', async () => {
  const user = userEvent.setup()
  const login = vi.fn().mockResolvedValue(undefined)
  renderWithProviders(<LoginPage />, { auth: { login } })

  await user.type(screen.getByLabelText('Username'), 'admin')
  await user.type(screen.getByLabelText('Password'), 'x')
  await user.click(screen.getByRole('button', { name: 'Sign in' }))

  expect(login).toHaveBeenCalledWith('admin', 'x')
  expect(screen.getByLabelText('Password')).not.toHaveAttribute('minLength')
})

test('shows a loading state while signing in', async () => {
  const user = userEvent.setup()
  let resolve: () => void = () => undefined
  const login = vi.fn(() => new Promise<void>((done) => (resolve = done)))
  renderWithProviders(<LoginPage />, { auth: { login } })

  await user.type(screen.getByLabelText('Username'), 'admin')
  await user.type(screen.getByLabelText('Password'), 'secret')
  await user.click(screen.getByRole('button', { name: 'Sign in' }))

  expect(screen.getByRole('button', { name: 'Signing in...' })).toBeDisabled()
  resolve()
  expect(await screen.findByRole('button', { name: 'Sign in' })).toBeEnabled()
})

test('shows an accessible login error', async () => {
  const user = userEvent.setup()
  const login = vi.fn().mockRejectedValue(new Error('Invalid credentials'))
  renderWithProviders(<LoginPage />, { auth: { login } })

  await user.type(screen.getByLabelText('Username'), 'michael')
  await user.type(screen.getByLabelText('Password'), 'password123')
  await user.click(screen.getByRole('button', { name: 'Sign in' }))

  expect(await screen.findByRole('alert')).toHaveTextContent('Invalid credentials')
})

test('shows a generic error for non-Error rejections', async () => {
  const user = userEvent.setup()
  const login = vi.fn().mockRejectedValue('boom')
  renderWithProviders(<LoginPage />, { auth: { login } })

  await user.type(screen.getByLabelText('Username'), 'michael')
  await user.type(screen.getByLabelText('Password'), 'password123')
  await user.click(screen.getByRole('button', { name: 'Sign in' }))

  expect(await screen.findByRole('alert')).toHaveTextContent('Login failed')
})
