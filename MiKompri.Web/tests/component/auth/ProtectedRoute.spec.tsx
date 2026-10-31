import React from 'react'
import { InteractionStatus } from '@azure/msal-browser'
import { fireEvent, render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { MemoryRouter, Route, Routes } from 'react-router-dom'
import { ProtectedRoute } from '../../../src/auth/guards/ProtectedRoute'

vi.mock('@azure/msal-react', () => ({
  useIsAuthenticated: vi.fn(),
  useMsal: vi.fn(),
}))

import { useIsAuthenticated, useMsal } from '@azure/msal-react'

describe('ProtectedRoute', () => {
  it('discards an open private form when the selected account changes', () => {
    function PrivateForm() {
      const [value, setValue] = React.useState('')
      return <input aria-label="Private draft" value={value} onChange={event => setValue(event.target.value)} />
    }
    function Host() {
      return <MemoryRouter><Routes><Route element={<ProtectedRoute />}><Route path="/" element={<PrivateForm />} /></Route></Routes></MemoryRouter>
    }
    vi.mocked(useMsal).mockReturnValue({ accounts: [{ homeAccountId: 'account-a', tenantId: '00000000-0000-0000-0000-000000000000' }], inProgress: InteractionStatus.None } as never)
    const view = render(<Host />)
    fireEvent.change(screen.getByLabelText('Private draft'), { target: { value: 'Private content A' } })
    vi.mocked(useMsal).mockReturnValue({ accounts: [{ homeAccountId: 'account-b', tenantId: '00000000-0000-0000-0000-000000000000' }], inProgress: InteractionStatus.None } as never)
    view.rerender(<Host />)
    expect(screen.getByLabelText('Private draft')).toHaveValue('')
  })
  it('renderiza contenido protegido cuando el usuario está autenticado', () => {
    vi.mocked(useIsAuthenticated).mockReturnValue(true)
    vi.mocked(useMsal).mockReturnValue({ accounts: [{ homeAccountId: 'active-user', tenantId: '00000000-0000-0000-0000-000000000000' }], inProgress: InteractionStatus.None } as never)

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

  it('permite acceso cuando MSAL tiene cuentas aunque useIsAuthenticated esté en false', () => {
    vi.mocked(useIsAuthenticated).mockReturnValue(false)
    vi.mocked(useMsal).mockReturnValue({
      accounts: [{ homeAccountId: 'acc-1', tenantId: '00000000-0000-0000-0000-000000000000' }],
      inProgress: InteractionStatus.None,
    } as never)

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

  it('redirecciona a login cuando no hay autenticación ni cuentas', () => {
    vi.mocked(useIsAuthenticated).mockReturnValue(false)
    vi.mocked(useMsal).mockReturnValue({ accounts: [], inProgress: InteractionStatus.None } as never)

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
