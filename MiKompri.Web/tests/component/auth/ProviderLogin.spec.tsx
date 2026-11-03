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
it('offers only Google without exposing Microsoft or account linking', () => {
  Object.assign(env, { entraClientId: 'legacy-spa', ciamEnabled: true, ciamClientId: 'qa-spa',
    ciamAuthority: 'https://qa.ciamlogin.com/qa', ciamTenantId: 'qa', ciamApiScope: 'api://qa/access_as_user' })
  render(<MemoryRouter><LoginPage /></MemoryRouter>)
  const buttons = screen.getAllByRole('button')
  expect(buttons[0]).toHaveTextContent('Continuar con Google')
  expect(screen.queryByRole('button', { name: 'Continuar con Microsoft' })).not.toBeInTheDocument()
  expect(screen.getByText('En la siguiente pantalla, pulsa «Iniciar sesión con Google» para continuar con Google.')).toBeInTheDocument()
  fireEvent.click(buttons[0])
  expect(login).toHaveBeenLastCalledWith(expect.any(Array), 'google')
  expect(screen.queryByRole('button', { name: /email|Apple|Vincular/i })).not.toBeInTheDocument()
})
it('does not advertise an unconfigured Google route', () => {
  env.ciamEnabled = false
  render(<MemoryRouter><LoginPage /></MemoryRouter>)
  expect(screen.queryByRole('button', { name: 'Continuar con Google' })).not.toBeInTheDocument()
  expect(screen.queryByRole('button', { name: 'Continuar con Microsoft' })).not.toBeInTheDocument()
  expect(screen.getByRole('status')).toHaveTextContent('El acceso con Google no está disponible en este entorno.')
})
