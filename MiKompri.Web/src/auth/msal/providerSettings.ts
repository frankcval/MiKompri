import { env } from '../../shared/config/env'

export type AuthProvider = 'microsoft' | 'google'
const routeKey = 'mikompri.auth-provider'
export function selectedProvider(): AuthProvider {
  try { return sessionStorage.getItem(routeKey) === 'google' && googleAvailable() ? 'google' : 'microsoft' } catch { return 'microsoft' }
}
export function googleAvailable() {
  return env.ciamEnabled && Boolean(env.ciamClientId && env.ciamAuthority && env.ciamTenantId && env.ciamApiScope)
}
export function getProviderSettings(provider: AuthProvider) {
  if (provider === 'google') {
    if (!googleAvailable()) throw new Error('Google no está disponible en este entorno.')
    return { clientId: env.ciamClientId, tenantId: env.ciamTenantId, authority: env.ciamAuthority, scope: env.ciamApiScope,
      knownAuthorities: [new URL(env.ciamAuthority).hostname, `${env.ciamTenantId}.ciamlogin.com`],
      domainHint: 'google' }
  }
  return { clientId: env.entraClientId, tenantId: env.entraTenantId, authority: env.entraAuthority, scope: env.apiScope,
    knownAuthorities: [] as string[], domainHint: undefined }
}
export function selectProvider(provider: AuthProvider) {
  getProviderSettings(provider)
  sessionStorage.setItem(routeKey, provider)
}
