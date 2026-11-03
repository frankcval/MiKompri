import { AppIcon } from '../../../shared/ui/icons/AppIcon'
import { useRef, useState } from 'react'
import { Button, Dialog, DialogActions, DialogBody, DialogContent, DialogSurface, DialogTitle, Field, Input, Tooltip, useRestoreFocusTarget } from '@fluentui/react-components'
import { describeError } from '../../../shared/api/request'
import ui from '../../../shared/ui/AppSurface.module.css'
import { useDraft } from '../../../auth/session/draftStore'

export function ProductOptions({ name, children }: { name: string; children: React.ReactNode }) {
  const [open, setOpen] = useState(false)
  return <details className={ui.productOptions} onToggle={event => setOpen(event.currentTarget.open)} onKeyDown={event => {
    if (event.key === 'Escape' && event.currentTarget.open && event.currentTarget.contains(event.target as Node)) {
      event.currentTarget.open = false; event.currentTarget.querySelector('summary')?.focus(); event.stopPropagation()
    }
  }}>
    <Tooltip content={`Opciones de ${name}`} relationship="label"><summary aria-label={`Opciones de ${name}`}><AppIcon name="more" /><span className={ui.optionsText}>Más opciones</span></summary></Tooltip>
    {open ? <div className={ui.rowActions}>{children}</div> : null}
  </details>
}

export function ConfirmAction({ label, text = label, description, onConfirm, disabled = false, icon = 'delete' }: { label: string; text?: string; description: string; onConfirm: () => Promise<void>; disabled?: boolean; icon?: 'delete' | 'close' }) {
  const restore = useRestoreFocusTarget()
  const trigger = useRef<HTMLButtonElement>(null)
  const [open, setOpen] = useState(false)
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  const close = () => { setOpen(false); requestAnimationFrame(() => trigger.current?.focus()) }
  return <><Button {...restore} ref={trigger} icon={<AppIcon name={icon} />} className={ui.danger} aria-label={label} disabled={disabled} onClick={event => { event.currentTarget.focus(); setError(''); setOpen(true) }}>{text}</Button>
    <Dialog open={open} onOpenChange={(_, d) => { if (!pending) { if (d.open) setOpen(true); else close() } }}><DialogSurface><DialogBody>
      <DialogTitle>{label}</DialogTitle><DialogContent><p>{description}</p>{error ? <p role="alert">{error}</p> : null}</DialogContent>
      <DialogActions><Button disabled={pending} onClick={close}>Cancelar</Button><Button icon={<AppIcon name={icon} />} className={ui.danger} disabled={pending} onClick={async () => {
        if (pending) return; setPending(true)
        try { await onConfirm(); close() } catch (cause) { setError(describeError(cause)) } finally { setPending(false) }
      }}>{pending ? 'Guardando…' : 'Confirmar'}</Button></DialogActions>
    </DialogBody></DialogSurface></Dialog>
  </>
}

export function RenameListAction({ name, onRename, accountKey = '', scope = name }: { name: string; onRename: (value: string) => Promise<void>; accountKey?: string; scope?: string }) {
  const restore = useRestoreFocusTarget()
  const [open, setOpen] = useState(false)
  const [value, setValue] = useDraft(accountKey, `rename:${scope}`, name)
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  return <><Button {...restore} icon={<AppIcon name="edit" />} onClick={event => { event.currentTarget.focus(); setError(''); setOpen(true) }}>Renombrar</Button>
    <Dialog open={open} onOpenChange={(_, d) => { if (!pending) setOpen(d.open) }}><DialogSurface><form className={ui.form} onSubmit={async e => {
      e.preventDefault(); if (pending || !value.trim()) return; setPending(true)
      try { await onRename(value.trim()); setOpen(false) } catch (cause) { setError(describeError(cause)) } finally { setPending(false) }
    }}><DialogBody><DialogTitle>Renombrar lista</DialogTitle><DialogContent><Field label="Nombre de la lista" required><Input value={value} maxLength={120} required disabled={pending} onChange={(_, d) => setValue(d.value)} /></Field>{error ? <p role="alert">{error}</p> : null}</DialogContent>
      <DialogActions><Button disabled={pending} onClick={() => setOpen(false)}>Cancelar</Button><Button appearance="primary" type="submit" disabled={pending}>{pending ? 'Guardando…' : 'Guardar nombre'}</Button></DialogActions>
    </DialogBody></form></DialogSurface></Dialog>
  </>
}

export function EditItemAction({ item, onEdit, accountKey = '' }: { item: { id: string; productName: string; quantity: number; productPrice?: number | null }; onEdit: (quantity: number, price: number | null) => Promise<void>; accountKey?: string }) {
  const restore = useRestoreFocusTarget()
  const [open, setOpen] = useState(false)
  const [quantity, setQuantity] = useDraft(accountKey, `edit:${item.id}:quantity`, String(item.quantity))
  const [price, setPrice] = useDraft(accountKey, `edit:${item.id}:price`, item.productPrice == null ? '' : String(item.productPrice))
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  return <><Button {...restore} icon={<AppIcon name="edit" />} aria-label={`Editar ${item.productName}`} onClick={event => { event.currentTarget.focus(); setError(''); setOpen(true) }}>Editar</Button>
    <Dialog open={open} onOpenChange={(_, d) => { if (!pending) setOpen(d.open) }}><DialogSurface><form className={ui.form} onSubmit={async e => {
      e.preventDefault(); if (pending) return
      const q = Number(quantity); const p = price.trim() ? Number(price.replace(',', '.')) : null
      if (!Number.isSafeInteger(q) || q < 1 || (p != null && (!Number.isFinite(p) || p < 0))) { setError('Revisa la cantidad y el precio.'); return }
      setPending(true)
      try { await onEdit(q, p); setOpen(false) } catch (cause) { setError(describeError(cause)) } finally { setPending(false) }
    }}><DialogBody><DialogTitle>Editar {item.productName}</DialogTitle><DialogContent className={ui.form}>
      <Field label="Cantidad" required><Input value={quantity} type="number" min={1} step={1} required disabled={pending} onChange={(_, d) => setQuantity(d.value)} /></Field>
      <Field label="Precio estimado (opcional)" hint="Déjalo vacío si todavía no lo sabes."><Input value={price} inputMode="decimal" disabled={pending} onChange={(_, d) => setPrice(d.value)} /></Field>
      {error ? <p role="alert">{error}</p> : null}</DialogContent><DialogActions><Button disabled={pending} onClick={() => setOpen(false)}>Cancelar</Button><Button appearance="primary" type="submit" disabled={pending}>{pending ? 'Guardando…' : 'Guardar producto'}</Button></DialogActions>
    </DialogBody></form></DialogSurface></Dialog>
  </>
}
