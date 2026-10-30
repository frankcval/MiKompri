import type { PropsWithChildren } from 'react'
import { FluentProvider } from '@fluentui/react-components'
import { QueryClientProvider } from '@tanstack/react-query'
import { EventType, PublicClientApplication, type AuthenticationResult } from '@azure/msal-browser'
import { MsalProvider } from '@azure/msal-react'
import { RouterProvider } from 'react-router-dom'
import { appTheme } from '../../shared/ui/theme'
import { queryClient } from './queryClient'
import { onSessionReset, resetPrivateSession } from '../../auth/session/sessionBoundary'
import { clearDrafts } from '../../auth/session/draftStore'
import { msalConfig } from '../../auth/msal/msalConfig'
import { appRouter } from '../router'

onSessionReset(() => { void queryClient.cancelQueries(); queryClient.clear(); clearDrafts() })

const msalInstance = new PublicClientApplication(msalConfig)

msalInstance.addEventCallback((event) => {
  if (event.eventType === EventType.LOGIN_SUCCESS || event.eventType === EventType.ACQUIRE_TOKEN_SUCCESS) {
    const result = event.payload as AuthenticationResult | null
    if (result?.account) {
      const before = msalInstance.getActiveAccount()
      if (before && before.homeAccountId !== result.account.homeAccountId) resetPrivateSession()
      msalInstance.setActiveAccount(result.account)
    }
  }
})

const firstAccount = msalInstance.getAllAccounts()[0]
if (!msalInstance.getActiveAccount() && firstAccount) {
  msalInstance.setActiveAccount(firstAccount)
}

function Providers({ children }: PropsWithChildren) {
  return (
    <FluentProvider theme={appTheme}>
      <MsalProvider instance={msalInstance}>
        <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
      </MsalProvider>
    </FluentProvider>
  )
}

export function AppProviders() {
  return (
    <Providers>
      <RouterProvider router={appRouter} />
    </Providers>
  )
}
