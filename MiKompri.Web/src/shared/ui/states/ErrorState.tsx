import { AppIcon } from '../icons/AppIcon'
import ui from '../AppSurface.module.css'
import { Text } from '@fluentui/react-components'

type ErrorStateProps = {
  title?: string
  description?: string
}

export function ErrorState({ title = 'Ocurrió un error', description = 'Intenta nuevamente más tarde.' }: ErrorStateProps) {
  return (
    <section className={ui.feedback} role="alert">
      <AppIcon name="error" /><Text weight="semibold">{title}</Text>
      <Text>{description}</Text>
    </section>
  )
}
