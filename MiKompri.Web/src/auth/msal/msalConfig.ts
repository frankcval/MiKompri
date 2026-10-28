import { LogLevel, type Configuration } from '@azure/msal-browser'
import { env } from '../../shared/config/env'

export const msalConfig: Configuration = {
  auth: {
    clientId: env.entraClientId,
    authority: env.entraAuthority,
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

export const loginRequest = {
  scopes: [env.apiScope],
}
