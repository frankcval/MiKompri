import { lazy, Suspense, useRef, useState, type PropsWithChildren } from 'react'
import { Link, NavLink, Outlet, useLocation } from 'react-router-dom'
import { useRestoreFocusTarget } from '@fluentui/react-components'
import s from './MainLayout.module.css'
import { AccountMenu } from '../../shared/ui/AccountMenu'
import { AppIcon } from '../../shared/ui/icons/AppIcon'
import { IconAction } from '../../shared/ui/icons/IconAction'

const MobileNavigationDrawer = lazy(() => import('./MobileNavigationDrawer'))

const destinations = [
  ['/lists', 'Mis listas', 'lists'], ['/groups', 'Grupos', 'groups'], ['/product-catalog', 'Productos', 'products'],
] as const

export function MainLayout({ children }: PropsWithChildren) {
  const [open, setOpen] = useState(false)
  const location = useLocation()
  const restoreFocus = useRestoreFocusTarget()
  const menuTrigger = useRef<HTMLButtonElement>(null)
  const closeMenu = () => { setOpen(false); requestAnimationFrame(() => menuTrigger.current?.focus()) }
  const links = destinations.map(([to, label, icon]) => {
    const selected = location.pathname.startsWith(to) || (to === '/lists' && /\/(personal|shared)-lists/.test(location.pathname))
    return <Link key={to} to={to} aria-current={selected ? 'page' : undefined} onClick={() => setOpen(false)}><AppIcon name={icon} size={24} selected={selected} /><span>{label}</span></Link>
  })
  return <div className={s.shell}>
    <a className={s.skip} href="#main-content">Saltar al contenido</a>
    <header className={s.header}><NavLink to="/" aria-label="MiKompri, inicio"><span aria-hidden="true">✓</span>MiKompri.</NavLink>
      <AccountMenu className={s.account} />
      <IconAction {...restoreFocus} ref={menuTrigger} name="menu" className={s.menuButton} label="Abrir menú" aria-expanded={open} onKeyDown={event => { if (event.key === 'Escape' && open) closeMenu() }} onClick={event => { event.currentTarget.focus(); setOpen(true) }} />
    </header>
    <div className={s.body}>
      <nav className={`${s.desktopNav} ${s.links}`} aria-label="Navegación principal">{links}</nav>
      <main className={s.main} id="main-content" tabIndex={-1}>{children ?? <Outlet />}</main>
    </div>
    {open ? <Suspense fallback={<span role="status">Preparando menú…</span>}><MobileNavigationDrawer onClose={closeMenu}>{links}</MobileNavigationDrawer></Suspense> : null}
    <nav className={s.bottomNav} aria-label="Accesos rápidos">{links}</nav>
  </div>
}
