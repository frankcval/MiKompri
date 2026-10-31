import { PublicClientApplication, type AccountInfo, type IPublicClientApplication } from '@azure/msal-browser'
import { configurationFor } from '../msal/msalConfig'
import { getProviderSettings, selectedProvider, selectProvider, type AuthProvider } from '../msal/providerSettings'
import { announceAccountBoundary, assertCurrentSession, sessionStamp } from './sessionBoundary'
import { clearDrafts } from './draftStore'
import { clearReturnIntent, rememberReturnTo } from './returnIntent'
import { accountMatchesRoute, currentAccount } from './accountIdentity'

export async function executeAuthenticatedWrite<T>(
  operation: (token: string) => Promise<T>,
  getAccessToken: () => Promise<string>,
  onUnauthorized: () => Promise<void>,
): Promise<T> {
  try {
    const token = await getAccessToken()
    return await operation(token)
  } catch (error) {
    const status =
      typeof error === 'object' && error !== null && 'status' in error
        ? Number((error as { status: number }).status)
        : undefined

    const requiresInteraction = error instanceof Error && (error.name === 'InteractionRequiredAuthError' || /interaction_required|login_required|no_tokens_found/.test(error.message))
    if (status === 401 || requiresInteraction) {
      await onUnauthorized()
    }

    throw error
  }
}

export function createAuthSessionService(msalInstance: IPublicClientApplication) {
  return {
    async login(scopes: string[], provider?: AuthProvider): Promise<void> {
      if (typeof window !== 'undefined' && !['/login', '/auth/callback'].includes(window.location.pathname)) rememberReturnTo(window.location.pathname + window.location.search)
      const route = provider ?? selectedProvider()
      const settings = getProviderSettings(route)
      const actual = msalInstance.getConfiguration?.().auth
      const changed = route !== selectedProvider() || Boolean(actual &&
        (actual.clientId !== settings.clientId || actual.authority?.replace(/\/$/, '') !== settings.authority.replace(/\/$/, '')))
      announceAccountBoundary()
      clearDrafts()
      selectProvider(route)
      const client = changed ? new PublicClientApplication(configurationFor(route)) : msalInstance
      if (changed) { await msalInstance.clearCache?.(); await client.initialize(); await client.handleRedirectPromise(); await client.clearCache() }
      await client.loginRedirect({ scopes: provider ? [settings.scope] : scopes,
        ...(settings.domainHint ? { domainHint: settings.domainHint } : {}), prompt: 'select_account' })
    },

    async logout(account?: AccountInfo): Promise<void> {
      announceAccountBoundary()
      clearDrafts()
      clearReturnIntent()
      await msalInstance.clearCache?.()
      // MSAL 3.30 clears all accounts/credentials and temporary MSAL storage here.
      // Reuse that API instead of creating extra clients during logout.
      await msalInstance.logoutRedirect({ account })
    },

    async getAccessToken(scopes: string[], account?: AccountInfo): Promise<string> {
      const cached = msalInstance.getAllAccounts?.() ?? []
      const activeAccount = account ?? currentAccount(msalInstance, cached)
      if (!activeAccount || !accountMatchesRoute(activeAccount)) {
        throw new Error('No hay cuenta activa en MSAL para adquirir token.')
      }

      const stamp = sessionStamp()
      const tokenResult = await msalInstance.acquireTokenSilent({ scopes, account: activeAccount })
      assertCurrentSession(stamp)
      return tokenResult.accessToken
    },
  }
}
