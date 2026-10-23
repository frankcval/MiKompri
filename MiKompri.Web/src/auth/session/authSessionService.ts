import type { AccountInfo, AuthenticationResult, IPublicClientApplication } from '@azure/msal-browser'

export const AUTH_SESSION_STORAGE_KEY = 'mikompri.auth.session'

export type SessionSnapshot = {
  accountId: string
  username: string
  accessToken: string
  expiresAt: number
}

type StorageLike = Pick<Storage, 'getItem' | 'setItem' | 'removeItem'>

export function createSessionSnapshot(
  accountId: string,
  username: string,
  accessToken: string,
  expiresAt: number,
): SessionSnapshot {
  return { accountId, username, accessToken, expiresAt }
}

export function isSessionRestorable(expiresAt: number, now = Date.now()): boolean {
  return expiresAt > now
}

export function restoreSessionSnapshot(
  storage: StorageLike = sessionStorage,
  key = AUTH_SESSION_STORAGE_KEY,
  now = Date.now(),
): SessionSnapshot | null {
  const raw = storage.getItem(key)
  if (!raw) {
    return null
  }

  try {
    const parsed = JSON.parse(raw) as SessionSnapshot
    if (!parsed?.accessToken || !isSessionRestorable(parsed.expiresAt, now)) {
      storage.removeItem(key)
      return null
    }

    return parsed
  } catch {
    storage.removeItem(key)
    return null
  }
}

export function persistSessionSnapshot(
  snapshot: SessionSnapshot,
  storage: StorageLike = sessionStorage,
  key = AUTH_SESSION_STORAGE_KEY,
): void {
  storage.setItem(key, JSON.stringify(snapshot))
}

export function clearSessionSnapshot(
  storage: StorageLike = sessionStorage,
  key = AUTH_SESSION_STORAGE_KEY,
): void {
  storage.removeItem(key)
}

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

export function createAuthSessionService(
  msalInstance: IPublicClientApplication,
  storage: StorageLike = sessionStorage,
) {
  return {
    async login(scopes: string[]): Promise<void> {
      await msalInstance.loginRedirect({ scopes })
    },

    async logout(account?: AccountInfo): Promise<void> {
      clearSessionSnapshot(storage)
      await msalInstance.logoutRedirect({ account })
    },

    async bootstrapFromRedirect(): Promise<SessionSnapshot | null> {
      const result = await msalInstance.handleRedirectPromise()
      if (!result) {
        const firstAccount = msalInstance.getAllAccounts()[0]
        if (firstAccount) {
          msalInstance.setActiveAccount(firstAccount)
        }

        return restoreSessionSnapshot(storage)
      }

      if (result.account) {
        msalInstance.setActiveAccount(result.account)
      }

      return storeAuthResult(result, storage)
    },

    async getAccessToken(scopes: string[], account?: AccountInfo): Promise<string> {
      const activeAccount = account ?? msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0]
      const tokenResult = await msalInstance.acquireTokenSilent({ scopes, account: activeAccount })
      const snapshot = buildSnapshot(tokenResult)
      persistSessionSnapshot(snapshot, storage)
      return tokenResult.accessToken
    },
  }
}

function storeAuthResult(result: AuthenticationResult, storage: StorageLike): SessionSnapshot {
  const snapshot = buildSnapshot(result)
  persistSessionSnapshot(snapshot, storage)
  return snapshot
}

function buildSnapshot(result: AuthenticationResult): SessionSnapshot {
  const account = result.account
  const accountId = account?.homeAccountId ?? 'unknown-account'
  const username = account?.username ?? ''
  const expiresAt = result.expiresOn?.getTime() ?? Date.now() + 30 * 60_000

  return createSessionSnapshot(accountId, username, result.accessToken, expiresAt)
}
