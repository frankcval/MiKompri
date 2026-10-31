import type { ReactNode } from 'react'
import { Drawer, DrawerBody, DrawerHeader, DrawerHeaderTitle } from '@fluentui/react-components'
import { IconAction } from '../../shared/ui/icons/IconAction'
import s from './MainLayout.module.css'

export default function MobileNavigationDrawer({ onClose, children }: { onClose: () => void; children: ReactNode }) {
  return <Drawer open onOpenChange={(_, data) => { if (!data.open) onClose() }} position="end" size="small">
    <DrawerHeader><DrawerHeaderTitle action={<IconAction name="close" className={s.closeButton} label="Cerrar menú" onClick={onClose} />}>MiKompri</DrawerHeaderTitle></DrawerHeader>
    <DrawerBody><nav className={s.links} aria-label="Navegación principal">{children}</nav></DrawerBody>
  </Drawer>
}
