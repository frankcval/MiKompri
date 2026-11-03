import React from 'react'
import '../../../setup/jsdom-dialogs'
import { cleanup, render, screen, within, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { FluentProvider } from '@fluentui/react-components'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { MemoryRouter } from 'react-router-dom'
import { afterEach, beforeEach, expect, it, vi } from 'vitest'
import { ProductCatalogPage } from '../../../../src/features/product-catalog/pages/ProductCatalogPage'
import { appTheme } from '../../../../src/shared/ui/theme'
vi.mock('@azure/msal-react', () => ({ useMsal: () => ({ accounts: [{ homeAccountId: 'catalog-qa', tenantId: '00000000-0000-0000-0000-000000000000' }], instance: { acquireTokenSilent: async () => ({ accessToken: 'synthetic' }) } }) }))
beforeEach(() => vi.stubGlobal('fetch', vi.fn(async (url: string) => {
  if (url.includes('price-history')) return Response.json({ catalogProductId: 'product', catalogProductName: 'Leche', records: [] })
  if (url.includes('catalog-products')) return Response.json([{ id: 'product', name: 'Leche', purchaseUnit: 'litro' }])
  if (url.includes('markets')) return Response.json([{ id: 'market', name: 'Mercado QA', isActive: true }])
  return Response.json([])
})))
afterEach(() => { cleanup(); vi.unstubAllGlobals() })
function mount() { render(<FluentProvider theme={appTheme}><QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}><MemoryRouter><ProductCatalogPage /></MemoryRouter></QueryClientProvider></FluentProvider>) }
it('prioritizes products and exposes history filters only for a selected product', async () => {
  const user = userEvent.setup(); mount()
  await screen.findByText('Leche (litro)')
  expect(screen.queryByRole('textbox', { name: 'Nombre del producto' })).not.toBeInTheDocument()
  expect(screen.queryByLabelText('Desde')).not.toBeInTheDocument()
  expect(screen.queryByRole('form', { name: 'Crear mercado' })).not.toBeInTheDocument()
  await user.click(screen.getByRole('button', { name: 'Ver historial' }))
  expect(await screen.findByRole('region', { name: 'Historial de precios de Leche' })).toBeVisible()
  expect(screen.getByLabelText('Desde')).toBeVisible()
  await user.click(screen.getByRole('tab', { name: 'Mercados' }))
  expect(await screen.findByText('Mercado QA')).toBeVisible()
  expect(screen.queryByLabelText('Buscar en el catálogo')).not.toBeInTheDocument()
  expect(screen.queryByRole('form', { name: 'Crear mercado' })).not.toBeInTheDocument()
  await user.click(screen.getByRole('button', { name: 'Añadir mercado' }))
  const dialog = screen.getByRole('dialog', { name: 'Añadir mercado' })
  expect(within(dialog).getByLabelText('Ubicación (opcional)')).toBeVisible()
  await user.click(within(dialog).getByRole('button', { name: 'Cancelar' }))
  await waitFor(() => expect(screen.getByRole('button', { name: 'Añadir mercado' })).toHaveFocus())
})
it('edits a product in a focused dialog without expanding its row', async () => {
  const user = userEvent.setup(); mount(); await screen.findByText('Leche (litro)')
  const trigger = screen.getByRole('button', { name: 'Editar producto Leche' })
  await user.click(trigger)
  const dialog = screen.getByRole('dialog', { name: 'Editar producto Leche' })
  expect(within(dialog).getByRole('textbox', { name: 'Nombre del producto' })).toHaveValue('Leche')
  expect(within(dialog).getByRole('textbox', { name: 'Unidad de compra' })).toHaveValue('litro')
  await user.keyboard('{Escape}')
  await waitFor(() => expect(trigger).toHaveFocus())
})
