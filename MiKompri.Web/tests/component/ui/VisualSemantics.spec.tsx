import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, expect, it } from 'vitest'
import { PageHeader } from '../../../src/shared/ui/PageHeader'
import { ErrorState } from '../../../src/shared/ui/states/ErrorState'

afterEach(cleanup)
it('mantiene el nombre del encabezado y oculta el icono a la tecnología asistiva', () => {
  render(<PageHeader icon="products" title="Catálogo de productos" description="Busca tus productos." />)
  const heading = screen.getByRole('heading', { level: 1, name: 'Catálogo de productos' })
  expect(heading.querySelector('svg')).toHaveAttribute('aria-hidden', 'true')
  expect(screen.getByText('Busca tus productos.')).toBeVisible()
})
it('comunica un error mediante texto y rol aunque el icono no sea perceptible', () => {
  render(<ErrorState title="Catálogo no disponible" description="Vuelve a intentarlo." />)
  expect(screen.getByRole('alert')).toHaveTextContent('Catálogo no disponible')
  expect(screen.getByRole('alert')).toHaveTextContent('Vuelve a intentarlo.')
  expect(screen.getByRole('alert').querySelector('svg')).toHaveAttribute('aria-hidden', 'true')
})
