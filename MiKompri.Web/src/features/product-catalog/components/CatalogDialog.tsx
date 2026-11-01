import { cloneElement, useEffect, useRef, useState, type ComponentPropsWithRef, type ReactElement, type ReactNode } from 'react'
import { Button, Dialog, DialogActions, DialogBody, DialogContent, DialogSurface, DialogTitle, DialogTrigger } from '@fluentui/react-components'
import ui from '../../../shared/ui/AppSurface.module.css'
import s from './CatalogSurface.module.css'

// Forms retain their own validation/write logic. Fluent owns the modal and trigger focus.
export function CatalogDialog({ title, trigger, children }: {
  title: string
  trigger: ReactElement
  children: (close: () => void, onPendingChange: (pending: boolean) => void) => ReactNode
}) {
  const [open, setOpen] = useState(false)
  const [pending, setPending] = useState(false)
  const target = useRef<HTMLButtonElement>(null)
  const [restore, setRestore] = useState(false)
  const close = () => { setRestore(true); setOpen(false) }
  useEffect(() => {
    if (open || pending || !restore) return
    const frame = requestAnimationFrame(() => { target.current?.focus(); setRestore(false) })
    return () => cancelAnimationFrame(frame)
  }, [open, pending, restore])
  return <Dialog open={open} onOpenChange={(_, data) => { if (!pending) { if (data.open) setOpen(true); else close() } }}>
    <DialogTrigger disableButtonEnhancement>{cloneElement(trigger as ReactElement<ComponentPropsWithRef<'button'>>, { ref: target, onClick: event => event.currentTarget.focus() })}</DialogTrigger>
    <DialogSurface className={`${ui.panel} ${ui.page} ${s.dialog}`}>
      <DialogBody><DialogTitle>{title}</DialogTitle>
        <DialogContent>{open ? children(close, setPending) : null}</DialogContent>
        <DialogActions><DialogTrigger disableButtonEnhancement><Button disabled={pending}>Cancelar</Button></DialogTrigger></DialogActions>
      </DialogBody>
    </DialogSurface>
  </Dialog>
}
