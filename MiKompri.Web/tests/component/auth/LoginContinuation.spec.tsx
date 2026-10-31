import React, { StrictMode } from 'react'
import { afterEach, expect, it, vi } from 'vitest'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { InteractionStatus } from '@azure/msal-browser'
import { LoginPage } from '../../../src/features/profile/pages/LoginPage'
import { rememberReturnTo } from '../../../src/auth/session/returnIntent'

vi.mock('@azure/msal-react', () => ({ useIsAuthenticated: () => true, useMsal: () => ({ accounts: [{ homeAccountId: 'login-test', tenantId: '00000000-0000-0000-0000-000000000000' }], instance: {}, inProgress: InteractionStatus.None }) }))
vi.mock('../../../src/auth/session/authSessionService', () => ({ createAuthSessionService: () => ({ getAccessToken: async () => 'test-only' }) }))
vi.mock('../../../src/features/profile/api/profileApi', () => ({ syncMyProfile: async () => ({ id: 'test-user' }) }))
afterEach(cleanup)
it('conserva intención de creación incluso con efectos repetidos de StrictMode', async () => {
  rememberReturnTo('/personal-lists?create=1')
  render(<StrictMode><MemoryRouter initialEntries={['/auth/callback']}><Routes><Route path="/auth/callback" element={<LoginPage />} /><Route path="/personal-lists" element={<h1>Continuar creando</h1>} /><Route path="/lists" element={<h1>Índice</h1>} /></Routes></MemoryRouter></StrictMode>)
  expect(await screen.findByRole('heading', { name: 'Continuar creando' })).toBeInTheDocument()
  expect(screen.queryByRole('heading', { name: 'Índice' })).not.toBeInTheDocument()
})
