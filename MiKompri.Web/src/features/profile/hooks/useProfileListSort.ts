import { useQuery } from '@tanstack/react-query'
import type { AccountInfo } from '@azure/msal-browser'
import { accountSessionKey } from '../../../auth/session/accountIdentity'
import { env } from '../../../shared/config/env'
import { fetchMyProfile } from '../api/profileApi'

export function useProfileListSort(account: AccountInfo | undefined, token: () => Promise<string>, enabled = true) {
  const profile = useQuery({ queryKey: ['profile', accountSessionKey(account)], enabled: Boolean(account) && enabled, retry: false,
    queryFn: async () => fetchMyProfile(env.usersApiBaseUrl, await token()) })
  const sort = profile.data?.defaultListSort
  return sort && ['newest', 'oldest', 'name', 'updated'].includes(sort) ? sort : 'newest'
}
