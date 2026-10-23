import { Text } from '@fluentui/react-components'

export function UnauthorizedState() {
  return (
    <section role="alert">
      <Text weight="semibold">No autorizado</Text>
      <Text>Tu sesión no es válida. Inicia sesión nuevamente.</Text>
    </section>
  )
}
