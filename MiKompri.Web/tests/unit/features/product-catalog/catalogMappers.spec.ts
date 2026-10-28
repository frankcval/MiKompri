import { describe, expect, it } from 'vitest'

type ApiCatalogProduct = {
  id: string
  name: string
  purchaseUnit?: string | null
}

type CatalogProductView = {
  id: string
  name: string
  unit: string
}

function mapCatalogProduct(input: ApiCatalogProduct): CatalogProductView {
  return {
    id: input.id,
    name: input.name,
    unit: input.purchaseUnit?.trim() || 'N/A',
  }
}

describe('catalog mappers', () => {
  it('mapea purchaseUnit cuando existe', () => {
    const result = mapCatalogProduct({ id: 'p-1', name: 'Arroz', purchaseUnit: 'kg' })
    expect(result.unit).toBe('kg')
  })

  it('usa N/A cuando purchaseUnit no existe', () => {
    const result = mapCatalogProduct({ id: 'p-1', name: 'Arroz' })
    expect(result.unit).toBe('N/A')
  })
})
