import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../api'
import type { Role, User } from '../types'
import { RoleCheckboxes } from './RoleCheckboxes'

type Props = { token: string; user: User; roles: Role[] }

export function UserRolesEditor({ token, user, roles }: Props) {
  const queryClient = useQueryClient()
  const initial = roles.filter((role) => user.roles.includes(role.name)).map((role) => role.id)
  const [selected, setSelected] = useState<number[]>(initial)
  const changed = selected.length !== initial.length || selected.some((id) => !initial.includes(id))
  const assignRoles = useMutation({
    mutationFn: () => api.assignRoles(token, user.id, selected),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['users'] }),
  })

  return (
    <div className="roles-editor">
      <RoleCheckboxes
        roles={roles}
        selected={selected}
        onChange={setSelected}
        idPrefix={`user-${user.id}-role`}
      />
      <button
        className="compact"
        aria-label={`Save roles for ${user.username}`}
        disabled={assignRoles.isPending || selected.length === 0 || !changed}
        onClick={() => assignRoles.mutate()}
      >
        {assignRoles.isPending ? 'Saving...' : 'Save'}
      </button>
      {assignRoles.error && (
        <div className="alert error" role="alert">
          {assignRoles.error.message}
        </div>
      )}
    </div>
  )
}
