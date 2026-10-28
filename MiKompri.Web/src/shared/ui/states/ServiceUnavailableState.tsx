import ui from '../AppSurface.module.css'
import { Text } from '@fluentui/react-components'

export function ServiceUnavailableState() {
  return (
    <section className={ui.feedback} role="alert">
      <Text weight="semibold">Servicio no disponible</Text>
      <Text>El servicio está temporalmente no disponible. Intenta nuevamente.</Text>
    </section>
  )
}
