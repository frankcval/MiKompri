import { AppIcon } from './icons/AppIcon'
import type { IconName } from './icons/iconRegistry'
import s from './AppSurface.module.css'

export function PageHeader({ title, description, icon }: { title: string; description: string; icon?: IconName }) {
  return <header className={s.pageHeader}><h1>{icon ? <AppIcon name={icon} /> : null}{title}</h1><p>{description}</p></header>
}
