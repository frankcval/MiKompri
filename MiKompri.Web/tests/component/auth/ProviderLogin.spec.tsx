import React from 'react'
import { afterEach, beforeEach, expect, it, vi } from 'vitest'
import { cleanup, render, screen, fireEvent } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { InteractionStatus } from '@azure/msal-browser'
import { env } from '../../../src/shared/config/env'
import { LoginPage } from '../../../src/features/profile/pages/LoginPage'

const { login } = vi.hoisted(() => ({ login: vi.fn().mockResolvedValue(undefined) }))
vi.mock('@azure/msal-react', () => ({ useIsAuthenticated: () => false,
  useMsal: () => ({ accounts: [], instance: {}, inProgress: InteractionStatus.None }) }))
vi.mock('../../../src/auth/session/authSessionService', () => ({ createAuthSessionService: () => ({ login }) }))
const before = { ...env }
beforeEach(() => { login.mockClear(); Object.assign(env, before); sessionStorage.clear() })
afterEach(() => { cleanup(); Object.assign(env, before) })
it('shows accredited Google first and preserves Microsoft with separate routes', () => {
  Object.assign(env, { entraClientId: 'legacy-spa', ciamEnabled: true, ciamClientId: 'qa-spa',
    ciamAuthority: 'https://qa.ciamlogin.com/qa', ciamTenantId: 'qa', ciamApiScope: 'api://qa/access_as_user' })
  render(<MemoryRouter><LoginPage /></MemoryRouter>)
  const buttons = screen.getAllByRole('button')
  expect(buttons[0]).toHaveTextContent('Continuar con Google')
  expect(buttons[1]).toHaveTextContent('Continuar con Microsoft')
  expect(screen.getByText('En la siguiente pantalla, elige «Usar otra cuenta» si aparece y después «Iniciar sesión con Google».')).toBeInTheDocument()
  fireEvent.click(buttons[0])
  expect(login).toHaveBeenLastCalledWith(expect.any(Array), 'google')
  fireEvent.click(buttons[1])
  expect(login).toHaveBeenLastCalledWith(expect.any(Array), 'microsoft')
  expect(screen.queryByRole('button', { name: /email|Apple|Vincular/i })).not.toBeInTheDocument()
})
it('does not advertise an unconfigured Google route', () => {
  env.ciamEnabled = false
  render(<MemoryRouter><LoginPage /></MemoryRouter>)
  expect(screen.queryByRole('button', { name: 'Continuar con Google' })).not.toBeInTheDocument()
  expect(screen.getByRole('button', { name: 'Continuar con Microsoft' })).toBeInTheDocument()
})
