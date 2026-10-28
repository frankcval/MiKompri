import ui from '../AppSurface.module.css'
import { Spinner, Text } from '@fluentui/react-components'

type LoadingStateProps = {
  label?: string
}

export function LoadingState({ label = 'Cargando...' }: LoadingStateProps) {
  return (
    <section className={ui.feedback} aria-live="polite">
      <Spinner label={label} />
      <Text>{label}</Text>
    </section>
  )
}
