type Env = {
  ciamEnabled: boolean
  ciamClientId: string
  ciamTenantId: string
  ciamAuthority: string
  ciamApiScope: string
  environment: string
  usersApiBaseUrl: string
  shoppingListApiBaseUrl: string
  productCatalogApiBaseUrl: string
  entraClientId: string
  entraTenantId: string
  entraAuthority: string
  entraRedirectUri: string
  entraPostLogoutRedirectUri: string
  apiScope: string
}

const missingEnvKeys: string[] = []

function getEnv(key: string, value: string | undefined, fallback: string): string {
  if (value && value.trim().length > 0) {
    return value
  }

  missingEnvKeys.push(key)
  return fallback
}

const localhostOrigin = typeof window === 'undefined' ? 'http://localhost:5173' : window.location.origin
const entraTenantId = getEnv('VITE_ENTRA_TENANT_ID', import.meta.env.VITE_ENTRA_TENANT_ID, 'a6b72a33-ade4-45d3-a2a7-52b88d96f5eb')

export const env: Env = {
  ciamEnabled: import.meta.env.VITE_CIAM_ENABLED === 'true',
  ciamClientId: import.meta.env.VITE_CIAM_CLIENT_ID ?? '',
  ciamTenantId: import.meta.env.VITE_CIAM_TENANT_ID ?? '',
  ciamAuthority: import.meta.env.VITE_CIAM_AUTHORITY ?? '',
  ciamApiScope: import.meta.env.VITE_CIAM_API_SCOPE ?? '',
  environment: import.meta.env.VITE_ENVIRONMENT ?? 'local',
  usersApiBaseUrl: getEnv('VITE_USERS_API_BASE_URL', import.meta.env.VITE_USERS_API_BASE_URL, 'http://localhost:8082'),
  shoppingListApiBaseUrl: getEnv(
    'VITE_SHOPPINGLIST_API_BASE_URL',
    import.meta.env.VITE_SHOPPINGLIST_API_BASE_URL,
    'http://localhost:8080',
  ),
  productCatalogApiBaseUrl: getEnv(
    'VITE_PRODUCTCATALOG_API_BASE_URL',
    import.meta.env.VITE_PRODUCTCATALOG_API_BASE_URL,
    'http://localhost:8083',
  ),
  entraClientId: getEnv('VITE_ENTRA_CLIENT_ID', import.meta.env.VITE_ENTRA_CLIENT_ID, '00000000-0000-0000-0000-000000000000'),
  entraTenantId,
  entraAuthority: getEnv(
    'VITE_ENTRA_AUTHORITY',
    import.meta.env.VITE_ENTRA_AUTHORITY,
    `https://login.microsoftonline.com/${entraTenantId}/v2.0`,
  ),
  entraRedirectUri: getEnv('VITE_ENTRA_REDIRECT_URI', import.meta.env.VITE_ENTRA_REDIRECT_URI, `${localhostOrigin}/auth/callback`),
  entraPostLogoutRedirectUri: getEnv(
    'VITE_ENTRA_POST_LOGOUT_REDIRECT_URI',
    import.meta.env.VITE_ENTRA_POST_LOGOUT_REDIRECT_URI,
    localhostOrigin,
  ),
  apiScope: getEnv(
    'VITE_API_SCOPE',
    import.meta.env.VITE_API_SCOPE,
    'api://36adb79e-9563-4642-8bc9-455358d76273/access_as_user',
  ),
}

if (missingEnvKeys.length > 0) {
  console.warn(
    `[MiKompri.Web] Variables de entorno no definidas: ${missingEnvKeys.join(', ')}. Se aplicaron valores por defecto locales.`,
  )
}
