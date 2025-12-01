import { useMemo } from 'react'
import { useAuth } from './index'

export function useHasRole(...roles: string[]) {
  const { profile } = useAuth()
  return useMemo(() => Boolean(profile && roles.includes(profile.role)), [profile, roles])
}
