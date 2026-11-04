import { useState, type FormEvent } from 'react'
import { useAuth } from './auth'

export function LoginPage() {
  const { login } = useAuth()
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  async function submit(event: FormEvent) {
    event.preventDefault(); setError(''); setLoading(true)
    try { await login(username, password) } catch (reason) { setError(reason instanceof Error ? reason.message : 'Login failed') } finally { setLoading(false) }
  }
  return <main className="centered"><section className="card login-card"><div className="brand">FYM Technology</div><h1>Sign in</h1><p className="muted">Manage users and roles securely.</p><form onSubmit={submit}><label>Username<input autoComplete="username" value={username} onChange={(event) => setUsername(event.target.value)} minLength={3} required /></label><label>Password<input type="password" autoComplete="current-password" value={password} onChange={(event) => setPassword(event.target.value)} minLength={8} required /></label>{error && <div className="alert error" role="alert">{error}</div>}<button disabled={loading}>{loading ? 'Signing in...' : 'Sign in'}</button></form></section></main>
}
