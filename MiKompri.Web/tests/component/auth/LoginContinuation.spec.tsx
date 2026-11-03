import React, { StrictMode } from 'react'
import { afterEach, expect, it, vi } from 'vitest'
import { cleanup, render, screen, fireEvent, waitFor } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { InteractionStatus } from '@azure/msal-browser'
import { LoginPage } from '../../../src/features/profile/pages/LoginPage'
import { rememberReturnTo } from '../../../src/auth/session/returnIntent'

vi.mock('@azure/msal-react', () => ({ useIsAuthenticated: () => true, useMsal: () => ({ accounts: [{ homeAccountId: 'login-test', tenantId: '00000000-0000-0000-0000-000000000000' }], instance: {}, inProgress: InteractionStatus.None }) }))
vi.mock('../../../src/auth/session/authSessionService', () => ({ createAuthSessionService: () => ({ getAccessToken: async () => 'test-only' }) }))
const { syncMyProfile } = vi.hoisted(() => ({ syncMyProfile: vi.fn(async () => ({ id: 'test-user' })) }))
vi.mock('../../../src/features/profile/api/profileApi', () => ({ syncMyProfile }))
afterEach(() => { cleanup(); syncMyProfile.mockReset(); syncMyProfile.mockResolvedValue({ id: 'test-user' }) })
it('conserva intención de creación incluso con efectos repetidos de StrictMode', async () => {
  rememberReturnTo('/personal-lists?create=1')
  render(<StrictMode><MemoryRouter initialEntries={['/auth/callback']}><Routes><Route path="/auth/callback" element={<LoginPage />} /><Route path="/personal-lists" element={<h1>Continuar creando</h1>} /><Route path="/lists" element={<h1>Índice</h1>} /></Routes></MemoryRouter></StrictMode>)
  expect(await screen.findByRole('heading', { name: 'Continuar creando' })).toBeInTheDocument()
  expect(screen.queryByRole('heading', { name: 'Índice' })).not.toBeInTheDocument()
})

it('reports an unavailable API and retries the existing session without repeating sign-in', async () => {
  syncMyProfile.mockRejectedValueOnce(Object.assign(new Error('Unavailable'), { status: 503 }))
  render(<StrictMode><MemoryRouter initialEntries={['/auth/callback']}><Routes>
    <Route path="/auth/callback" element={<LoginPage />} />
    <Route path="/lists" element={<h1>Mis listas</h1>} />
  </Routes></MemoryRouter></StrictMode>)
  await waitFor(() => expect(screen.getByRole('status')).toHaveTextContent('El servicio no está disponible temporalmente.'))
  expect(syncMyProfile).toHaveBeenCalledTimes(1)
  fireEvent.click(screen.getByRole('button', { name: 'Reintentar acceso' }))
  expect(await screen.findByRole('heading', { name: 'Mis listas' })).toBeInTheDocument()
  expect(syncMyProfile).toHaveBeenCalledTimes(2)
})

it('keeps a failed bootstrap stopped until the user retries', async () => {
  syncMyProfile.mockRejectedValue(new Error('No hay conexión con el servicio.'))
  const view = render(<MemoryRouter initialEntries={['/auth/callback']}><LoginPage /></MemoryRouter>)
  expect(await screen.findByRole('button', { name: 'Reintentar acceso' })).toBeEnabled()
  view.rerender(<MemoryRouter initialEntries={['/auth/callback']}><LoginPage /></MemoryRouter>)
  expect(syncMyProfile).toHaveBeenCalledTimes(1)
})
