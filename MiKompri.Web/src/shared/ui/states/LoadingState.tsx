import { Spinner, Text } from '@fluentui/react-components'

type LoadingStateProps = {
  label?: string
}

export function LoadingState({ label = 'Cargando...' }: LoadingStateProps) {
  return (
    <section aria-live="polite" style={{ display: 'grid', gap: '0.5rem' }}>
      <Spinner label={label} />
      <Text>{label}</Text>
    </section>
  )
}
