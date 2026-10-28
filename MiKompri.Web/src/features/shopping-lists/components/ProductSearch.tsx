import { Field, Input } from '@fluentui/react-components'
export function ProductSearch({ value, onChange, disabled }: { value: string; onChange: (value: string) => void; disabled?: boolean }) {
  return <Field label="Buscar productos"><Input type="search" value={value} disabled={disabled} placeholder="Por ejemplo, leche…" autoComplete="off" onChange={(_, data) => onChange(data.value)} /></Field>
}
