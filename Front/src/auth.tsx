import { createContext, useContext, useMemo, useState, type PropsWithChildren } from 'react'
import { api } from './api'
import type { Session } from './types'

const storageKey = 'fym.session'
type AuthContextValue = { session: Session | null; login: (username: string, password: string) => Promise<void>; logout: () => void }
const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<Session | null>(() => {
    const stored = sessionStorage.getItem(storageKey)
    if (!stored) return null
    try {
      const parsed = JSON.parse(stored) as Session
      return new Date(parsed.expiresAtUtc) > new Date() ? parsed : null
    } catch { return null }
  })
  const value = useMemo<AuthContextValue>(() => ({ session, login: async (username, password) => { const next = await api.login(username, password); sessionStorage.setItem(storageKey, JSON.stringify(next)); setSession(next) }, logout: () => { sessionStorage.removeItem(storageKey); setSession(null) } }), [session])
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used within AuthProvider')
  return context
}
