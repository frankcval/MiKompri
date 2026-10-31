import type { AccountInfo, IPublicClientApplication } from '@azure/msal-browser'
import { getProviderSettings, selectedProvider } from '../msal/providerSettings'
import { env } from '../../shared/config/env'

export function currentAccount(instance: IPublicClientApplication, accounts: AccountInfo[]) {
  const active = instance?.getActiveAccount?.()
  if (active && accountMatchesRoute(active)) return active
  const matching = accounts.filter(accountMatchesRoute)
  return matching.length === 1 ? matching[0] : undefined
}
export function accountMatchesRoute(account: AccountInfo) {
  return account.tenantId === getProviderSettings(selectedProvider()).tenantId
}
export function accountSessionKey(account?: AccountInfo) {
  if (!account) return ''
  const provider = selectedProvider()
  return `${env.environment}:${provider}:${getProviderSettings(provider).clientId}:${account.tenantId}:${account.homeAccountId}`
}
