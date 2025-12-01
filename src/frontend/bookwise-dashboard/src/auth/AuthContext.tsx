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
import { fetchCurrentUser } from '../config/api'
import { firebaseAuth } from '../config/firebase'

type AuthContextValue = {
  user: User | null
  loading: boolean
  accessError: string | null
  signIn: (email: string, password: string) => Promise<void>
  signInWithGoogle: () => Promise<void>
  signOut: () => Promise<void>
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function FirebaseAuthProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<User | null>(null)
  const [initializing, setInitializing] = useState(true)
  const [checkingAccess, setCheckingAccess] = useState(false)
  const [accessError, setAccessError] = useState<string | null>(null)

  useEffect(() => {
    const unsubscribe = onAuthStateChanged(firebaseAuth, currentUser => {
      setUser(currentUser)
      setInitializing(false)
    })
    return unsubscribe
  }, [])

  useEffect(() => {
    if (!user) {
      setCheckingAccess(false)
      return
    }

    let active = true
    const controller = new AbortController()

    const validateAllowlist = async () => {
      setCheckingAccess(true)
      try {
        const token = await user.getIdToken()
        await fetchCurrentUser(token, controller.signal)
        if (active) {
          setAccessError(null)
        }
      } catch (error) {
        if (!active) {
          return
        }
        setAccessError(
          'Your Google account is not authorized for BookWise. Please contact an administrator for access.',
        )
        await firebaseSignOut(firebaseAuth)
      } finally {
        if (active) {
          setCheckingAccess(false)
        }
      }
    }

    void validateAllowlist()

    return () => {
      active = false
      controller.abort()
    }
  }, [user])

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
    () => ({
      user,
      loading: initializing || checkingAccess,
      accessError,
      signIn,
      signInWithGoogle,
      signOut,
    }),
    [user, initializing, checkingAccess, accessError, signIn, signInWithGoogle, signOut],
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
