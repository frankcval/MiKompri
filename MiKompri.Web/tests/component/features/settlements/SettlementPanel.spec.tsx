import React from 'react'
import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { SettlementPage } from '../../../../src/features/settlements/pages/SettlementPage'

vi.mock('@azure/msal-react', () => ({
  useMsal: () => ({
    instance: {},
    accounts: [],
  }),
}))

describe('SettlementPanel', () => {
  it('muestra estado sin sesión', () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <SettlementPage />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(screen.getByText(/no hay sesión activa/i)).toBeInTheDocument()
  })
})
