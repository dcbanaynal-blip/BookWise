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
  isAdmin: boolean
  isActive: boolean
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

export type UserListItemResponse = {
  userId: string
  firstName: string
  lastName: string
  role: string
  createdAt: string
  createdBy: string
  isActive: boolean
  emails: Array<{
    id: string
    email: string
    createdAt: string
    createdBy: string
  }>
}

const authHeaders = async (idToken: string) => ({
  Authorization: `Bearer ${idToken}`,
  'Content-Type': 'application/json',
})

export async function getAdminUsers(idToken: string, signal?: AbortSignal) {
  const response = await fetch(`${API_BASE_URL}/api/admin/users`, {
    method: 'GET',
    headers: await authHeaders(idToken),
    signal,
  })
  if (!response.ok) {
    throw new Error('Failed to load users.')
  }
  return (await response.json()) as UserListItemResponse[]
}

export type InviteUserRequest = {
  firstName: string
  lastName: string
  role: string
  emails: string[]
}

export async function inviteUser(idToken: string, payload: InviteUserRequest) {
  const response = await fetch(`${API_BASE_URL}/api/admin/users`, {
    method: 'POST',
    headers: await authHeaders(idToken),
    body: JSON.stringify(payload),
  })
  if (!response.ok) {
    const detail = await response.text()
    throw new Error(detail || 'Unable to invite user.')
  }
  return (await response.json()) as UserListItemResponse
}

export async function updateUserRole(
  idToken: string,
  userId: string,
  role: string,
) {
  const response = await fetch(`${API_BASE_URL}/api/admin/users/${userId}/role`, {
    method: 'PUT',
    headers: await authHeaders(idToken),
    body: JSON.stringify({ role }),
  })
  if (!response.ok) {
    const detail = await response.text()
    throw new Error(detail || 'Unable to update role.')
  }
  return (await response.json()) as UserListItemResponse
}

export async function addUserEmail(
  idToken: string,
  userId: string,
  email: string,
) {
  const response = await fetch(`${API_BASE_URL}/api/admin/users/${userId}/emails`, {
    method: 'POST',
    headers: await authHeaders(idToken),
    body: JSON.stringify({ email }),
  })
  if (!response.ok) {
    const detail = await response.text()
    throw new Error(detail || 'Unable to add email.')
  }
  return (await response.json()) as UserListItemResponse
}

export async function removeUserEmail(
  idToken: string,
  userId: string,
  emailId: string,
) {
  const response = await fetch(
    `${API_BASE_URL}/api/admin/users/${userId}/emails/${emailId}`,
    {
      method: 'DELETE',
      headers: await authHeaders(idToken),
    },
  )
  if (!response.ok) {
    const detail = await response.text()
    throw new Error(detail || 'Unable to remove email.')
  }
}

export async function updateUserDetails(
  idToken: string,
  userId: string,
  details: { firstName: string; lastName: string },
) {
  const response = await fetch(`${API_BASE_URL}/api/admin/users/${userId}`, {
    method: 'PUT',
    headers: await authHeaders(idToken),
    body: JSON.stringify(details),
  })
  if (!response.ok) {
    const detail = await response.text()
    throw new Error(detail || 'Unable to update user details.')
  }
  return (await response.json()) as UserListItemResponse
}

export async function updateUserStatus(
  idToken: string,
  userId: string,
  isActive: boolean,
) {
  const response = await fetch(`${API_BASE_URL}/api/admin/users/${userId}/status`, {
    method: 'PUT',
    headers: await authHeaders(idToken),
    body: JSON.stringify({ isActive }),
  })
  if (!response.ok) {
    const detail = await response.text()
    throw new Error(detail || 'Unable to update user status.')
  }
  return (await response.json()) as UserListItemResponse
}
