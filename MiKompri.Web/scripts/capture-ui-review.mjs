// Synthetic harness only: no account sessions, cloud access or storage exports.
import { chromium, firefox, webkit } from '@playwright/test'
import { writeFileSync } from 'node:fs'
import { modernFixtures, ids } from '../tests/e2e/fixtures/modernization.ts'
const base = process.argv[2] || 'http://127.0.0.1:5180'
const out = new URL('../../specs/011-web-iconography/evidence/', import.meta.url)
const screens = [
 ['home', '/', 'La compra, mejor', true], ['login', '/login', 'Inicio de sesión', true],
 ['lists', '/lists', 'Mis listas'], ['history', '/lists?view=history', 'Mis listas'],
 ['personal-lists', '/personal-lists', 'Listas personales'], ['personal', '/personal-lists/' + ids.personal, 'Compra de la semana'],
 ['personal-expenses', '/personal-lists/' + ids.personal + '?tab=expenses', 'Compra de la semana'],
 ['shared-lists', '/shared-lists?groupId=' + ids.group, 'Listas compartidas'], ['shared', '/shared-lists/' + ids.shared, 'Compra de casa'],
 ['expenses', '/shared-lists/' + ids.shared + '?tab=expenses', 'Compra de casa'], ['people', '/shared-lists/' + ids.shared + '?tab=people', 'Compra de casa'],
 ['groups', '/groups', 'Grupos'], ['group-detail', '/groups/' + ids.group, 'Casa'],
 ['catalog', '/product-catalog', 'Catálogo de productos'], ['markets', '/product-catalog', 'Catálogo de productos'],
 ['price-history', '/product-catalog', 'Catálogo de productos'], ['profile', '/profile', 'Perfil'],
]
const results = []
for (const [engine, launcher] of Object.entries({ chromium, firefox, webkit })) {
 const browser = await launcher.launch()
 try { for (const width of engine === 'chromium' ? [390,1280] : [390]) for (const [screen,path,heading,anonymous] of screens) {
  const page = await browser.newPage({ viewport: { width, height: 900 } })
  await modernFixtures(page, 3)
  await page.route('**/api/v1/groups', route => route.fulfill({ json: [{ id: ids.group, name: 'Casa', myRole: 'Owner', memberCount: 1 }] }))
  await page.route('**/api/v1/markets**', route => route.fulfill({ json: [{ id: ids.group, name: 'Mercado del barrio', locationHint: 'Cerca de casa', isActive: true }] }))
  await page.route('**/api/v1/catalog-products/*/price-history**', route => route.fulfill({ json: { catalogProductId: ids.product, catalogProductName: 'Tomates', records: [{ marketId: ids.group, marketName: 'Mercado del barrio', effectiveDate: '2026-10-20', priceAmount: 2.8, currency: 'EUR' }] } }))
  await page.goto(base + '/tests/visual/index.html?route=' + encodeURIComponent(path) + (anonymous ? '&anonymous' : ''))
  await page.getByRole('heading', { level: 1, name: new RegExp(heading) }).waitFor()
  await page.evaluate(() => document.fonts.ready)
  if (screen === 'markets') await page.getByRole('tab', { name: 'Mercados', exact: true }).click()
  if (screen === 'price-history') { await page.getByRole('button', { name: 'Ver historial' }).click(); await page.getByText('2,80 €', { exact: true }).waitFor() }
  if (screen === 'expenses') await page.getByRole('heading', { name: 'Balance por persona' }).waitFor()
  if (screen === 'personal-expenses') await page.getByRole('region', { name: 'Gastos personales' }).waitFor()
  if (screen === 'people') await page.getByRole('region', { name: 'Personas del grupo' }).waitFor()
  await page.waitForTimeout(150)
  const overflow = await page.evaluate(() => document.documentElement.scrollWidth > innerWidth)
  const file = `review-${engine}-${screen}-${width}.png`
  await page.screenshot({ path: new URL(file,out).pathname.replace(/^\/([A-Z]:)/,'$1'), fullPage: true, animations: 'disabled' })
  results.push({ browser: engine, screen, width, file, overflow }); await page.close()
 } } finally { await browser.close() }
}
writeFileSync(new URL('all-sections-screenshots.json',out), JSON.stringify({ kind:'synthetic-harness-captures', results },null,2)+'\n')
console.log(JSON.stringify({ captures: results.length, overflow: results.filter(r=>r.overflow) }))
