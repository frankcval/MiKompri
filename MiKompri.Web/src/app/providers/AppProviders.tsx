import type { PropsWithChildren } from 'react'
import { FluentProvider } from '@fluentui/react-components'
import { QueryClientProvider } from '@tanstack/react-query'
import { PublicClientApplication } from '@azure/msal-browser'
import { MsalProvider } from '@azure/msal-react'
import { RouterProvider } from 'react-router-dom'
import { appTheme } from '../../shared/ui/theme'
import { queryClient } from './queryClient'
import { msalConfig } from '../../auth/msal/msalConfig'
import { appRouter } from '../router'

const msalInstance = new PublicClientApplication(msalConfig)

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
