import { useState, type FormEvent } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { api } from '../api'
import type { Role } from '../types'
import { RoleCheckboxes } from './RoleCheckboxes'

type Props = { token: string; roles: Role[] }

export function CreateUserForm({ token, roles }: Props) {
  const queryClient = useQueryClient()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [roleIds, setRoleIds] = useState<number[]>([])
  const createUser = useMutation({
    mutationFn: () => api.createUser(token, { username, password, roleIds }),
    onSuccess: async () => {
      setUsername('')
      setPassword('')
      setRoleIds([])
      await queryClient.invalidateQueries({ queryKey: ['users'] })
    },
  })

  function submit(event: FormEvent) {
    event.preventDefault()
    createUser.mutate()
  }

  return (
    <section className="card">
      <h2>Create user</h2>
      <form onSubmit={submit}>
        <label>
          Username
          <input
            value={username}
            onChange={(event) => setUsername(event.target.value)}
            minLength={3}
            maxLength={100}
            required
          />
        </label>
        <label>
          Temporary password
          <input
            type="password"
            autoComplete="new-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            minLength={12}
            maxLength={128}
            required
          />
        </label>
        <fieldset>
          <legend>Roles</legend>
          <RoleCheckboxes
            roles={roles}
            selected={roleIds}
            onChange={setRoleIds}
            idPrefix="new-user-role"
          />
        </fieldset>
        {createUser.error && (
          <div className="alert error" role="alert">
            {createUser.error.message}
          </div>
        )}
        {createUser.isSuccess && (
          <div className="alert success" role="status">
            User {createUser.data.username} created.
          </div>
        )}
        <button disabled={createUser.isPending || roleIds.length === 0}>
          {createUser.isPending ? 'Creating...' : 'Create user'}
        </button>
      </form>
    </section>
  )
}
