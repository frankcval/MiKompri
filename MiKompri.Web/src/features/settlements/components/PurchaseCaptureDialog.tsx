import { lazy, Suspense, useEffect, useRef, useState, type RefObject } from 'react'
import { Button, Dialog, DialogActions, DialogBody, DialogContent, DialogSurface, DialogTitle, Spinner } from '@fluentui/react-components'
import type { SharedListItem } from '../../shared-lists/api/sharedListsApi'
import ui from '../../../shared/ui/AppSurface.module.css'

const Content = lazy(() => import('./PurchaseCapture').then(module => ({ default: module.PurchaseCaptureContent })))

export function PurchaseCaptureDialog({ listId, item, groupId, onClose, restoreFocusRef }: {
  listId: string; item: SharedListItem; groupId?: string; onClose: () => void; restoreFocusRef?: RefObject<HTMLElement | null>
}) {
  const [pending, setPending] = useState(false)
  const restore = useRef(document.activeElement as HTMLElement | null)
  const surface = useRef<HTMLDivElement>(null)
  useEffect(() => {
    const frame = requestAnimationFrame(() => { if (surface.current && !surface.current.contains(document.activeElement)) surface.current.focus() })
    return () => cancelAnimationFrame(frame)
  }, [])
  useEffect(() => {
    if (restoreFocusRef?.current) restore.current = restoreFocusRef.current
    return () => { requestAnimationFrame(() => { if (restore.current?.isConnected) restore.current.focus() }) }
  }, [restoreFocusRef])
  return <Dialog open onOpenChange={(_, data) => { if (!data.open && !pending) onClose() }}>
    <DialogSurface ref={surface} tabIndex={-1} className={ui.panel}><DialogBody>
      <DialogTitle>Datos de compra de {item.productName}</DialogTitle>
      <DialogContent><Suspense fallback={<Spinner label="Cargando datos de compra…" />}>
        <Content listId={listId} item={item} groupId={groupId} onClose={onClose} onPendingChange={setPending} />
      </Suspense></DialogContent>
      <DialogActions><Button disabled={pending} onClick={onClose}>Ahora no</Button></DialogActions>
    </DialogBody></DialogSurface>
  </Dialog>
}
