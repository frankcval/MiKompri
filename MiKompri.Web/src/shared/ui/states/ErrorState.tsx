import { Text } from '@fluentui/react-components'

type ErrorStateProps = {
  title?: string
  description?: string
}

export function ErrorState({ title = 'Ocurrió un error', description = 'Intenta nuevamente más tarde.' }: ErrorStateProps) {
  return (
    <section role="alert">
      <Text weight="semibold">{title}</Text>
      <Text>{description}</Text>
    </section>
  )
}
