import {
  onAuthStateChanged,
  signInWithEmailAndPassword,
  signInWithPopup,
  GoogleAuthProvider,
  signOut as firebaseSignOut,
  type User,
} from 'firebase/auth'
import { PropsWithChildren, useCallback, useEffect, useMemo, useState } from 'react'
import { fetchCurrentUser, type UserProfileResponse } from '@/config/api'
import { firebaseAuth } from '@/config/firebase'
import { AuthContext } from './context'

export function FirebaseAuthProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<User | null>(null)
  const [profile, setProfile] = useState<UserProfileResponse | null>(null)
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
      } catch {
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

  const value = useMemo(
    () => ({
      user,
      profile,
      loading: initializing || checkingAccess,
      accessError,
      signIn,
      signInWithGoogle,
      signOut,
      hasRole: (...roles: string[]) => Boolean(profile && roles.includes(profile.role)),
    }),
    [user, profile, initializing, checkingAccess, accessError, signIn, signInWithGoogle, signOut],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
