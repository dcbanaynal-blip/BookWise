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
import { fetchCurrentUser, type UserProfileResponse } from '../config/api'
import { firebaseAuth } from '../config/firebase'

type AuthContextValue = {
  user: User | null
  profile: UserProfileResponse | null
  loading: boolean
  accessError: string | null
  hasRole: (...roles: string[]) => boolean
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
  const [profile, setProfile] = useState<UserProfileResponse | null>(null)

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
      setProfile(null)
      return
    }

    let active = true
    const controller = new AbortController()

    const validateAllowlist = async () => {
      setCheckingAccess(true)
      try {
        const token = await user.getIdToken()
        const currentUserProfile = await fetchCurrentUser(token, controller.signal)
        if (active) {
          setAccessError(null)
          setProfile(currentUserProfile)
        }
      } catch (error) {
        if (!active) {
          return
        }
        setAccessError(
          'Your Google account is not authorized for BookWise. Please contact an administrator for access.',
        )
        await firebaseSignOut(firebaseAuth)
        setProfile(null)
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
    setProfile(null)
  }, [])

  const hasRole = useCallback(
    (...roles: string[]) => {
      if (!profile || roles.length === 0) {
        return false
      }
      return roles.includes(profile.role)
    },
    [profile],
  )

  const value = useMemo(
    () => ({
      user,
      profile,
      loading: initializing || checkingAccess,
      accessError,
      hasRole,
      signIn,
      signInWithGoogle,
      signOut,
    }),
    [user, profile, initializing, checkingAccess, accessError, hasRole, signIn, signInWithGoogle, signOut],
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

export function useHasRole(...roles: string[]) {
  const { hasRole } = useAuth()
  return hasRole(...roles)
}
