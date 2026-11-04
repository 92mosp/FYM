import { useState, type FormEvent } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api } from './api'
import { useAuth } from './auth'

export function DashboardPage() {
  const { session, logout } = useAuth()
  const queryClient = useQueryClient()
  const token = session?.accessToken ?? ''
  const isAdmin = session?.user.roles.includes('SuperAdmin') ?? false
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [roleIds, setRoleIds] = useState<number[]>([])
  const users = useQuery({ queryKey: ['users'], queryFn: () => api.users(token), enabled: Boolean(token) })
  const roles = useQuery({ queryKey: ['roles'], queryFn: () => api.roles(token), enabled: Boolean(token) })
  const createUser = useMutation({ mutationFn: () => api.createUser(token, { username, password, roleIds }), onSuccess: async () => { setUsername(''); setPassword(''); setRoleIds([]); await queryClient.invalidateQueries({ queryKey: ['users'] }) } })
  function submit(event: FormEvent) { event.preventDefault(); createUser.mutate() }
  return <><header><div><strong>FYM Technology</strong><span className="muted">User administration</span></div><div className="header-actions"><span>{session?.user.username}</span><button className="secondary compact" onClick={logout}>Sign out</button></div></header><main className="layout">{isAdmin && <section className="card"><h2>Create user</h2><form onSubmit={submit}><label>Username<input value={username} onChange={(event) => setUsername(event.target.value)} minLength={3} maxLength={100} required /></label><label>Temporary password<input type="password" value={password} onChange={(event) => setPassword(event.target.value)} minLength={12} maxLength={128} required /></label><fieldset><legend>Roles</legend>{roles.data?.map((role) => <label className="checkbox" key={role.id}><input type="checkbox" checked={roleIds.includes(role.id)} onChange={(event) => setRoleIds((current) => event.target.checked ? [...current, role.id] : current.filter((id) => id !== role.id))} />{role.name}</label>)}</fieldset>{createUser.error && <div className="alert error" role="alert">{createUser.error.message}</div>}<button disabled={createUser.isPending || roleIds.length === 0}>{createUser.isPending ? 'Creating...' : 'Create user'}</button></form></section>}<section className="card table-card"><div className="section-heading"><div><h2>Users</h2><p className="muted">Accounts and assigned roles</p></div><button className="secondary compact" onClick={() => users.refetch()}>Refresh</button></div>{users.isLoading && <p>Loading users...</p>}{users.error && <div className="alert error" role="alert">{users.error.message}</div>}{users.data && <div className="table-scroll"><table><thead><tr><th>Username</th><th>Roles</th><th>Created</th></tr></thead><tbody>{users.data.map((user) => <tr key={user.id}><td>{user.username}</td><td><div className="badges">{user.roles.map((role) => <span className="badge" key={role}>{role}</span>)}</div></td><td>{new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(new Date(user.createdAtUtc))}</td></tr>)}</tbody></table></div>}</section></main></>
}
