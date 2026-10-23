import { Button, Card, CardHeader, Field, Input, Text } from '@fluentui/react-components'
import { useMsal } from '@azure/msal-react'
import { useQuery } from '@tanstack/react-query'
import { useMemo, useState } from 'react'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { createAuthSessionService } from '../../../auth/session/authSessionService'
import { env } from '../../../shared/config/env'
import { getCatalogProducts, getMarkets, getProductPriceHistory } from '../api/productCatalogApi'
import { PriceHistoryPanel } from '../components/PriceHistoryPanel'
import { withViewState } from '../../../shared/ui/states/withViewState'

export function ProductCatalogPage() {
  const { instance, accounts } = useMsal()
  const account = accounts[0]
  const authSession = useMemo(() => createAuthSessionService(instance), [instance])

  const [selectedProductId, setSelectedProductId] = useState('')
  const [selectedMarketId, setSelectedMarketId] = useState('')

  const productsQuery = useQuery({
    queryKey: ['catalog-products', account?.homeAccountId],
    enabled: Boolean(account),
    retry: false,
    queryFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return getCatalogProducts(env.productCatalogApiBaseUrl, token)
    },
  })

  const marketsQuery = useQuery({
    queryKey: ['catalog-markets', account?.homeAccountId],
    enabled: Boolean(account),
    retry: false,
    queryFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return getMarkets(env.productCatalogApiBaseUrl, token)
    },
  })

  const historyQuery = useQuery({
    queryKey: ['catalog-price-history', selectedProductId, selectedMarketId, account?.homeAccountId],
    enabled: Boolean(account && selectedProductId),
    retry: false,
    queryFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return getProductPriceHistory(
        env.productCatalogApiBaseUrl,
        token,
        selectedProductId,
        selectedMarketId || undefined,
      )
    },
  })

  if (!account) {
    return withViewState({ state: 'unauthorized', content: null })
  }

  if (productsQuery.isLoading || marketsQuery.isLoading) {
    return withViewState({ state: 'loading', content: null })
  }

  if (productsQuery.isError || marketsQuery.isError) {
    const status = (productsQuery.error as { status?: number } | undefined)?.status
      ?? (marketsQuery.error as { status?: number } | undefined)?.status

    return withViewState({
      state: status === 503 ? 'serviceUnavailable' : 'error',
      content: null,
      errorTitle: 'No fue posible cargar ProductCatalog',
      errorDescription: status === 503 ? 'El servicio no está disponible temporalmente.' : 'Intenta nuevamente.',
    })
  }

  return withViewState({
    state: 'ready',
    content: (
      <main style={{ padding: '1.5rem', display: 'grid', gap: '1rem' }}>
        <h1>ProductCatalog</h1>

        <Card>
          <CardHeader header={<Text weight="semibold">Productos</Text>} />
          {productsQuery.data?.length ? (
            <section style={{ display: 'grid', gap: '0.5rem' }}>
              {productsQuery.data.map((product) => (
                <div
                  key={product.id}
                  style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '0.75rem' }}
                >
                  <Text>
                    {product.name} ({product.purchaseUnit ?? 'N/A'})
                  </Text>
                  <Button appearance="secondary" onClick={() => setSelectedProductId(product.id)}>
                    Ver historial
                  </Button>
                </div>
              ))}
            </section>
          ) : (
            withViewState({
              state: 'empty',
              content: null,
              emptyTitle: 'Sin productos',
              emptyDescription: 'No hay productos disponibles.',
            })
          )}
        </Card>

        <Card>
          <CardHeader header={<Text weight="semibold">Mercados</Text>} />
          <Field label="Filtrar por MarketId (opcional)">
            <Input value={selectedMarketId} onChange={(_, data) => setSelectedMarketId(data.value)} />
          </Field>
          {marketsQuery.data?.length ? (
            <section style={{ display: 'grid', gap: '0.4rem' }}>
              {marketsQuery.data.map((market) => (
                <Text key={market.id}>
                  {market.name} ({market.id})
                </Text>
              ))}
            </section>
          ) : (
            withViewState({
              state: 'empty',
              content: null,
              emptyTitle: 'Sin mercados',
              emptyDescription: 'No hay mercados disponibles.',
            })
          )}
        </Card>

        <PriceHistoryPanel
          history={historyQuery.data ?? null}
          isLoading={historyQuery.isLoading}
          errorStatus={(historyQuery.error as { status?: number } | undefined)?.status}
        />
      </main>
    ),
  })
}
