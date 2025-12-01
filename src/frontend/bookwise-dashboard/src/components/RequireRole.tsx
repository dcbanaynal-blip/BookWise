import { PropsWithChildren, ReactNode } from 'react'
import { useAuth } from '@/auth'

type RequireRoleProps = PropsWithChildren<{
  allowedRoles: string[]
  fallback?: ReactNode
}>

export function RequireRole({ allowedRoles, fallback = null, children }: RequireRoleProps) {
  const { loading, profile, hasRole } = useAuth()

  if (loading) {
    return null
  }

  if (profile && hasRole(...allowedRoles)) {
    return <>{children}</>
  }

  return <>{fallback}</>
}
