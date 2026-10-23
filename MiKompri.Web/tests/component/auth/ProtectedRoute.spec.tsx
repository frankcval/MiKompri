import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from '../../../src/auth/guards/ProtectedRoute'

vi.mock('@azure/msal-react', () => ({
  useIsAuthenticated: vi.fn(),
}))

import { useIsAuthenticated } from '@azure/msal-react'

describe('ProtectedRoute', () => {
  it('renderiza contenido protegido cuando el usuario está autenticado', () => {
    vi.mocked(useIsAuthenticated).mockReturnValue(true)

    render(
      <MemoryRouter initialEntries={['/']}>
        <Routes>
          <Route element={<ProtectedRoute />}>
            <Route path="/" element={<div>private-page</div>} />
          </Route>
          <Route path="/login" element={<div>login-page</div>} />
        </Routes>
      </MemoryRouter>,
    )

    expect(screen.getByText('private-page')).toBeInTheDocument()
  })

  it('redirecciona a login cuando el usuario no está autenticado', () => {
    vi.mocked(useIsAuthenticated).mockReturnValue(false)

    render(
      <MemoryRouter initialEntries={['/']}>
        <Routes>
          <Route element={<ProtectedRoute />}>
            <Route path="/" element={<div>private-page</div>} />
          </Route>
          <Route path="/login" element={<div>login-page</div>} />
        </Routes>
      </MemoryRouter>,
    )

    expect(screen.getByText('login-page')).toBeInTheDocument()
  })
})
