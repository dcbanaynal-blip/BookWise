import { useContext } from 'react'
import { AuthContext } from './context'
export { FirebaseAuthProvider } from './AuthProvider'

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within FirebaseAuthProvider')
  }
  return context
}
