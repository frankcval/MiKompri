import { Text } from '@fluentui/react-components'

export function ForbiddenState() {
  return (
    <section role="alert">
      <Text weight="semibold">Sin permisos</Text>
      <Text>No tienes permisos para acceder a este recurso.</Text>
    </section>
  )
}
