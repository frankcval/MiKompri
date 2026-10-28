import { useEffect, useRef, useState } from 'react'
import { Controller, useForm, useWatch } from 'react-hook-form'
import { useMsal } from '@azure/msal-react'
import { readDraft, writeDraft } from '../../../auth/session/draftStore'
import { z } from 'zod'
import { Button, Dialog, DialogActions, DialogBody, DialogContent, DialogSurface, DialogTitle, DialogTrigger, Field, Input } from '@fluentui/react-components'
import ui from '../../../shared/ui/AppSurface.module.css'

const schema = z.object({ name: z.string().trim().min(1, 'Escribe un nombre para tu lista.').max(120, 'Usa como máximo 120 caracteres.') })
export function CreateListDialog({ title, onCreate, label = 'Crear lista', disabled = false, initialOpen = false, draftScope }: {
  title: string; label?: string; disabled?: boolean; initialOpen?: boolean; draftScope?: string; onCreate: (name: string) => Promise<void>
}) {
  const [open, setOpen] = useState(initialOpen)
  const [error, setError] = useState('')
  const [pending, setPending] = useState(false)
  const submitting = useRef(false)
  const trigger = useRef<HTMLButtonElement>(null)
  const { accounts } = useMsal()
  const account = accounts[0]?.homeAccountId
  const scope = `create-list:${draftScope ?? title}`
  const { control, handleSubmit, reset, setError: fieldError, formState: { errors } } = useForm<{ name: string }>({ defaultValues: account ? readDraft(account, scope, { name: '' }) : { name: '' } })
  const draft = useWatch({ control })
  useEffect(() => { if (account) writeDraft(account, scope, draft) }, [account, scope, draft])
  const close = () => { if (!pending) { setOpen(false); reset({ name: '' }); setError(''); requestAnimationFrame(() => trigger.current?.focus()) } }
  return <Dialog open={open} onOpenChange={(_, data) => { if (!pending) { if (data.open) setOpen(true); else close() } }}>
    <DialogTrigger disableButtonEnhancement><Button ref={trigger} appearance="primary" disabled={disabled} onClick={event => event.currentTarget.focus()}>{label}</Button></DialogTrigger>
    <DialogSurface aria-label={title} className={`${ui.page} ${ui.panel}`}>
      <form className={ui.form} onSubmit={event => { void handleSubmit(async value => {
        if (submitting.current) return
        const parsed = schema.safeParse(value)
        if (!parsed.success) { fieldError('name', { message: parsed.error.issues[0].message }, { shouldFocus: true }); return }
        setError('')
        submitting.current = true
        setPending(true)
        try { await onCreate(parsed.data.name); setOpen(false); reset({ name: '' }) }
        catch (cause) { setError(cause instanceof Error ? cause.message : 'No fue posible crear la lista. Inténtalo de nuevo.') }
        finally { submitting.current = false; setPending(false) }
      })(event) }}>
        <DialogBody><DialogTitle>{title}</DialogTitle><DialogContent>
          <Field label="Nombre de lista" required validationMessage={errors.name?.message}>
            <Controller control={control} name="name" render={({ field }) => <Input input={{ ref: field.ref }} name={field.name} value={field.value} onBlur={field.onBlur} onChange={(_, data) => field.onChange(data.value)} required maxLength={120} autoComplete="off" disabled={pending} />} />
          </Field>
          {error ? <p role="alert">{error}</p> : null}
        </DialogContent><DialogActions>
          <Button type="button" disabled={pending} onClick={close}>Cancelar</Button>
          <Button type="submit" appearance="primary" disabled={pending}>{pending ? 'Creando lista…' : 'Crear lista'}</Button>
        </DialogActions></DialogBody>
      </form>
    </DialogSurface>
  </Dialog>
}
