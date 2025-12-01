import type { PropsWithChildren } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../auth'

export function ProtectedRoute({ children }: PropsWithChildren) {
  const { user, loading } = useAuth()
  const location = useLocation()

  if (loading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-blue-gray-50/50">
        <div className="rounded-xl bg-white px-6 py-4 shadow">
          <p className="text-sm font-medium text-blue-gray-500">Checking your session...</p>
        </div>
      </div>
    )
  }

  if (!user) {
    return <Navigate to="/auth/sign-in" state={{ from: location }} replace />
  }

  return children
}
