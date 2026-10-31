import { lazy, Suspense, useRef, useState } from 'react'
import { Link, useLinkClickHandler } from 'react-router-dom'
import { Accordion, AccordionHeader, AccordionItem, AccordionPanel, Button, useRestoreFocusTarget } from '@fluentui/react-components'
import { exampleList } from './exampleList'
import s from './HomePage.module.css'
import { ShoppingListView } from '../../shared/ui/ShoppingListView'
import { IconAction } from '../../shared/ui/icons/IconAction'

const MobileNavigationDrawer = lazy(() => import('../../app/layout/MobileNavigationDrawer'))

function CreateListLink({ children = 'Crear mi primera lista' }: { children?: React.ReactNode }) {
  const onClick = useLinkClickHandler<HTMLAnchorElement>('/personal-lists?create=1')
  return <Button className={s.create} as="a" href="/personal-lists?create=1" onClick={onClick} appearance="primary" size="large">{children}</Button>
}

function HomeHeader() {
  const [open, setOpen] = useState(false)
  const restoreFocusTarget = useRestoreFocusTarget()
  const menuTrigger = useRef<HTMLButtonElement>(null)
  const closeMenu = () => { setOpen(false); requestAnimationFrame(() => menuTrigger.current?.focus()) }
  const links = <><a href="#como-funciona" onClick={() => setOpen(false)}>Cómo funciona</a><a href="#para-compartir" onClick={() => setOpen(false)}>Para compartir</a><a href="#preguntas" onClick={() => setOpen(false)}>Preguntas</a><Link to="/login">Entrar</Link></>
  return <header className={s.header}><div className={s.headerInner}>
    <Link className={s.brand} to="/" aria-label="MiKompri, inicio"><span aria-hidden="true">✓</span>MiKompri.</Link>
    <nav className={s.desktopNav} aria-label="Navegación de portada">{links}</nav>
    <div className={s.headerActions}><CreateListLink>Crear una lista</CreateListLink><IconAction {...restoreFocusTarget} ref={menuTrigger} name="menu" className={s.menuButton} label="Abrir menú" aria-expanded={open} onKeyDown={event => { if (event.key === 'Escape' && open) closeMenu() }} onClick={event => { event.currentTarget.focus(); setOpen(true) }} /></div>
    {open ? <Suspense fallback={<span className={s.menuLoading} role="status">Preparando menú…</span>}><MobileNavigationDrawer onClose={closeMenu} navClassName={s.mobileNav} navLabel="Navegación móvil" closeClassName={s.closeMenu}>{links}</MobileNavigationDrawer></Suspense> : null}
  </div></header>
}

export function ShoppingListPreview() {
  const [items, setItems] = useState(exampleList)
  return <div className={s.visual} id="ejemplo">
    <p className={s.caption}>Una lista. Todo el mundo al día.</p>
    <section className={s.preview} aria-label="Lista de ejemplo, datos ficticios">
      <div className={s.previewTop}><strong>✓ MiKompri</strong><span>Ejemplo interactivo</span></div>
      <div className={s.previewHeading}><span>Espacio · Casa</span><h2>Compra de la semana</h2><p className={s.members}>Tú, Ana y Dani · Personas ficticias</p></div>
      <div className={s.groceries}><ShoppingListView items={items.map((item, index) => ({ id: String(index), productName: item.name, quantity: 1, quantityLabel: item.quantity, productPrice: Number(item.price.replace(' €', '').replace(',', '.')), isPurchased: item.purchased }))}
        onToggle={(id, purchased) => setItems(current => current.map((row, i) => i === Number(id) ? { ...row, purchased } : row))} /></div>
      <div className={s.previewBottom}><span>Total estimado<strong>24,80 €</strong></span><Button onClick={() => setItems(exampleList)}>Restablecer ejemplo</Button></div>
    </section>
    <div className={s.balanceNote}><span aria-hidden="true">✓</span><div><strong>Las cuentas, claras.</strong><small>Cada uno sabe cuánto le toca.</small></div><b aria-hidden="true">÷ 3</b></div>
    <p className={s.hint}>Prueba a marcar algo. Los cambios son temporales y no se guardan.</p>
  </div>
}

