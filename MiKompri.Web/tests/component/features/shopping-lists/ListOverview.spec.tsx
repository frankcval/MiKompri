import { cleanup, fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, expect, it } from 'vitest'
import { MemoryRouter, useLocation } from 'react-router-dom'
import { ListOverviewControls, EstimatedTotal } from '../../../../src/features/shopping-lists/components/ListOverview'

afterEach(cleanup)
function Location() { return <output data-testid="location">{useLocation().search}</output> }
it('hace visible Historial y conserva filtro y orden al cambiar de vista', () => {
  render(<MemoryRouter initialEntries={['/lists?filter=shared&sort=oldest']}><ListOverviewControls /><Location /></MemoryRouter>)
  const views = screen.getByRole('navigation', { name: 'Mostrar listas' })
  expect(within(views).getByRole('link', { name: 'Activas' })).toHaveAttribute('aria-current', 'page')
  fireEvent.click(within(views).getByRole('link', { name: 'Historial' }))
  expect(screen.getByTestId('location')).toHaveTextContent('filter=shared&sort=oldest&view=history')
  expect(within(views).getByRole('link', { name: 'Historial' })).toHaveAttribute('aria-current', 'page')
  fireEvent.change(screen.getByLabelText('Ordenar listas'), { target: { value: 'name' } })
  expect(screen.getByTestId('location')).toHaveTextContent('filter=shared&sort=name&view=history')
})
it('conserva la advertencia textual de estimación parcial sin convertir precio desconocido en cero conocido', () => {
  render(<EstimatedTotal items={[{ quantity: 2, productPrice: 3 }, { quantity: 1, productPrice: null }]} />)
  expect(screen.getByText(/Estimación parcial/)).toHaveTextContent('1 producto sin precio')
  expect(screen.getByText(/6,00/)).toBeInTheDocument()
})
