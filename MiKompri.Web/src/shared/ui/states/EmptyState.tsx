import { Text } from '@fluentui/react-components'

type EmptyStateProps = {
  title?: string
  description?: string
}

export function EmptyState({ title = 'Sin datos', description = 'No hay información para mostrar.' }: EmptyStateProps) {
  return (
    <section>
      <Text weight="semibold">{title}</Text>
      <Text>{description}</Text>
    </section>
  )
}
