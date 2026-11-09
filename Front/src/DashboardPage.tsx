import { useQuery } from '@tanstack/react-query'
import { api } from './api'
import { useAuth } from './auth-context'
import { CreateUserForm } from './components/CreateUserForm'
import { UsersTable } from './components/UsersTable'
import { superAdminRole } from './types'

export function DashboardPage() {
  const { session, logout } = useAuth()
  const token = session?.accessToken ?? ''
  const isAdmin = session?.user.roles.includes(superAdminRole) ?? false
  const roles = useQuery({
    queryKey: ['roles'],
    queryFn: () => api.roles(token),
    enabled: Boolean(token),
  })
  const roleList = roles.data ?? []

  return (
    <>
      <header>
        <div>
          <strong>FYM Technology</strong>
          <span className="muted">User administration</span>
        </div>
        <div className="header-actions">
          <span>{session?.user.username}</span>
          <button className="secondary compact" onClick={logout}>
            Sign out
          </button>
        </div>
      </header>
      <main className={isAdmin ? 'layout' : 'layout single'}>
        {isAdmin && <CreateUserForm token={token} roles={roleList} />}
        <UsersTable token={token} roles={roleList} canEditRoles={isAdmin} />
      </main>
    </>
  )
}
