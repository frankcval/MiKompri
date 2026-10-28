import ui from '../AppSurface.module.css'
import { Text } from '@fluentui/react-components'

type EmptyStateProps = {
  title?: string
  description?: string
}

export function EmptyState({ title = 'Sin datos', description = 'No hay información para mostrar.' }: EmptyStateProps) {
  return (
    <section className={ui.feedback}>
      <Text weight="semibold">{title}</Text>
      <Text>{description}</Text>
    </section>
  )
}
