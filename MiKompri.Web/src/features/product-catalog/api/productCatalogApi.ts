export type CatalogProduct = {
  id: string
  name: string
  purchaseUnit?: string | null
  isActive?: boolean
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
}

export type ProductPriceHistory = {
  catalogProductId: string
  marketId?: string | null
  records: ProductPriceRecord[]
}

async function parseCatalogError(response: Response): Promise<Error & { status?: number }> {
  const error = new Error(`product_catalog_api_error_${response.status}`) as Error & { status?: number }
  error.status = response.status
  return error
}

export async function getCatalogProducts(productCatalogApiBaseUrl: string, accessToken: string): Promise<CatalogProduct[]> {
  const response = await fetch(`${productCatalogApiBaseUrl}/api/v1/catalog-products`, {
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

export async function getMarkets(productCatalogApiBaseUrl: string, accessToken: string): Promise<Market[]> {
  const response = await fetch(`${productCatalogApiBaseUrl}/api/v1/markets`, {
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
): Promise<ProductPriceHistory> {
  const url = new URL(`${productCatalogApiBaseUrl}/api/v1/catalog-products/${catalogProductId}/price-history`)
  if (marketId) {
    url.searchParams.set('marketId', marketId)
  }

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
