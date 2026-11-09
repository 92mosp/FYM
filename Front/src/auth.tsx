import { useCallback, useEffect, useMemo, useState, type PropsWithChildren } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { api, setUnauthorizedHandler } from './api'
import { AuthContext, type AuthContextValue } from './auth-context'
import { sessionSchema, type Session } from './types'

export const storageKey = 'fym.session'
const maxTimeoutMs = 2_147_483_647

function readStoredSession(): Session | null {
  const stored = sessionStorage.getItem(storageKey)
  if (!stored) return null
  try {
    const parsed = sessionSchema.safeParse(JSON.parse(stored))
    if (parsed.success && new Date(parsed.data.expiresAtUtc) > new Date()) return parsed.data
  } catch {
    return null
  }
  sessionStorage.removeItem(storageKey)
  return null
}

export function AuthProvider({ children }: PropsWithChildren) {
  const queryClient = useQueryClient()
  const [session, setSession] = useState<Session | null>(readStoredSession)

  const logout = useCallback(() => {
    sessionStorage.removeItem(storageKey)
    queryClient.clear()
    setSession(null)
  }, [queryClient])

  const login = useCallback(async (username: string, password: string) => {
    const next = await api.login(username, password)
    sessionStorage.setItem(storageKey, JSON.stringify(next))
    setSession(next)
  }, [])

  useEffect(() => {
    setUnauthorizedHandler(logout)
    return () => setUnauthorizedHandler(null)
  }, [logout])

  useEffect(() => {
    if (!session) return
    const remaining = new Date(session.expiresAtUtc).getTime() - Date.now()
    const timer = setTimeout(logout, Math.min(Math.max(remaining, 0), maxTimeoutMs))
    return () => clearTimeout(timer)
  }, [session, logout])

  const value = useMemo<AuthContextValue>(
    () => ({ session, login, logout }),
    [session, login, logout],
  )
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
