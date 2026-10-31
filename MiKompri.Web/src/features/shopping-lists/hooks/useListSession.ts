import { currentAccount } from '../../../auth/session/accountIdentity'
import { useMsal } from '@azure/msal-react'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { createAuthSessionService, executeAuthenticatedWrite } from '../../../auth/session/authSessionService'

export function useListSession() {
  const { instance, accounts } = useMsal()
  const account = currentAccount(instance, accounts)
  const session = createAuthSessionService(instance)
  return {
    account,
    token: () => session.getAccessToken(loginRequest.scopes, account),
    write: <T,>(operation: (token: string) => Promise<T>) => executeAuthenticatedWrite(operation,
      () => session.getAccessToken(loginRequest.scopes, account), () => session.login(loginRequest.scopes)),
  }
}
