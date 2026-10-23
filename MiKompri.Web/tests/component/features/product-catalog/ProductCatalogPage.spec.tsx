import React from 'react'
import { describe, expect, it, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ProductCatalogPage } from '../../../../src/features/product-catalog/pages/ProductCatalogPage'

vi.mock('@azure/msal-react', () => ({
  useMsal: () => ({
    instance: {},
    accounts: [],
  }),
}))

describe('ProductCatalogPage', () => {
  it('muestra estado unauthorized cuando no hay sesión', () => {
    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <ProductCatalogPage />
        </MemoryRouter>
      </QueryClientProvider>,
    )

    expect(screen.getByText(/no autorizado/i)).toBeInTheDocument()
  })
})
