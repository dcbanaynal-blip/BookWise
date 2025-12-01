import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useAuth } from '@/auth'
import {
  addUserEmail,
  getAdminUsers,
  inviteUser,
  removeUserEmail,
  updateUserRole,
  type InviteUserRequest,
} from '@/config/api'

export function useAdminUsers() {
  const { user } = useAuth()
  return useQuery({
    queryKey: ['admin-users'],
    enabled: !!user,
    queryFn: async ({ signal }) => {
      const token = await user!.getIdToken()
      return getAdminUsers(token, signal)
    },
  })
}

export function useInviteUserMutation() {
  const queryClient = useQueryClient()
  const { user } = useAuth()

  return useMutation({
    mutationFn: async (payload: InviteUserRequest) => {
      const token = await user!.getIdToken()
      return inviteUser(token, payload)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] })
    },
  })
}

export function useUpdateUserRoleMutation() {
  const queryClient = useQueryClient()
  const { user } = useAuth()

  return useMutation({
    mutationFn: async ({ userId, role }: { userId: string; role: string }) => {
      const token = await user!.getIdToken()
      return updateUserRole(token, userId, role)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] })
    },
  })
}

export function useAddUserEmailMutation() {
  const queryClient = useQueryClient()
  const { user } = useAuth()

  return useMutation({
    mutationFn: async ({ userId, email }: { userId: string; email: string }) => {
      const token = await user!.getIdToken()
      return addUserEmail(token, userId, email)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] })
    },
  })
}

export function useRemoveUserEmailMutation() {
  const queryClient = useQueryClient()
  const { user } = useAuth()

  return useMutation({
    mutationFn: async ({ userId, emailId }: { userId: string; emailId: string }) => {
      const token = await user!.getIdToken()
      return removeUserEmail(token, userId, emailId)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['admin-users'] })
    },
  })
}
