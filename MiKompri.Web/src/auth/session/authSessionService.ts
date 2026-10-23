import type { AccountInfo, IPublicClientApplication } from '@azure/msal-browser'

export async function executeAuthenticatedWrite<T>(
  operation: (token: string) => Promise<T>,
  getAccessToken: () => Promise<string>,
  onUnauthorized: () => Promise<void>,
): Promise<T> {
  const token = await getAccessToken()

  try {
    return await operation(token)
  } catch (error) {
    const status =
      typeof error === 'object' && error !== null && 'status' in error
        ? Number((error as { status: number }).status)
        : undefined

    if (status === 401) {
      await onUnauthorized()
    }

    throw error
  }
}

export function createAuthSessionService(msalInstance: IPublicClientApplication) {
  return {
    async login(scopes: string[]): Promise<void> {
      await msalInstance.loginRedirect({ scopes })
    },

    async logout(account?: AccountInfo): Promise<void> {
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
