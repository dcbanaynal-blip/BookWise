const apiBaseUrl = import.meta.env.VITE_API_BASE_URL

if (!apiBaseUrl) {
  throw new Error('Missing VITE_API_BASE_URL environment variable for API requests.')
}

export const API_BASE_URL = apiBaseUrl.replace(/\/$/, '')

export type UserProfileResponse = {
  userId: string
  firstName: string
  lastName: string
  email: string
  role: string
  emails: string[]
}

export async function fetchCurrentUser(idToken: string, signal?: AbortSignal) {
  const response = await fetch(`${API_BASE_URL}/api/users/me`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${idToken}`,
    },
    signal,
  })

  if (!response.ok) {
    throw new Error('User is not allowlisted.')
  }

  return (await response.json()) as UserProfileResponse
}
