import { Card, CardHeader, Text } from '@fluentui/react-components'
import type { ProductPriceHistory } from '../api/productCatalogApi'

type PriceHistoryPanelProps = {
  history: ProductPriceHistory | null
  isLoading?: boolean
  errorStatus?: number
}

export function PriceHistoryPanel({ history, isLoading = false, errorStatus }: PriceHistoryPanelProps) {
  if (isLoading) {
    return (
      <Card>
        <CardHeader header={<Text weight="semibold">Historial de precios</Text>} />
        <Text>Cargando historial...</Text>
      </Card>
    )
  }

  if (errorStatus) {
    return (
      <Card>
        <CardHeader header={<Text weight="semibold">Historial de precios</Text>} />
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
    <Card>
      <CardHeader header={<Text weight="semibold">Historial de precios</Text>} />
      {records.length === 0 ? (
        <Text>Sin historial de precios para el filtro seleccionado.</Text>
      ) : (
        <section style={{ display: 'grid', gap: '0.4rem' }}>
          {records.map((record) => (
            <Text key={`${record.marketId}-${record.effectiveDate}-${record.priceAmount}`}>
              {record.effectiveDate} · {record.marketId} · {record.priceAmount}
            </Text>
          ))}
        </section>
      )}
    </Card>
  )
}
