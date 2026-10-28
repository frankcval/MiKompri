// Test-only entry. Never imported by src/main.tsx or included in the production build.
import React from 'react'
import { createRoot } from 'react-dom/client'
import { MemoryRouter, useRoutes, useLocation } from 'react-router-dom'
import { FluentProvider } from '@fluentui/react-components'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MsalContext } from '@azure/msal-react'
import { InteractionStatus, type AccountInfo, type IPublicClientApplication } from '@azure/msal-browser'
import { appTheme } from '../../src/shared/ui/theme'
import { appRoutes } from '../../src/app/router/routes'
import '../../src/index.css'

const account: AccountInfo = { homeAccountId: 'visual-test', localAccountId: 'visual-test', environment: 'test', tenantId: 'test', username: 'visual@example.test', name: 'Persona de prueba' }
const params = new URLSearchParams(location.search)
const route = params.get('route') ?? '/personal-lists'
const anonymous = params.has('anonymous')
const instance = {
  getActiveAccount: () => anonymous ? null : account,
  getAllAccounts: () => anonymous ? [] : [account],
  acquireTokenSilent: async () => ({ accessToken: 'visual-test-token' }),
  getLogger: () => ({ clone: () => ({ verbose: () => {}, info: () => {}, warning: () => {}, error: () => {} }) }),
  loginRedirect: async () => {}, logoutRedirect: async () => {},
} as unknown as IPublicClientApplication
export function Routes() { return useRoutes(appRoutes) }
export function TestLocation() { const current = useLocation(); return <output hidden data-testid="test-route">{current.pathname}{current.search}</output> }
createRoot(document.getElementById('root')!).render(
  <FluentProvider theme={appTheme}><MsalContext.Provider value={{ instance, accounts: anonymous ? [] : [account], inProgress: InteractionStatus.None, logger: instance.getLogger() }}>
    <QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}>
      <MemoryRouter initialEntries={[route]}><TestLocation /><Routes /></MemoryRouter>
    </QueryClientProvider>
  </MsalContext.Provider></FluentProvider>,
)
