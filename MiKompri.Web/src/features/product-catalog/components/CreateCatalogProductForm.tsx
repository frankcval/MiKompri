import ui from '../../../shared/ui/AppSurface.module.css'
import { Button, Field, Input } from '@fluentui/react-components'
import { useRef, useState } from 'react'
import type { CatalogProduct, CreateCatalogProductInput } from '../api/productCatalogApi'

export function CreateCatalogProductForm({ onCreate }: { onCreate: (input: CreateCatalogProductInput) => Promise<CatalogProduct> }) {
  const [name, setName] = useState('')
  const [unit, setUnit] = useState('')
  const [pending, setPending] = useState(false)
  const [error, setError] = useState('')
  const [created, setCreated] = useState('')
  const submitting = useRef(false)
  const valid = name.trim().length > 0 && name.trim().length <= 120 && unit.trim().length > 0 && unit.trim().length <= 30

  return <form className={ui.form} onSubmit={async (event) => {
    event.preventDefault()
    if (!valid || submitting.current) return
    submitting.current = true
    setPending(true)
    setError('')
    setCreated('')
    try {
      const product = await onCreate({ name: name.trim(), purchaseUnit: unit.trim() })
      setCreated(`${product.name} se creó y ya está disponible para tus listas.`)
      setName('')
      setUnit('')
    } catch (cause) {
      setError(cause instanceof Error ? cause.message : 'No fue posible crear el producto. Inténtalo de nuevo.')
    } finally {
      submitting.current = false
      setPending(false)
    }
  }}>
    <Field label="Nombre del producto" required>
      <Input required maxLength={120} value={name} disabled={pending} onChange={(_, data) => setName(data.value)} />
    </Field>
    <Field label="Unidad de compra" required hint="Por ejemplo: unidad, kg o litro.">
      <Input required maxLength={30} value={unit} disabled={pending} onChange={(_, data) => setUnit(data.value)} />
    </Field>
    {error ? <p role="alert">{error}</p> : null}
    {created ? <p role="status">{created}</p> : null}
    <Button appearance="primary" type="submit" disabled={!valid || pending}>{pending ? 'Creando producto…' : 'Crear producto'}</Button>
  </form>
}
