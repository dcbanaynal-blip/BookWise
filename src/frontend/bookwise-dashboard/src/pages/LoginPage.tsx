import { useEffect, useState, type FormEvent } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'

type LocationState = {
  from?: {
    pathname?: string
  }
}

export function LoginPage() {
  const { signIn, loading, user } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as LocationState | undefined)?.from?.pathname ?? '/'

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  useEffect(() => {
    if (!loading && user) {
      navigate(from, { replace: true })
    }
  }, [user, loading, from, navigate])

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setSubmitting(true)
    setError(null)

    try {
      await signIn(email, password)
      navigate(from, { replace: true })
    } catch (err) {
      const message =
        err instanceof Error ? err.message : 'Unable to sign in. Please try again later.'
      setError(message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-slate-100 px-4">
      <form
        onSubmit={handleSubmit}
        className="w-full max-w-md rounded-2xl bg-white p-8 shadow-xl"
      >
        <h1 className="text-2xl font-bold text-slate-900">Welcome to BookWise</h1>
        <p className="mt-1 text-sm text-slate-500">
          Sign in with your Firebase credentials to continue.
        </p>

        {error && (
          <div className="mt-4 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
            {error}
          </div>
        )}

        <div className="mt-6 space-y-4">
          <div>
            <label className="text-sm font-medium text-slate-700">Email address</label>
            <input
              type="email"
              value={email}
              onChange={event => setEmail(event.target.value)}
              className="mt-1 w-full rounded-xl border border-slate-200 px-3 py-2 text-sm text-slate-900 outline-none ring-primary focus:ring-2"
              required
              autoComplete="email"
            />
          </div>
          <div>
            <label className="text-sm font-medium text-slate-700">Password</label>
            <input
              type="password"
              value={password}
              onChange={event => setPassword(event.target.value)}
              className="mt-1 w-full rounded-xl border border-slate-200 px-3 py-2 text-sm text-slate-900 outline-none ring-primary focus:ring-2"
              required
              autoComplete="current-password"
            />
          </div>
        </div>

        <button
          type="submit"
          disabled={submitting}
          className="mt-6 w-full rounded-xl bg-primary px-4 py-2 text-center text-sm font-semibold text-white transition hover:bg-primary/90 disabled:cursor-not-allowed disabled:bg-primary/70"
        >
          {submitting ? 'Signing in...' : 'Sign in'}
        </button>
      </form>
    </div>
  )
}
