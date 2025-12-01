import { createContext } from 'react'
import type { User } from 'firebase/auth'
import type { UserProfileResponse } from '@/config/api'

export type AuthContextValue = {
  user: User | null
  profile: UserProfileResponse | null
  loading: boolean
  accessError: string | null
  signIn: (email: string, password: string) => Promise<void>
  signInWithGoogle: () => Promise<void>
  signOut: () => Promise<void>
  hasRole: (...roles: string[]) => boolean
}

export const AuthContext = createContext<AuthContextValue | null>(null)
