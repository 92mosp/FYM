import { DashboardPage } from './DashboardPage'
import { LoginPage } from './LoginPage'
import { useAuth } from './auth'

export function App() {
  const { session } = useAuth()
  return session ? <DashboardPage /> : <LoginPage />
}
