import React from 'react'
import { afterEach, expect, it } from 'vitest'
import { cleanup, render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router-dom'
import { ListOverviewControls } from '../../../src/features/shopping-lists/components/ListOverview'
afterEach(cleanup)
it('uses the owned profile default but respects an explicit URL sort', () => {
  render(<MemoryRouter><ListOverviewControls defaultSort="oldest" /></MemoryRouter>)
  expect(screen.getByRole('combobox', { name: 'Ordenar listas' })).toHaveValue('oldest')
  cleanup()
  render(<MemoryRouter initialEntries={['/?sort=name']}><ListOverviewControls defaultSort="oldest" /></MemoryRouter>)
  expect(screen.getByRole('combobox', { name: 'Ordenar listas' })).toHaveValue('name')
})
