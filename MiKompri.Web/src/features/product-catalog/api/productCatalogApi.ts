import { responseError } from '../../../shared/api/request'
import { request } from '../../../shared/api/request'
export type CatalogProduct = {
  id: string
  name: string
  purchaseUnit?: string | null
  isActive?: boolean
}

export type CreateCatalogProductInput = { name: string; purchaseUnit: string }

export async function createCatalogProduct(baseUrl: string, token: string, input: CreateCatalogProductInput): Promise<CatalogProduct> {
  const response = await fetch(`${baseUrl}/api/v1/catalog-products`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${token}`, 'Content-Type': 'application/json', Accept: 'application/json' },
    body: JSON.stringify(input),
  })
  if (!response.ok) {
    let message = response.status === 409
      ? 'Ya existe un producto con ese nombre y unidad. Puedes seleccionarlo en tu lista.'
      : 'No fue posible crear el producto. Inténtalo de nuevo.'
    if (response.status === 403) message = 'No tienes permiso para crear productos.'
    if (response.status === 400) {
      try {
        const body = await response.json() as { error?: string; errors?: { message: string }[] | Record<string, string[]> }
        const messages = Array.isArray(body.errors) ? body.errors.map((error) => error.message) : Object.values(body.errors ?? {}).flat()
        message = messages.join(' ') || body.error || 'Revisa el nombre y la unidad de compra.'
      } catch { /* Keep the fallback when the server does not return JSON. */ }
    }
    throw Object.assign(new Error(message), { status: response.status })
  }
  return await response.json() as CatalogProduct
}

export type Market = {
  id: string
  name: string
  locationHint?: string | null
  isActive?: boolean
}

export type ProductPriceRecord = {
  id?: string
  marketId: string
  effectiveDate: string
  priceAmount: number
  marketName?: string
  currency?: string
}

export type ProductPriceHistory = {
  catalogProductId: string
  catalogProductName?: string
  marketId?: string | null
  records: ProductPriceRecord[]
}

async function parseCatalogError(response: Response): Promise<Error & { status?: number }> {
  return responseError(response)
}

export async function getCatalogProducts(productCatalogApiBaseUrl: string, accessToken: string, search?: string): Promise<CatalogProduct[]> {
  const response = await fetch(`${productCatalogApiBaseUrl}/api/v1/catalog-products${search?.trim() ? `?search=${encodeURIComponent(search.trim())}` : ''}`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      Accept: 'application/json',
    },
  })

  if (!response.ok) {
    throw await parseCatalogError(response)
  }

  return (await response.json()) as CatalogProduct[]
}

export async function getMarkets(productCatalogApiBaseUrl: string, accessToken: string, includeInactive = false): Promise<Market[]> {
  const response = await fetch(`${productCatalogApiBaseUrl}/api/v1/markets${includeInactive ? '?includeInactive=true' : ''}`, {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      Accept: 'application/json',
    },
  })

  if (!response.ok) {
    throw await parseCatalogError(response)
  }

  return (await response.json()) as Market[]
}

export async function getProductPriceHistory(
  productCatalogApiBaseUrl: string,
  accessToken: string,
  catalogProductId: string,
  marketId?: string,
  from?: string,
  to?: string,
): Promise<ProductPriceHistory> {
  const url = new URL(`${productCatalogApiBaseUrl}/api/v1/catalog-products/${catalogProductId}/price-history`)
  if (marketId) {
    url.searchParams.set('marketId', marketId)
  }
  if (from) url.searchParams.set('from', from)
  if (to) url.searchParams.set('to', to)

  const response = await fetch(url.toString(), {
    method: 'GET',
    headers: {
      Authorization: `Bearer ${accessToken}`,
      Accept: 'application/json',
    },
  })

  if (!response.ok) {
    throw await parseCatalogError(response)
  }

  return (await response.json()) as ProductPriceHistory
}

export const updateCatalogProduct = (base: string, token: string, id: string, input: CreateCatalogProductInput) => request<CatalogProduct>(`${base}/api/v1/catalog-products/${id}`, token, 'PUT', input)
export const saveMarket = (base: string, token: string, input: { name: string; locationHint?: string | null }, id?: string) => request<Market>(`${base}/api/v1/markets${id ? `/${id}` : ''}`, token, id ? 'PUT' : 'POST', input)
export const deactivateMarket = (base: string, token: string, id: string) => request<void>(`${base}/api/v1/markets/${id}/deactivate`, token, 'PATCH')
export const registerProductPrice = (base: string, token: string, input: { catalogProductId: string; marketId: string; effectiveDate: string; priceAmount: number }) => request<string>(`${base}/api/v1/product-prices`, token, 'POST', input)
