import s from './AppSurface.module.css'

export function PageHeader({ title, description }: { title: string; description: string }) {
  return <header className={s.pageHeader}><h1>{title}</h1><p>{description}</p></header>
}
