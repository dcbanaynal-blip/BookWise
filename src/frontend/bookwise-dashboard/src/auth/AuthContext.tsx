import {
  onAuthStateChanged,
  signInWithEmailAndPassword,
  signInWithPopup,
  GoogleAuthProvider,
  signOut as firebaseSignOut,
  type User,
} from 'firebase/auth'
import {
  createContext,
  useState,
  useEffect,
  useMemo,
  useCallback,
  useContext,
  type PropsWithChildren,
} from 'react'
import { firebaseAuth } from '../config/firebase'

type AuthContextValue = {
  user: User | null
  loading: boolean
  signIn: (email: string, password: string) => Promise<void>
  signInWithGoogle: () => Promise<void>
  signOut: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function FirebaseAuthProvider({ children }: PropsWithChildren) {
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

  const signInWithGoogle = useCallback(async () => {
    const provider = new GoogleAuthProvider()
    provider.setCustomParameters({ prompt: 'select_account' })
    await signInWithPopup(firebaseAuth, provider)
  }, [])

  const signOut = useCallback(async () => {
    await firebaseSignOut(firebaseAuth)
  }, [])

  const value = useMemo(
    () => ({ user, loading, signIn, signInWithGoogle, signOut }),
    [user, loading, signIn, signInWithGoogle, signOut],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within FirebaseAuthProvider')
  }
  return context
}
