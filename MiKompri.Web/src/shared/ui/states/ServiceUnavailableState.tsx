import { Text } from '@fluentui/react-components'

export function ServiceUnavailableState() {
  return (
    <section role="alert">
      <Text weight="semibold">Servicio no disponible</Text>
      <Text>El servicio está temporalmente no disponible. Intenta nuevamente.</Text>
    </section>
  )
}
