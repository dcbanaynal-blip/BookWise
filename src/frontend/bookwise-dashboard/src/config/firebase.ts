import { initializeApp, getApps, getApp, type FirebaseOptions } from 'firebase/app'
import { getAuth } from 'firebase/auth'

const firebaseConfig: FirebaseOptions = {
  apiKey: import.meta.env.VITE_FIREBASE_API_KEY,
  authDomain: import.meta.env.VITE_FIREBASE_AUTH_DOMAIN,
  projectId: import.meta.env.VITE_FIREBASE_PROJECT_ID,
  appId: import.meta.env.VITE_FIREBASE_APP_ID,
}

const firebaseApp = getApps().length ? getApp() : initializeApp(firebaseConfig)

export const firebaseAuth = getAuth(firebaseApp)
