import { useQuery } from '@tanstack/react-query'
import { api } from '../api'
import type { Role } from '../types'
import { UserRolesEditor } from './UserRolesEditor'

type Props = { token: string; roles: Role[]; canEditRoles: boolean }

const dateFormat = new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' })

export function UsersTable({ token, roles, canEditRoles }: Props) {
  const users = useQuery({
    queryKey: ['users'],
    queryFn: () => api.users(token),
    enabled: Boolean(token),
  })

  return (
    <section className="card table-card">
      <div className="section-heading">
        <div>
          <h2>Users</h2>
          <p className="muted">Accounts and assigned roles</p>
        </div>
        <button className="secondary compact" onClick={() => void users.refetch()}>
          Refresh
        </button>
      </div>
      {users.isLoading && <p>Loading users...</p>}
      {users.error && (
        <div className="alert error" role="alert">
          {users.error.message}
        </div>
      )}
      {users.data && (
        <div className="table-scroll">
          <table>
            <thead>
              <tr>
                <th>Username</th>
                <th>Roles</th>
                <th>Created</th>
                {canEditRoles && <th>Edit roles</th>}
              </tr>
            </thead>
            <tbody>
              {users.data.map((user) => (
                <tr key={user.id}>
                  <td>{user.username}</td>
                  <td>
                    <div className="badges">
                      {user.roles.map((role) => (
                        <span className="badge" key={role}>
                          {role}
                        </span>
                      ))}
                    </div>
                  </td>
                  <td>{dateFormat.format(new Date(user.createdAtUtc))}</td>
                  {canEditRoles && (
                    <td>
                      <UserRolesEditor
                        key={`${user.id}-${user.roles.join(',')}-${roles.length}`}
                        token={token}
                        user={user}
                        roles={roles}
                      />
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
