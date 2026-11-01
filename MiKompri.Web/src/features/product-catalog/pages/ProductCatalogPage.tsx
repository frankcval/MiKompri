import { AppIcon } from '../../../shared/ui/icons/AppIcon'
import { currentAccount, accountSessionKey } from '../../../auth/session/accountIdentity'
import { PageHeader } from '../../../shared/ui/PageHeader'
import ui from '../../../shared/ui/AppSurface.module.css'
import { Button, Card, Field, Input, Select, Tab, TabList } from '@fluentui/react-components'
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
import { CatalogEditor, MarketManagement, RegisterPriceForm } from '../components/CatalogManagement'
import { CatalogDialog } from '../components/CatalogDialog'
import { IconAction } from '../../../shared/ui/icons/IconAction'
import s from '../components/CatalogSurface.module.css'
import { withViewState } from '../../../shared/ui/states/withViewState'

export function ProductCatalogPage() {
  return <section className={ui.page}><PageHeader icon="products" title="Catálogo de productos" description="Los productos de tus listas, sus unidades y el historial de precios." /><ProductCatalogContent /></section>
}

function ProductCatalogContent() {
  const { instance, accounts } = useMsal()
  const account = currentAccount(instance, accounts)
  const queryClient = useQueryClient()
  const [searchParams] = useSearchParams()
  const requestedReturn = searchParams.get('returnTo') ?? ''
  const returnTo = /^\/(personal-lists|shared-lists)\/[0-9a-f-]{36}(\?tab=(shopping|expenses|people))?$/i.test(requestedReturn) ? requestedReturn : null
  const authSession = useMemo(() => createAuthSessionService(instance), [instance])

  const [selectedProductId, setSelectedProductId] = useState('')
  const [selectedMarketId, setSelectedMarketId] = useState('')
  const [search, setSearch] = useState('')
  const [view, setView] = useState('products')
  const [from, setFrom] = useState('')
  const [to, setTo] = useState('')

  const productsQuery = useQuery({
    placeholderData: previous => previous,
    queryKey: ['catalog-products', accountSessionKey(account), search],
    enabled: Boolean(account),
    retry: false,
    queryFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return getCatalogProducts(env.productCatalogApiBaseUrl, token, search)
    },
  })

  const marketsQuery = useQuery({
    queryKey: ['catalog-markets', accountSessionKey(account), 'all'],
    enabled: Boolean(account),
    retry: false,
    queryFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return getMarkets(env.productCatalogApiBaseUrl, token, true)
    },
  })

  const historyQuery = useQuery({
    queryKey: ['catalog-price-history', selectedProductId, selectedMarketId, accountSessionKey(account), from, to],
    enabled: Boolean(account && selectedProductId && !(from && to && from > to)),
    retry: false,
    queryFn: async () => {
      const token = await authSession.getAccessToken(loginRequest.scopes, account)
      return getProductPriceHistory(
        env.productCatalogApiBaseUrl,
        token,
        selectedProductId,
        selectedMarketId || undefined,
        from || undefined,
        to || undefined,
      )
    },
  })

  const selectedProduct = productsQuery.data?.find(product => product.id === selectedProductId)

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

        <div className={s.toolbar}>
          <TabList aria-label="Secciones del catálogo" selectedValue={view} onTabSelect={(_, data) => setView(String(data.value))}>
            <Tab value="products" icon={<AppIcon name="products" selected={view === 'products'} />}>Productos</Tab>
            <Tab value="markets" icon={<AppIcon name="markets" selected={view === 'markets'} />}>Mercados</Tab>
          </TabList>
          {view === 'products' ? <CatalogDialog title="Crear producto" trigger={<Button appearance="primary" icon={<AppIcon name="add" />}>Crear producto</Button>}>{(_close, onPendingChange) =>
            <CreateCatalogProductForm key={accountSessionKey(account)} onPendingChange={onPendingChange} onCreate={async input => {
              const product = await executeAuthenticatedWrite(
                token => createCatalogProduct(env.productCatalogApiBaseUrl, token, input),
                () => authSession.getAccessToken(loginRequest.scopes, account),
                async () => authSession.login(loginRequest.scopes),
              )
              await queryClient.invalidateQueries({ queryKey: ['catalog-products', accountSessionKey(account)] })
              return product
            }} />
          }</CatalogDialog> : null}
        </div>
        <section role="tabpanel" aria-label={view === 'products' ? 'Productos' : 'Mercados'} className={ui.stack}>
          {view === 'markets' ? <Card className={ui.panel}><MarketManagement markets={marketsQuery.data ?? []} /></Card> : <>
            <Card className={ui.panel}>
              <Field label="Buscar en el catálogo"><Input contentBefore={<AppIcon name="search" size={20} />} type="search" value={search} onChange={(_, d) => setSearch(d.value)} /></Field>
              {productsQuery.data?.length ? <ul className={s.products} aria-label="Productos del catálogo">{productsQuery.data.map(product => <li key={product.id} className={s.product}>
                <div className={s.identity}><strong>{product.name} ({product.purchaseUnit ?? 'N/A'})</strong></div>
                <div className={s.actions}>
                  <Button icon={<AppIcon name="history" />} appearance="subtle" aria-expanded={selectedProductId === product.id} aria-controls={selectedProductId === product.id ? 'catalog-price-history' : undefined} onClick={() => setSelectedProductId(product.id)}>Ver historial</Button>
                  <CatalogDialog title={`Editar producto ${product.name}`} trigger={<IconAction name="edit" label={`Editar producto ${product.name}`} />}>{(close, onPendingChange) =>
                    <CatalogEditor product={product} onDone={close} onPendingChange={onPendingChange} />
                  }</CatalogDialog>
                </div>
              </li>)}</ul> : withViewState({ state: 'empty', content: null, emptyTitle: 'Sin productos', emptyDescription: 'No hay productos disponibles.' })}
            </Card>
            {selectedProduct ? <section id="catalog-price-history" className={s.history} role="region" aria-label={`Historial de precios de ${selectedProduct.name}`}>
              <PriceHistoryPanel filters={<div className={ui.stack}>
                <Field label="Filtrar por mercado (opcional)"><Select value={selectedMarketId} onChange={(_, data) => setSelectedMarketId(data.value)}>
                  <option value="">Todos los mercados</option>{marketsQuery.data?.map(market => <option key={market.id} value={market.id}>{market.name}</option>)}
                </Select></Field>
                <div className={ui.actions}><Field label="Desde"><Input type="date" value={from} onChange={(_, data) => setFrom(data.value)} /></Field><Field label="Hasta"><Input type="date" value={to} onChange={(_, data) => setTo(data.value)} /></Field></div>
                {from && to && from > to ? <p role="alert">La fecha inicial debe ser anterior o igual a la final.</p> : null}
              </div>} productName={selectedProduct.name} markets={marketsQuery.data ?? []} history={historyQuery.data ?? null} isLoading={historyQuery.isLoading}
                errorStatus={(historyQuery.error as { status?: number } | undefined)?.status} hasError={historyQuery.isError} selected
                onRetry={() => { void historyQuery.refetch() }} />
              <Card className={ui.panel}><RegisterPriceForm key={selectedProductId} product={selectedProduct} markets={marketsQuery.data ?? []} /></Card>
            </section> : null}
          </>}
        </section>
      </section>
    ),
  })
}
