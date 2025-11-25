import {
  onAuthStateChanged,
  signInWithEmailAndPassword,
  signOut as firebaseSignOut,
  type User,
} from 'firebase/auth'
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react'
import { firebaseAuth } from '../config/firebase'

type AuthContextValue = {
  user: User | null
  loading: boolean
  signIn: (email: string, password: string) => Promise<void>
  signOut: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const unsubscribe = onAuthStateChanged(firebaseAuth, currentUser => {
      setUser(currentUser)
      setLoading(false)
    })

    return unsubscribe
  }, [])

  const signIn = useCallback(async (email: string, password: string) => {
    await signInWithEmailAndPassword(firebaseAuth, email, password)
  }, [])

  const signOut = useCallback(async () => {
    await firebaseSignOut(firebaseAuth)
  }, [])

  const value = useMemo(
    () => ({
      user,
      loading,
      signIn,
      signOut,
    }),
    [user, loading, signIn, signOut],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
