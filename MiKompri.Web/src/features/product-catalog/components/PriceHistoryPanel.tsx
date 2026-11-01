import type { ReactNode } from 'react'
import ui from '../../../shared/ui/AppSurface.module.css'
import { Button, Card, CardHeader, Text } from '@fluentui/react-components'
import type { Market, ProductPriceHistory } from '../api/productCatalogApi'

type PriceHistoryPanelProps = {
  history: ProductPriceHistory | null
  productName?: string
  filters?: ReactNode
  isLoading?: boolean
  errorStatus?: number
  markets?: Market[]
  hasError?: boolean
  selected?: boolean
  onRetry?: () => void
}

export function PriceHistoryPanel({ filters, productName, history, isLoading = false, errorStatus, markets = [], hasError = false, selected = true, onRetry }: PriceHistoryPanelProps) {
  const name = productName || history?.catalogProductName
  const title = `Historial de precios${name ? ` de ${name}` : ''}`
  if (isLoading) {
    return (
      <Card className={ui.panel}>
        <CardHeader header={<h2>{title}</h2>} />
        {filters}
        <Text>Cargando historial...</Text>
      </Card>
    )
  }

  if (hasError || errorStatus) {
    return (
      <Card className={ui.panel}>
        <CardHeader header={<h2>{title}</h2>} />
        {filters}
        <Text>
          {errorStatus === 503
            ? 'ProductCatalog no disponible temporalmente.'
            : 'No fue posible cargar historial de precios.'}
        </Text>
        {onRetry ? <Button onClick={onRetry}>Reintentar historial</Button> : null}
      </Card>
    )
  }

  const records = history?.records ?? []

  return (
    <Card className={ui.panel}>
      <CardHeader header={<h2>{title}</h2>} />
        {filters}
      {!selected ? <Text>Selecciona un producto para consultar su historial de precios.</Text> : records.length === 0 ? (
        <Text>Sin historial de precios para el filtro seleccionado.</Text>
      ) : (
        <section className={ui.rows}>
          {records.map((record) => (
            <div className={ui.row} key={`${record.marketId}-${record.effectiveDate}-${record.priceAmount}`}>
              <span>{Number.isNaN(Date.parse(record.effectiveDate)) ? record.effectiveDate : new Intl.DateTimeFormat('es-ES', { timeZone: 'UTC' }).format(new Date(record.effectiveDate))} · {record.marketName || markets.find(market => market.id === record.marketId)?.name || 'Mercado no disponible'}</span>
              <strong>{new Intl.NumberFormat('es-ES', { style: 'currency', currency: record.currency || 'EUR' }).format(record.priceAmount)}</strong>
            </div>
          ))}
        </section>
      )}
    </Card>
  )
}
