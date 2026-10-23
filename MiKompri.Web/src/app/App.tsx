import { MainLayout } from './layout/MainLayout'

function App() {
  return (
    <MainLayout>
      <section style={{ display: 'grid', gap: '0.75rem' }}>
        <h2 style={{ margin: 0 }}>Inicio</h2>
        <p>Fundación transversal del cliente web inicializada.</p>
      </section>
    </MainLayout>
  )
}

export default App
