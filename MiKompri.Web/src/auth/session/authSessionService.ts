import type { AccountInfo, IPublicClientApplication } from '@azure/msal-browser'
import { clearDrafts } from './draftStore'
import { clearReturnIntent, rememberReturnTo } from './returnIntent'

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
    async login(scopes: string[]): Promise<void> {
      if (typeof window !== 'undefined' && !['/login', '/auth/callback'].includes(window.location.pathname)) rememberReturnTo(window.location.pathname + window.location.search)
      await msalInstance.loginRedirect({ scopes })
    },

    async logout(account?: AccountInfo): Promise<void> {
      clearDrafts()
      clearReturnIntent()
      await msalInstance.logoutRedirect({ account })
    },

    async getAccessToken(scopes: string[], account?: AccountInfo): Promise<string> {
      const activeAccount = account ?? msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0]
      if (!activeAccount) {
        throw new Error('No hay cuenta activa en MSAL para adquirir token.')
      }

      const tokenResult = await msalInstance.acquireTokenSilent({ scopes, account: activeAccount })
      return tokenResult.accessToken
    },
  }
}
