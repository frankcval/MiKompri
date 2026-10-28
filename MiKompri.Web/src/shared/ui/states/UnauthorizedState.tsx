import ui from '../AppSurface.module.css'
import { Text } from '@fluentui/react-components'

export function UnauthorizedState() {
  return (
    <section className={ui.feedback} role="alert">
      <Text weight="semibold">No autorizado</Text>
      <Text>Tu sesión no es válida. Inicia sesión nuevamente.</Text>
    </section>
  )
}
