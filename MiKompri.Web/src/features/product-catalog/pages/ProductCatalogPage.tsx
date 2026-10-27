import { PageHeader } from '../../../shared/ui/PageHeader'
import ui from '../../../shared/ui/AppSurface.module.css'
import { Button, Card, CardHeader, Field, Select, Text } from '@fluentui/react-components'
import { useMsal } from '@azure/msal-react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useSearchParams } from 'react-router-dom'
import { useMemo, useState } from 'react'
import { loginRequest } from '../../../auth/msal/msalConfig'
import { createAuthSessionService, executeAuthenticatedWrite } from '../../../auth/session/authSessionService'
import { env } from '../../../shared/config/env'
import { createCatalogProduct, getCatalogProducts, getMarkets, getProductPriceHistory } from '../api/productCatalogApi'
import { CreateCatalogProductForm } from '../components/CreateCatalogProductForm'
import { PriceHistoryPanel } from '../components/PriceHistoryPanel'
import { withViewState } from '../../../shared/ui/states/withViewState'

export function ProductCatalogPage() {
  return <section className={ui.page}><PageHeader title="Catálogo de productos" description="Los productos de tus listas, sus unidades y el historial de precios." /><ProductCatalogContent /></section>
}

function ProductCatalogContent() {
  const { instance, accounts } = useMsal()
  const account = accounts[0]
  const queryClient = useQueryClient()
  const [searchParams] = useSearchParams()
  const requestedReturn = searchParams.get('returnTo') ?? ''
  const returnTo = /^\/(personal-lists|shared-lists)\/[0-9a-f-]{36}(\?tab=(shopping|expenses))?$/i.test(requestedReturn) ? requestedReturn : null
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
      <section className={ui.stack}>
        {returnTo ? <Link to={returnTo}>Volver a la lista</Link> : null}

        <Card className={ui.panel}>
          <CardHeader header={<h2 style={{ margin: 0 }}>Crear producto</h2>} />
          <CreateCatalogProductForm key={account.homeAccountId} onCreate={async (input) => {
            const product = await executeAuthenticatedWrite(
              (token) => createCatalogProduct(env.productCatalogApiBaseUrl, token, input),
              () => authSession.getAccessToken(loginRequest.scopes, account),
              async () => authSession.login(loginRequest.scopes),
            )
            await queryClient.invalidateQueries({ queryKey: ['catalog-products', account.homeAccountId] })
            return product
          }} />
        </Card>

        <Card className={ui.panel}>
          <CardHeader header={<h2>Productos</h2>} />
          {productsQuery.data?.length ? (
            <section className={ui.stack}>
              {productsQuery.data.map((product) => (
                <div
                  key={product.id}
                  className={ui.row}
                >
                  <Text style={{ minWidth: 0, overflowWrap: 'anywhere' }}>
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

        <Card className={ui.panel}>
          <CardHeader header={<h2>Mercados</h2>} />
          <Field label="Filtrar por mercado (opcional)">
            <Select value={selectedMarketId} onChange={(_, data) => setSelectedMarketId(data.value)}>
              <option value="">Todos los mercados</option>
              {marketsQuery.data?.map(market => <option key={market.id} value={market.id}>{market.name}</option>)}
            </Select>
          </Field>
          {marketsQuery.data?.length ? (
            <section className={ui.stack}>
              {marketsQuery.data.map((market) => (
                <div className={ui.row} key={market.id}><Text>{market.name}</Text></div>
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
          markets={marketsQuery.data ?? []}
          history={historyQuery.data ?? null}
          isLoading={historyQuery.isLoading}
          errorStatus={(historyQuery.error as { status?: number } | undefined)?.status}
        />
      </section>
    ),
  })
}
