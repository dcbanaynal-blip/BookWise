import { describe, expect, it } from 'vitest'
import { type ReactNode } from 'react'
import { render, screen } from '@testing-library/react'
import { RequireRole } from '../RequireRole'
import { AuthContext, type AuthContextValue } from '@/auth/context'

const baseContext: AuthContextValue = {
  user: null,
  profile: null,
  loading: false,
  accessError: null,
  signIn: async () => {},
  signInWithGoogle: async () => {},
  signOut: async () => {},
  hasRole: () => false,
}

const renderWithAuth = (ctx: Partial<AuthContextValue>, ui: ReactNode) =>
  render(<AuthContext.Provider value={{ ...baseContext, ...ctx }}>{ui}</AuthContext.Provider>)

describe('RequireRole', () => {
  it('renders children when user has required role', () => {
    renderWithAuth(
      {
        profile: {
          userId: '1',
          firstName: 'Admin',
          lastName: 'User',
          email: 'admin@example.com',
          role: 'Admin',
          isAdmin: true,
          isActive: true,
          emails: [],
        },
        hasRole: () => true,
      },
      <RequireRole allowedRoles={['Admin']}>
        <span>Visible</span>
      </RequireRole>,
    )

    expect(screen.getByText('Visible')).toBeInTheDocument()
  })

  it('renders fallback when role is not permitted', () => {
    renderWithAuth(
      {
        profile: {
          userId: '2',
          firstName: 'Viewer',
          lastName: 'User',
          email: 'viewer@example.com',
          role: 'Viewer',
          isAdmin: false,
          isActive: true,
          emails: [],
        },
      },
      <RequireRole allowedRoles={['Admin']} fallback={<span>Denied</span>}>
        <span>Hidden</span>
      </RequireRole>,
    )

    expect(screen.getByText('Denied')).toBeInTheDocument()
    expect(screen.queryByText('Hidden')).not.toBeInTheDocument()
  })
})
