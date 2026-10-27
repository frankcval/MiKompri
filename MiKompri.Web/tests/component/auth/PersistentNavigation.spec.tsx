import React from 'react'
import { act, cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { MemoryRouter, useRoutes, useNavigate, useLocation } from 'react-router-dom'
import { InteractionStatus } from '@azure/msal-browser'
import { useIsAuthenticated, useMsal } from '@azure/msal-react'
import { appRoutes } from '../../../src/app/router/routes'

vi.mock('@azure/msal-react', () => ({ useIsAuthenticated: vi.fn(), useMsal: vi.fn() }))
vi.mock('../../../src/features/profile/pages/ProfilePage', () => ({ ProfilePage: () => <h1>Perfil de prueba</h1> }))
vi.mock('../../../src/features/profile/pages/LoginPage', () => ({ LoginPage: () => <h1>Acceso de prueba</h1> }))
vi.mock('../../../src/features/groups/pages/GroupsPage', () => ({ GroupsPage: () => <h1>Grupos de prueba</h1> }))
vi.mock('../../../src/features/shopping-lists/pages/PersonalListsPage', () => ({ PersonalListsPage: () => <h1>Personales de prueba</h1> }))
vi.mock('../../../src/features/shared-lists/pages/SharedListsPage', () => ({ SharedListsPage: () => <h1>Compartidas de prueba</h1> }))
vi.mock('../../../src/features/product-catalog/pages/ProductCatalogPage', () => ({ ProductCatalogPage: () => <h1>Catálogo de prueba</h1> }))

const destinations = [
  ['/profile', 'Perfil'], ['/groups', 'Grupos'], ['/personal-lists', 'Listas personales'],
  ['/shared-lists', 'Listas compartidas'], ['/product-catalog', 'Catálogo'],
]

function TestRoutes() {
  const navigate = useNavigate()
  const location = useLocation()
  const content = useRoutes(appRoutes)
  return <><output data-testid="path">{location.pathname}</output><button onClick={() => navigate(-1)}>Atrás</button><button onClick={() => navigate(1)}>Adelante</button>{content}</>
}

beforeEach(() => {
  vi.mocked(useIsAuthenticated).mockReturnValue(true)
  vi.mocked(useMsal).mockReturnValue({ accounts: [], inProgress: InteractionStatus.None } as never)
})
afterEach(cleanup)

describe('navegación autenticada persistente', () => {
  it('mantiene el mismo menú entre todas las opciones y al volver/avanzar', async () => {
    render(<MemoryRouter initialEntries={['/']}><TestRoutes /></MemoryRouter>)
    const menu = screen.getByRole('navigation')
    for (const [path, label] of destinations) {
      fireEvent.click(screen.getByRole('link', { name: label }))
      expect(screen.getByTestId('path')).toHaveTextContent(path)
      expect(screen.getByRole('navigation')).toBe(menu)
      expect(screen.getByRole('link', { name: label })).toHaveAttribute('aria-current', 'page')
      expect(screen.getAllByRole('main')).toHaveLength(1)
    }
    await act(async () => { fireEvent.click(screen.getByRole('button', { name: 'Atrás' })) })
    expect(screen.getByRole('link', { name: 'Listas compartidas' })).toHaveAttribute('aria-current', 'page')
    await act(async () => { fireEvent.click(screen.getByRole('button', { name: 'Adelante' })) })
    expect(screen.getByRole('link', { name: 'Catálogo' })).toHaveAttribute('aria-current', 'page')
    expect(screen.getByRole('navigation')).toBe(menu)
  })

  it.each(destinations)('monta el menú al iniciar directamente en %s', (path, label) => {
    render(<MemoryRouter initialEntries={[path]}><TestRoutes /></MemoryRouter>)
    expect(screen.getByRole('navigation')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: label })).toHaveAttribute('aria-current', 'page')
  })

  it.each(destinations)('protege el acceso directo sin sesión a %s', (path) => {
    vi.mocked(useIsAuthenticated).mockReturnValue(false)
    render(<MemoryRouter initialEntries={[path]}><TestRoutes /></MemoryRouter>)
    expect(screen.getByRole('heading', { name: 'Acceso de prueba' })).toBeInTheDocument()
    expect(screen.queryByRole('navigation')).not.toBeInTheDocument()
  })

  it('espera al bootstrap de MSAL antes de mostrar el layout', () => {
    vi.mocked(useMsal).mockReturnValue({ accounts: [], inProgress: InteractionStatus.Startup } as never)
    render(<MemoryRouter initialEntries={['/groups']}><TestRoutes /></MemoryRouter>)
    expect(screen.queryByRole('navigation')).not.toBeInTheDocument()
  })
})
