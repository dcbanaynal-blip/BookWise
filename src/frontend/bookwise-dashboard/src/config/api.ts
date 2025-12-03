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

export type AccountResponse = {
  accountId: number
  externalAccountNumber: string | null
  name: string
  segmentCode: string
  fullSegmentCode: string
  level: number
  type: string
  parentAccountId: number | null
  hasChildren: boolean
}

export type AccountTreeResponse = AccountResponse & {
  children: AccountTreeResponse[]
}

export type AccountTypeValue = 0 | 1 | 2 | 3 | 4

export type CreateAccountRequest = {
  externalAccountNumber?: string | null
  name: string
  segmentCode: string
  type: AccountTypeValue
  parentAccountId?: number | null
}

export type UpdateAccountRequest = {
  name: string
  segmentCode: string
  type: AccountTypeValue
}

type GetAccountsOptions = {
  includeTree?: boolean
  search?: string
  signal?: AbortSignal
}

export async function getAccounts(idToken: string, options?: GetAccountsOptions) {
  const params = new URLSearchParams()
  const includeTree = options?.includeTree ?? false
  if (includeTree) {
    params.set('includeTree', 'true')
  }
  if (options?.search) {
    if (includeTree) {
      throw new Error('Search cannot be combined with tree mode.')
    }
    params.set('search', options.search)
  }

  const response = await fetch(
    `${API_BASE_URL}/api/accounts${params.toString() ? `?${params.toString()}` : ''}`,
    {
      method: 'GET',
      headers: await authHeaders(idToken),
      signal: options?.signal,
    },
  )

  if (!response.ok) {
    const detail = await response.text()
    throw new Error(detail || 'Failed to load accounts.')
  }

  if (includeTree) {
    return (await response.json()) as AccountTreeResponse[]
  }

  return (await response.json()) as AccountResponse[]
}

export async function getAccountById(idToken: string, accountId: number, signal?: AbortSignal) {
  const response = await fetch(`${API_BASE_URL}/api/accounts/${accountId}`, {
    method: 'GET',
    headers: await authHeaders(idToken),
    signal,
  })

  if (!response.ok) {
    const detail = await response.text()
    throw new Error(detail || 'Failed to load account.')
  }

  return (await response.json()) as AccountResponse
}

export async function createAccount(idToken: string, payload: CreateAccountRequest) {
  const response = await fetch(`${API_BASE_URL}/api/accounts`, {
    method: 'POST',
    headers: await authHeaders(idToken),
    body: JSON.stringify(payload),
  })
  if (!response.ok) {
    const detail = await response.text()
    throw new Error(detail || 'Unable to create account.')
  }
  return (await response.json()) as AccountResponse
}

export async function updateAccount(
  idToken: string,
  accountId: number,
  payload: UpdateAccountRequest,
) {
  const response = await fetch(`${API_BASE_URL}/api/accounts/${accountId}`, {
    method: 'PUT',
    headers: await authHeaders(idToken),
    body: JSON.stringify(payload),
  })
  if (!response.ok) {
    const detail = await response.text()
    throw new Error(detail || 'Unable to update account.')
  }
  return (await response.json()) as AccountResponse
}

export async function deleteAccount(idToken: string, accountId: number) {
  const response = await fetch(`${API_BASE_URL}/api/accounts/${accountId}`, {
    method: 'DELETE',
    headers: await authHeaders(idToken),
  })
  if (!response.ok) {
    const detail = await response.text()
    throw new Error(detail || 'Unable to delete account.')
  }
}
