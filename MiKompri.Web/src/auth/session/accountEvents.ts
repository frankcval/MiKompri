import { EventType, type AuthenticationResult, type EventMessage, type IPublicClientApplication } from '@azure/msal-browser'
import { resetPrivateSession } from './sessionBoundary'
import { accountMatchesRoute } from './accountIdentity'

let loginError = ''
const listeners = new Set<() => void>()
export const getLoginError = () => loginError
export function onLoginErrorChanged(listener: () => void) {
  listeners.add(listener)
  return () => { listeners.delete(listener) }
}
function reportLoginError(message: string) {
  loginError = message
  for (const listener of [...listeners]) listener()
}

export function applyAccountEvent(instance: IPublicClientApplication, event: EventMessage) {
  if (event.eventType === EventType.LOGIN_START || event.eventType === EventType.LOGIN_SUCCESS) reportLoginError('')
  if (event.eventType === EventType.LOGIN_FAILURE) {
    const code = event.error && 'errorCode' in event.error ? String(event.error.errorCode) : ''
    reportLoginError(/user_cancelled|access_denied|AADB2C90091/i.test(code)
      ? 'Has cancelado el acceso. Puedes volver a intentarlo.'
      : 'No se pudo completar el acceso del proveedor. Vuelve a intentarlo.')
    return
  }
  // A late silent acquisition for A must never replace an explicit login as B.
  if (event.eventType !== EventType.LOGIN_SUCCESS) return
  const result = event.payload as AuthenticationResult | null
  if (!result?.account) return
  if (!accountMatchesRoute(result.account)) {
    reportLoginError('La cuenta no corresponde al acceso elegido. Vuelve a iniciar sesión.')
    return
  }
  const before = instance.getActiveAccount()
  if (before && before.homeAccountId !== result.account.homeAccountId) resetPrivateSession()
  instance.setActiveAccount(result.account)
}
