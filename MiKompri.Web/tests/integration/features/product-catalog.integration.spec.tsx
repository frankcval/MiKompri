import { describe, expect, it, vi } from 'vitest'
import { getCatalogProducts, getProductPriceHistory } from '../../../src/features/product-catalog/api/productCatalogApi'

describe('product catalog integration', () => {
  it('consulta catálogo y retorna empty cuando no hay historial', async () => {
    const fetchMock = vi
      .spyOn(globalThis, 'fetch')
      .mockResolvedValueOnce(
        new Response(JSON.stringify([{ id: 'p-1', name: 'Arroz', purchaseUnit: 'kg' }]), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      )
      .mockResolvedValueOnce(
        new Response(JSON.stringify({ catalogProductId: 'p-1', marketId: null, records: [] }), {
          status: 200,
          headers: { 'Content-Type': 'application/json' },
        }),
      )

    const products = await getCatalogProducts('http://localhost:5003', 'token-1')
    const history = await getProductPriceHistory('http://localhost:5003', 'token-1', 'p-1')

    expect(products.length).toBe(1)
    expect(history.records.length).toBe(0)
    fetchMock.mockRestore()
  })

  it('propaga 503 cuando ProductCatalog no está disponible', async () => {
    const fetchMock = vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce(new Response('{}', { status: 503 }))

    await expect(getCatalogProducts('http://localhost:5003', 'token-1')).rejects.toBeDefined()
    fetchMock.mockRestore()
  })
})
