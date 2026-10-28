import React from 'react'
import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { PersonalListsPage } from '../../../../src/features/shopping-lists/pages/PersonalListsPage'

vi.mock('@azure/msal-react', () => ({
  useMsal: () => ({
    instance: {},
    accounts: [],
  }),
}))

describe('PersonalListsPage', () => {
  it('renderiza estado sin sesión cuando no hay cuenta activa', () => {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false } },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <PersonalListsPage />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(screen.getByText(/no hay sesión activa/i)).toBeInTheDocument()
  })
})
