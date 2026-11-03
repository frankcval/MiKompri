import type { ReactNode } from 'react'
import { Drawer, DrawerBody, DrawerHeader, DrawerHeaderTitle } from '@fluentui/react-components'
import { IconAction } from '../../shared/ui/icons/IconAction'
import s from './MainLayout.module.css'

export default function MobileNavigationDrawer({ onClose, children, navClassName = s.links, navLabel = 'Navegación principal', closeClassName = s.closeButton }: { onClose: () => void; children: ReactNode; navClassName?: string; navLabel?: string; closeClassName?: string }) {
  return <Drawer open onOpenChange={(_, data) => { if (!data.open) onClose() }} position="end" size="small">
    <DrawerHeader><DrawerHeaderTitle action={<IconAction name="close" className={closeClassName} label="Cerrar menú" onClick={onClose} />}>MiKompri</DrawerHeaderTitle></DrawerHeader>
    <DrawerBody><nav className={navClassName} aria-label={navLabel}>{children}</nav></DrawerBody>
  </Drawer>
}
