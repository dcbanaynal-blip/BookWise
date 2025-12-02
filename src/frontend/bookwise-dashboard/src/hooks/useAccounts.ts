import { useAuth } from '@/auth'
import {
  type AccountResponse,
  type AccountTreeResponse,
  type CreateAccountRequest,
  type UpdateAccountRequest,
  createAccount,
  deleteAccount,
  getAccountById,
  getAccounts,
  updateAccount,
} from '@/config/api'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

type UseAccountsOptions = {
  includeTree?: boolean
  search?: string
  enabled?: boolean
}

type AccountsResult = AccountResponse[] | AccountTreeResponse[]

export function useAccountsQuery(options?: UseAccountsOptions) {
  const { user } = useAuth()

  return useQuery<AccountsResult>({
    queryKey: ['accounts', options?.includeTree ?? false, options?.search ?? ''],
    enabled: !!user && (options?.enabled ?? true),
    queryFn: async ({ signal }) => {
      const token = await user!.getIdToken()
      return getAccounts(token, {
        includeTree: options?.includeTree,
        search: options?.search,
        signal,
      })
    },
  })
}

export function useAccountQuery(accountId?: number) {
  const { user } = useAuth()

  return useQuery<AccountResponse>({
    queryKey: ['accounts', accountId],
    enabled: !!user && typeof accountId === 'number',
    queryFn: async ({ signal }) => {
      const token = await user!.getIdToken()
      return getAccountById(token, accountId!, signal)
    },
  })
}

export function useCreateAccountMutation() {
  const queryClient = useQueryClient()
  const { user } = useAuth()

  return useMutation({
    mutationFn: async (payload: CreateAccountRequest) => {
      const token = await user!.getIdToken()
      return createAccount(token, payload)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] })
    },
  })
}

export function useUpdateAccountMutation() {
  const queryClient = useQueryClient()
  const { user } = useAuth()

  return useMutation({
    mutationFn: async ({ accountId, payload }: { accountId: number; payload: UpdateAccountRequest }) => {
      const token = await user!.getIdToken()
      return updateAccount(token, accountId, payload)
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] })
      queryClient.invalidateQueries({ queryKey: ['accounts', variables.accountId] })
    },
  })
}

export function useDeleteAccountMutation() {
  const queryClient = useQueryClient()
  const { user } = useAuth()

  return useMutation({
    mutationFn: async (accountId: number) => {
      const token = await user!.getIdToken()
      await deleteAccount(token, accountId)
    },
    onSuccess: (_, accountId) => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] })
      queryClient.invalidateQueries({ queryKey: ['accounts', accountId] })
    },
  })
}