function HomeHero() {
  return <section className={`${s.container} ${s.hero}`} aria-labelledby="home-title"><div>
    <p className={s.eyebrow}>La compra es parte de la vida</p>
    <h1 id="home-title">La compra, mejor<br /><span>en compañía.</span></h1>
    <p className={s.description}>Lo que falta en casa. Lo que pone cada uno. Organiza tus listas, compra en equipo y reparte los gastos con MiKompri.</p>
    <div className={s.actions}><CreateListLink /><Button as="a" href="#ejemplo" size="large">Explorar el ejemplo</Button></div>
    <p className={s.hint}>Para guardar tus listas, entra con tu cuenta de Microsoft.</p>
    <p className={s.heroNote}>Para tu casa, tu piso compartido<br /> y los planes que hacéis juntos.</p>
  </div><ShoppingListPreview /></section>
}

const steps = [
  ['Prepara la lista', 'Añade lo que necesitas y tenlo todo a mano.'],
  ['Compra en compañía', 'Organiza las listas de tu grupo y marca lo que ya está.'],
  ['Deja las cuentas claras', 'Registra los gastos y consulta el reparto entre participantes.'],
]

function HowItWorks() {
  return <section className={`${s.container} ${s.section}`} id="como-funciona"><p className={s.eyebrow}>Del plan a la compra</p><h2>Menos vueltas.<br />Más fácil para todos.</h2><ol className={s.steps}>{steps.map(([title, description], i) => <li key={title}><span className={s.stepNumber}>0{i + 1}</span><h3>{title}</h3><p>{description}</p></li>)}</ol></section>
}

function SharingScenarios() {
  return <section className={s.sharing} id="para-compartir"><div className={`${s.container} ${s.sharingInner}`}><div><p className={s.eyebrow}>Si lo compartes, encaja</p><h2>Para los tuyos.<br />Y para lo vuestro.</h2><p>La compra de siempre o un plan especial.</p></div><div className={s.scenarios}>{[['En casa', 'La despensa, entre todos.'], ['En el piso', 'Compartir sin perder las cuentas.'], ['En vuestros planes', 'Una cena. Una escapada. Una lista.']].map(([title, copy]) => <div key={title}><h3>{title}</h3><p>{copy}</p></div>)}</div></div></section>
}

function HomeFaq() {
  return <section className={`${s.container} ${s.section} ${s.faq}`} id="preguntas"><div><p className={s.eyebrow}>Antes de empezar</p><h2>Las cosas claras,<br />desde el principio.</h2></div><Accordion collapsible multiple>
    {[
      ['¿Para qué sirve MiKompri?', 'Prepara listas personales o de grupo, organiza la compra y consulta el reparto de gastos.'],
      ['¿Necesito una cuenta?', 'Sí, para crear y guardar listas debes iniciar sesión con Microsoft. El ejemplo de esta portada se puede explorar sin cuenta.'],
      ['¿Se guardan los cambios del ejemplo?', 'No. Son datos ficticios y temporales, separados de tus listas reales. Puedes restablecerlos en cualquier momento.'],
      ['¿MiKompri realiza pagos?', 'No. El reparto muestra balances y propuestas para saldar cuentas; no realiza transferencias ni cobros.'],
    ].map(([question, answer], i) => <AccordionItem value={String(i)} key={question}><AccordionHeader>{question}</AccordionHeader><AccordionPanel>{answer}</AccordionPanel></AccordionItem>)}
  </Accordion></section>
}

export function HomePage() {
  return <div className={s.home}><a className={s.skip} href="#main-content" tabIndex={0}>Saltar al contenido</a><HomeHeader /><main id="main-content" tabIndex={-1}><HomeHero /><HowItWorks /><SharingScenarios /><HomeFaq /><section className={`${s.container} ${s.finalCta}`}><h2>La próxima compra empieza aquí.</h2><p>Una lista compartida. Una cosa menos en la cabeza.</p><CreateListLink /></section></main><footer className={`${s.container} ${s.footer}`}><Link className={s.brand} to="/">MiKompri.</Link><span>La compra, mejor en compañía.</span></footer></div>
}
