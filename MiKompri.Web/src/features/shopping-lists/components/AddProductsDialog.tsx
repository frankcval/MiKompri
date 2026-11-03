import { AppIcon } from '../../../shared/ui/icons/AppIcon'
import { lazy, Suspense, useEffect, useRef, useState } from 'react'
import { Button, Dialog, DialogActions, DialogBody, DialogContent, DialogSurface, DialogTitle, DialogTrigger, Spinner } from '@fluentui/react-components'
import type { ListItemInput } from '../api/shoppingListsApi'
import ui from '../../../shared/ui/AppSurface.module.css'

const Content = lazy(() => import('./AddProductsDialogContent').then(module => ({ default: module.AddProductsDialogContent })))

export function AddProductsDialog({ returnTo, onAdd, disabled = false }: { returnTo: string; onAdd: (item: ListItemInput) => Promise<void>; disabled?: boolean }) {
  const [open, setOpen] = useState(false)
  const [ready, setReady] = useState(false)
  const [pending, setPending] = useState(false)
  const trigger = useRef<HTMLButtonElement>(null)
  const surface = useRef<HTMLDivElement>(null)
  const close = () => { if (!pending) { setOpen(false); requestAnimationFrame(() => trigger.current?.focus()) } }
  useEffect(() => {
    if (!open) return
    const frame = requestAnimationFrame(() => { if (surface.current && !surface.current.contains(document.activeElement)) surface.current.focus() })
    return () => cancelAnimationFrame(frame)
  }, [open])
  return <Dialog open={open} onOpenChange={(_, data) => { if (!pending) { if (data.open) setOpen(true); else close() } }}>
    <DialogTrigger disableButtonEnhancement><Button ref={trigger} icon={<AppIcon name="add" />} appearance="primary" disabled={disabled} onClick={event => event.currentTarget.focus()}>Añadir productos</Button></DialogTrigger>
    <DialogSurface ref={surface} tabIndex={-1} className={`${ui.page} ${ui.panel}`}><DialogBody>
      <DialogTitle>{ready ? 'Añadir productos a la lista' : 'Preparando productos'}</DialogTitle>
      <DialogContent className={ui.stack}>{open ? <Suspense fallback={<Spinner label="Cargando selección de productos…" />}>
        <Content returnTo={returnTo} onAdd={onAdd} pending={pending} setPending={setPending} onReady={setReady} />
      </Suspense> : null}</DialogContent>
      <DialogActions><Button disabled={pending} onClick={close}>{ready ? 'Terminar' : 'Cancelar apertura de productos'}</Button></DialogActions>
    </DialogBody></DialogSurface>
  </Dialog>
}
