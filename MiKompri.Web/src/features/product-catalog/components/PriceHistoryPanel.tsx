import ui from '../../../shared/ui/AppSurface.module.css'
import { Card, CardHeader, Text } from '@fluentui/react-components'
import type { Market, ProductPriceHistory } from '../api/productCatalogApi'

type PriceHistoryPanelProps = {
  history: ProductPriceHistory | null
  isLoading?: boolean
  errorStatus?: number
  markets?: Market[]
}

export function PriceHistoryPanel({ history, isLoading = false, errorStatus, markets = [] }: PriceHistoryPanelProps) {
  if (isLoading) {
    return (
      <Card className={ui.panel}>
        <CardHeader header={<h2>Historial de precios</h2>} />
        <Text>Cargando historial...</Text>
      </Card>
    )
  }

  if (errorStatus) {
    return (
      <Card className={ui.panel}>
        <CardHeader header={<h2>Historial de precios</h2>} />
        <Text>
          {errorStatus === 503
            ? 'ProductCatalog no disponible temporalmente.'
            : 'No fue posible cargar historial de precios.'}
        </Text>
      </Card>
    )
  }

  const records = history?.records ?? []

  return (
    <Card className={ui.panel}>
      <CardHeader header={<h2>Historial de precios</h2>} />
      {records.length === 0 ? (
        <Text>Sin historial de precios para el filtro seleccionado.</Text>
      ) : (
        <section className={ui.rows}>
          {records.map((record) => (
            <div className={ui.row} key={`${record.marketId}-${record.effectiveDate}-${record.priceAmount}`}>
              <span>{Number.isNaN(Date.parse(record.effectiveDate)) ? record.effectiveDate : new Intl.DateTimeFormat('es-ES', { timeZone: 'UTC' }).format(new Date(record.effectiveDate))} · {markets.find(market => market.id === record.marketId)?.name ?? 'Mercado no disponible'}</span>
              <strong>{new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR' }).format(record.priceAmount)}</strong>
            </div>
          ))}
        </section>
      )}
    </Card>
  )
}
