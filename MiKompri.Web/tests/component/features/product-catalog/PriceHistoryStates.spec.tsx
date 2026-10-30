import React from 'react'
import { afterEach, describe, expect, it } from 'vitest'
import { cleanup, render, screen } from '@testing-library/react'
import { PriceHistoryPanel } from '../../../../src/features/product-catalog/components/PriceHistoryPanel'
afterEach(cleanup)

describe('price history', () => {
  it('distinguishes no selection, network error and real empty history', () => {
    const { rerender } = render(<PriceHistoryPanel history={null} selected={false} />)
    expect(screen.getByText(/Selecciona un producto/)).toBeInTheDocument()
    rerender(<PriceHistoryPanel history={null} hasError />)
    expect(screen.getByText(/No fue posible cargar historial/)).toBeInTheDocument()
    expect(screen.queryByText(/Sin historial/)).not.toBeInTheDocument()
    rerender(<PriceHistoryPanel history={{ catalogProductId: 'p', records: [] }} />)
    expect(screen.getByText(/Sin historial/)).toBeInTheDocument()
  })
  it('renders the historic market and currency from the API even when absent from active markets', () => {
    render(<PriceHistoryPanel markets={[]} history={{ catalogProductId: 'p', catalogProductName: 'Pan', records: [{ marketId: 'm', marketName: 'Mercado anterior', effectiveDate: '2026-10-20', priceAmount: 2.5, currency: 'USD' }] }} />)
    expect(screen.getByText(/Mercado anterior/)).toBeInTheDocument()
    expect(screen.getByText(/2,50/)).not.toHaveTextContent('€')
  })
})
