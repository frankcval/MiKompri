import { Field, Select } from '@fluentui/react-components'
import { useSearchParams } from 'react-router-dom'
import { estimatedTotal, listHistoryReason, type ListPresentation } from '../listPresentation'
import ui from '../../../shared/ui/AppSurface.module.css'

export function ListOverviewControls() {
  const [params, setParams] = useSearchParams()
  const change = (key: string, value: string) => { const next = new URLSearchParams(params); next.set(key, value); setParams(next) }
  return <div className={ui.actions}>
    <Field label="Mostrar listas"><Select value={params.get('view') === 'history' ? 'history' : 'active'} onChange={(_, data) => change('view', data.value)}>
      <option value="active">Activas</option><option value="history">Historial</option>
    </Select></Field>
    <Field label="Ordenar listas"><Select value={params.get('sort') ?? 'newest'} onChange={(_, data) => change('sort', data.value)}>
      <option value="newest">Más recientes</option><option value="oldest">Más antiguas</option><option value="updated">Última modificación</option><option value="name">Nombre</option>
    </Select></Field>
  </div>
}

export function EstimatedTotal({ items }: { items: NonNullable<ListPresentation['items']> }) {
  const total = estimatedTotal(items)
  return <span>Estimación{total.unknown ? ' parcial' : ''}: <strong>{new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR' }).format(total.amount)}</strong>
    {total.unknown ? ` · ${total.unknown} ${total.unknown === 1 ? 'producto sin precio' : 'productos sin precio'}` : ''}</span>
}

export function ListOverview({ list }: { list: ListPresentation }) {
  return <><span>{listHistoryReason(list) ?? `${list.items?.filter(item => !item.isPurchased).length ?? 0} pendientes`}</span><EstimatedTotal items={list.items ?? []} />
    {list.createdAt ? <small>Creada el {new Intl.DateTimeFormat('es-ES').format(new Date(list.createdAt))}</small> : null}</>
}
