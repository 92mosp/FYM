import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, expect, test, vi } from 'vitest'
import { api } from '../api'
import { makeUser, renderWithProviders, roles } from '../test/utils'
import { UserRolesEditor } from './UserRolesEditor'

vi.mock('../api', () => ({ api: { assignRoles: vi.fn() } }))
const assignRoles = vi.mocked(api.assignRoles)

beforeEach(() => {
  assignRoles.mockReset()
})

const maria = makeUser({ id: 2, username: 'maria', roles: ['User'] })

test('starts with the current roles and only enables save after a change', async () => {
  renderWithProviders(<UserRolesEditor token="t" user={maria} roles={roles} />)
  const save = screen.getByRole('button', { name: 'Save roles for maria' })

  expect(screen.getByLabelText('User')).toBeChecked()
  expect(screen.getByLabelText('SuperAdmin')).not.toBeChecked()
  expect(save).toBeDisabled()

  await userEvent.setup().click(screen.getByLabelText('SuperAdmin'))

  expect(save).toBeEnabled()
})

test('does not allow saving an empty role list', async () => {
  renderWithProviders(<UserRolesEditor token="t" user={maria} roles={roles} />)

  await userEvent.setup().click(screen.getByLabelText('User'))

  expect(screen.getByRole('button', { name: 'Save roles for maria' })).toBeDisabled()
})

test('saves the selected roles and refreshes users', async () => {
  assignRoles.mockResolvedValue({ ...maria, roles: ['SuperAdmin', 'User'] })
  const { queryClient } = renderWithProviders(
    <UserRolesEditor token="t" user={maria} roles={roles} />,
  )
  const invalidate = vi.spyOn(queryClient, 'invalidateQueries')
  const user = userEvent.setup()

  await user.click(screen.getByLabelText('SuperAdmin'))
  await user.click(screen.getByRole('button', { name: 'Save roles for maria' }))

  await waitFor(() => expect(invalidate).toHaveBeenCalledWith({ queryKey: ['users'] }))
  expect(assignRoles).toHaveBeenCalledWith('t', 2, [2, 1])
})

test('shows a pending state while saving', async () => {
  assignRoles.mockReturnValue(new Promise(() => undefined))
  renderWithProviders(<UserRolesEditor token="t" user={maria} roles={roles} />)

  const user = userEvent.setup()
  await user.click(screen.getByLabelText('SuperAdmin'))
  await user.click(screen.getByRole('button', { name: 'Save roles for maria' }))

  expect(await screen.findByRole('button', { name: 'Save roles for maria' })).toHaveTextContent(
    'Saving...',
  )
})

test('shows the API error', async () => {
  assignRoles.mockRejectedValue(new Error('You cannot remove your own SuperAdmin role.'))
  renderWithProviders(<UserRolesEditor token="t" user={maria} roles={roles} />)

  const user = userEvent.setup()
  await user.click(screen.getByLabelText('SuperAdmin'))
  await user.click(screen.getByRole('button', { name: 'Save roles for maria' }))

  expect(await screen.findByRole('alert')).toHaveTextContent(
    'You cannot remove your own SuperAdmin role.',
  )
})
