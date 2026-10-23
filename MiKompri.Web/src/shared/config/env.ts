type Env = {
  environment: string
  usersApiBaseUrl: string
  shoppingListApiBaseUrl: string
  productCatalogApiBaseUrl: string
  entraClientId: string
  entraTenantId: string
  entraAuthority: string
  entraRedirectUri: string
  entraPostLogoutRedirectUri: string
  apiAudience: string
}

function required(key: string, value: string | undefined): string {
  if (!value || value.trim().length === 0) {
    throw new Error(`Falta variable de entorno requerida: ${key}`)
  }

  return value
}

export const env: Env = {
  environment: import.meta.env.VITE_ENVIRONMENT ?? 'local',
  usersApiBaseUrl: required('VITE_USERS_API_BASE_URL', import.meta.env.VITE_USERS_API_BASE_URL),
  shoppingListApiBaseUrl: required('VITE_SHOPPINGLIST_API_BASE_URL', import.meta.env.VITE_SHOPPINGLIST_API_BASE_URL),
  productCatalogApiBaseUrl: required('VITE_PRODUCTCATALOG_API_BASE_URL', import.meta.env.VITE_PRODUCTCATALOG_API_BASE_URL),
  entraClientId: required('VITE_ENTRA_CLIENT_ID', import.meta.env.VITE_ENTRA_CLIENT_ID),
  entraTenantId: required('VITE_ENTRA_TENANT_ID', import.meta.env.VITE_ENTRA_TENANT_ID),
  entraAuthority: required('VITE_ENTRA_AUTHORITY', import.meta.env.VITE_ENTRA_AUTHORITY),
  entraRedirectUri: required('VITE_ENTRA_REDIRECT_URI', import.meta.env.VITE_ENTRA_REDIRECT_URI),
  entraPostLogoutRedirectUri: required('VITE_ENTRA_POST_LOGOUT_REDIRECT_URI', import.meta.env.VITE_ENTRA_POST_LOGOUT_REDIRECT_URI),
  apiAudience: required('VITE_API_AUDIENCE', import.meta.env.VITE_API_AUDIENCE),
}
