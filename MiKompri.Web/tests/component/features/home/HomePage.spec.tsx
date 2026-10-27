import React from 'react'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { InteractionStatus } from '@azure/msal-browser'
import { useIsAuthenticated, useMsal } from '@azure/msal-react'
import App from '../../../../src/app/App'

vi.mock('@azure/msal-react', () => ({ useIsAuthenticated: vi.fn(), useMsal: vi.fn() }))
afterEach(cleanup)
function renderEntry(authenticated: boolean, accounts: unknown[] = [], inProgress = InteractionStatus.None) {
  vi.mocked(useIsAuthenticated).mockReturnValue(authenticated)
  vi.mocked(useMsal).mockReturnValue({ accounts, inProgress } as never)
  render(<MemoryRouter><Routes><Route path="/" element={<App />} /><Route path="/personal-lists" element={<h1>Mis listas reales</h1>} /></Routes></MemoryRouter>)
}
describe('entrada de portada', () => {
  it('ofrece ejemplo público y creación en el flujo existente', () => {
    renderEntry(false)
    expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('La compra, mejor')
    expect(screen.getByRole('region', { name: 'Lista de ejemplo, datos ficticios' })).toBeInTheDocument()
    expect(screen.getAllByRole('link', { name: /Crear mi primera lista/ })[0]).toHaveAttribute('href', '/personal-lists')
  })
  it.each([true, false])('abre listas con sesión o cuentas MSAL (%s)', authenticated => {
    renderEntry(authenticated, authenticated ? [] : [{ homeAccountId: 'account' }])
    expect(screen.getByRole('heading', { name: 'Mis listas reales' })).toBeInTheDocument()
  })
  it('espera la restauración de MSAL sin mostrar una portada transitoria', () => {
    renderEntry(false, [], InteractionStatus.Startup)
    expect(screen.queryByRole('heading')).not.toBeInTheDocument()
    expect(screen.getByText('Cargando sesión')).toBeInTheDocument()
  })
})
