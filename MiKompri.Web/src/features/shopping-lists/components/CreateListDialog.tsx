import { AppIcon } from '../../../shared/ui/icons/AppIcon'
import { lazy, Suspense, useState } from 'react'
import { Button } from '@fluentui/react-components'

const Content = lazy(() => import('./CreateListDialogContent').then(module => ({ default: module.CreateListDialogContent })))

export function CreateListDialog(props: {
  title: string; label?: string; disabled?: boolean; initialOpen?: boolean; draftScope?: string; onCreate: (name: string) => Promise<void>
}) {
  const [requested, setRequested] = useState(props.initialOpen ?? false)
  const label = props.label ?? 'Crear lista'
  const trigger = <Button icon={<AppIcon name="add" />} appearance="primary" disabled={props.disabled || requested} aria-busy={requested} onClick={event => { event.currentTarget.focus(); setRequested(true) }}>{requested ? 'Abriendo formulario…' : label}</Button>
  return requested ? <Suspense fallback={trigger}><Content {...props} initialOpen /></Suspense> : trigger
}
