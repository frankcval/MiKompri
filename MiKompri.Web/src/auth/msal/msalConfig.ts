import { LogLevel, type Configuration } from '@azure/msal-browser'
import { getProviderSettings, selectedProvider, type AuthProvider } from './providerSettings'
import { env } from '../../shared/config/env'

export function configurationFor(provider: AuthProvider): Configuration {
 const route = getProviderSettings(provider)
 return {
  auth: {
    clientId: route.clientId,
    authority: route.authority,
    knownAuthorities: route.knownAuthorities,
    redirectUri: env.entraRedirectUri,
    postLogoutRedirectUri: env.entraPostLogoutRedirectUri,
  },
  cache: {
    cacheLocation: 'sessionStorage',
  },
  system: {
    loggerOptions: {
      loggerCallback: () => undefined,
      piiLoggingEnabled: false,
      logLevel: LogLevel.Warning,
    },
  },
}

}

export const msalConfig = configurationFor(selectedProvider())
export const loginRequest = {
  scopes: [getProviderSettings(selectedProvider()).scope],
}
