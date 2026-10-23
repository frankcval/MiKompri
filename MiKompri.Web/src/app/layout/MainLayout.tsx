import type { PropsWithChildren } from 'react'
import { Link } from 'react-router-dom'

export function MainLayout({ children }: PropsWithChildren) {
  return (
    <div
      style={{
        margin: '0 auto',
        maxWidth: '1200px',
        padding: '1rem',
        display: 'grid',
        gap: '1rem',
      }}
    >
      <header>
        <h1 style={{ margin: 0 }}>MiKompri Web</h1>
      </header>

      <nav aria-label="Navegación principal" style={{ display: 'flex', flexWrap: 'wrap', gap: '0.75rem' }}>
        <Link to="/profile">Perfil</Link>
        <Link to="/groups">Grupos</Link>
        <Link to="/personal-lists">Listas personales</Link>
        <Link to="/shared-lists">Listas compartidas</Link>
        <Link to="/product-catalog">ProductCatalog</Link>
      </nav>

      <main role="main">{children}</main>
    </div>
  )
}
