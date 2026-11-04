import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { vi } from 'vitest'
import { AuthProvider } from './auth'
import { LoginPage } from './LoginPage'

vi.mock('./api', () => ({ api: { login: vi.fn().mockRejectedValue(new Error('Invalid credentials')) } }))

test('shows an accessible login error', async () => {
  const user = userEvent.setup()
  render(<QueryClientProvider client={new QueryClient()}><AuthProvider><LoginPage /></AuthProvider></QueryClientProvider>)
  await user.type(screen.getByLabelText('Username'), 'michael')
  await user.type(screen.getByLabelText('Password'), 'password123')
  await user.click(screen.getByRole('button', { name: 'Sign in' }))
  expect(await screen.findByRole('alert')).toHaveTextContent('Invalid credentials')
})
