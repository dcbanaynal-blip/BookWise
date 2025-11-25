import { initializeApp, getApps, getApp, type FirebaseOptions } from 'firebase/app'
import { getAuth } from 'firebase/auth'

const requiredEnv = (key: keyof ImportMetaEnv): string => {
  const value = import.meta.env[key]
  if (!value) {
    throw new Error(`Missing Firebase env variable: ${key}`)
  }
  return value
}

const firebaseConfig: FirebaseOptions = {
  apiKey: requiredEnv('VITE_FIREBASE_API_KEY'),
  authDomain: requiredEnv('VITE_FIREBASE_AUTH_DOMAIN'),
  projectId: requiredEnv('VITE_FIREBASE_PROJECT_ID'),
  appId: requiredEnv('VITE_FIREBASE_APP_ID'),
}

const firebaseApp = getApps().length ? getApp() : initializeApp(firebaseConfig)

export const firebaseAuth = getAuth(firebaseApp)
