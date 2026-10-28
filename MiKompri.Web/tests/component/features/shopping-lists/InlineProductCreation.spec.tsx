import React from 'react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { FluentProvider } from '@fluentui/react-components'
import { InlineProductCreation } from '../../../../src/features/shopping-lists/components/InlineProductCreation'
import { appTheme } from '../../../../src/shared/ui/theme'

vi.mock('@azure/msal-react', () => ({ useMsal: () => ({ accounts: [{ homeAccountId: 'inline-test' }], instance: { acquireTokenSilent: async () => ({ accessToken: 'test-token' }) } }) }))
const id = '88888888-8888-4888-8888-888888888888'
let fetchMock: ReturnType<typeof vi.fn>
beforeEach(() => {
  fetchMock = vi.fn(async (_url, init) => init?.method === 'POST' ? Response.json({ id, name: 'Café', purchaseUnit: 'paquete' }, { status: 201 }) : Response.json([]))
  vi.stubGlobal('fetch', fetchMock)
})
afterEach(() => { cleanup(); vi.unstubAllGlobals() })
function mount(onAdd: (value: unknown) => Promise<void>) {
  render(<FluentProvider theme={appTheme}><QueryClientProvider client={new QueryClient()}><InlineProductCreation scope="list-1" onAdd={onAdd} /></QueryClientProvider></FluentProvider>)
  fireEvent.change(screen.getByRole('textbox', { name: 'Nombre del producto' }), { target: { value: 'Café' } })
  fireEvent.change(screen.getByRole('textbox', { name: 'Unidad de compra' }), { target: { value: 'paquete' } })
}
describe('alta e incorporación como dos pasos recuperables', () => {
  it('reutiliza el producto creado tras fallo al incorporarlo', async () => {
    const add = vi.fn().mockRejectedValueOnce(Object.assign(new Error('Servicio no disponible'), { status: 503 })).mockResolvedValue(undefined)
    mount(add)
    fireEvent.click(screen.getByRole('button', { name: 'Crear y añadir', exact: true }))
    await screen.findByRole('alert')
    fireEvent.click(screen.getByRole('button', { name: 'Reintentar incorporación' }))
    await screen.findByText('Café se ha añadido a la lista.')
    expect(fetchMock.mock.calls.filter(([, init]) => init?.method === 'POST')).toHaveLength(1)
    expect(add).toHaveBeenCalledTimes(2)
    expect(add).toHaveBeenLastCalledWith({ productId: id, productName: 'Café', quantity: 1, price: null })
  })
  it('no repite automáticamente una incorporación de resultado incierto', async () => {
    const add = vi.fn().mockRejectedValue(new Error('Respuesta perdida'))
    mount(add)
    fireEvent.click(screen.getByRole('button', { name: 'Crear y añadir', exact: true }))
    await screen.findByRole('alert')
    await waitFor(() => expect(screen.getByRole('button', { name: 'Reintentar incorporación' })).toBeDisabled())
    expect(add).toHaveBeenCalledTimes(1)
    expect(screen.getByRole('button', { name: 'He revisado la lista; permitir otro intento' })).toBeEnabled()
  })
})
