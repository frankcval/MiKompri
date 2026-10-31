import { applyAccountEvent } from '../../auth/session/accountEvents'
import type { PropsWithChildren } from 'react'
import { FluentProvider } from '@fluentui/react-components'
import { QueryClientProvider } from '@tanstack/react-query'
import { PublicClientApplication } from '@azure/msal-browser'
import { MsalProvider } from '@azure/msal-react'
import { RouterProvider } from 'react-router-dom'
import { appTheme } from '../../shared/ui/theme'
import { queryClient } from './queryClient'
import { onSessionReset } from '../../auth/session/sessionBoundary'
import { clearDrafts } from '../../auth/session/draftStore'
import { msalConfig } from '../../auth/msal/msalConfig'
import { accountMatchesRoute } from '../../auth/session/accountIdentity'
import { appRouter } from '../router'

onSessionReset(() => { void queryClient.cancelQueries(); queryClient.clear(); clearDrafts() })

const msalInstance = new PublicClientApplication(msalConfig)

msalInstance.addEventCallback(event => applyAccountEvent(msalInstance, event))

const cachedAccounts = msalInstance.getAllAccounts().filter(accountMatchesRoute)
const firstAccount = cachedAccounts.length === 1 ? cachedAccounts[0] : undefined
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
