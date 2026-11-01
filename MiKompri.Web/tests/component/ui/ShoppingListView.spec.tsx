import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, expect, it, vi } from 'vitest'
import { ShoppingListView } from '../../../src/shared/ui/ShoppingListView'

afterEach(cleanup)
it('conserva checkboxes, foco y estados textuales en una compra de cien productos', () => {
  const toggle = vi.fn()
  const items = Array.from({ length: 100 }, (_, i) => ({ id: String(i), productName: `Producto ${i}`, quantity: 1, productPrice: null, isPurchased: i === 0 }))
  const { rerender } = render(<ShoppingListView items={items} onToggle={toggle} />)
  expect(screen.getAllByRole('checkbox')).toHaveLength(100)
  expect(screen.getByRole('status')).toHaveTextContent('1 de 100 comprados')
  const first = screen.getByRole('checkbox', { name: 'Desmarcar comprado: Producto 0' })
  fireEvent.click(first)
  expect(first).toHaveFocus(); expect(toggle).toHaveBeenLastCalledWith('0', false)
  items[0].isPurchased = false
  rerender(<ShoppingListView items={items} onToggle={toggle} pendingIds={['0']} />)
  expect(screen.getByRole('checkbox', { name: 'Marcar comprado: Producto 0' })).toBe(first)
  fireEvent.click(first)
  expect(toggle).toHaveBeenCalledOnce()
  expect(screen.getByText(/100 productos sin precio/)).toBeInTheDocument()
})
