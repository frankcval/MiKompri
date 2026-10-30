import { chromium } from '@playwright/test'
import { writeFileSync, mkdirSync } from 'node:fs'
import { modernFixtures, ids } from '../tests/e2e/fixtures/modernization.ts'
const base = process.argv[2] ?? 'http://127.0.0.1:5175'
const label = process.argv[3] ?? 'current'
const count = Number(process.env.PERFORMANCE_SAMPLES ?? 20)
const browser = await chromium.launch()
const cases = [
  { name: 'home', path: '/', anonymous: true, heading: 'La compra, mejor' },
  { name: 'lists', path: label === 'baseline' ? '/personal-lists' : '/lists', heading: label === 'baseline' ? 'Listas personales' : 'Mis listas' },
  { name: 'personal', path: `/personal-lists/${ids.personal}`, heading: 'Compra de la semana' },
  { name: 'shared', path: `/shared-lists/${ids.shared}`, heading: 'Compra de casa' },
]
const samples = []
try {
  for (const scenario of cases) for (let index = 0; index < count; index++) {
    const context = await browser.newContext({ viewport: { width: 390, height: 844 } })
    const page = await context.newPage()
    const cdp = await context.newCDPSession(page)
    await cdp.send('Network.enable')
    await cdp.send('Network.setCacheDisabled', { cacheDisabled: true })
    await cdp.send('Network.emulateNetworkConditions', { offline: false, latency: 150, downloadThroughput: 1600000 / 8, uploadThroughput: 750000 / 8 })
    await cdp.send('Emulation.setCPUThrottlingRate', { rate: 4 })
    await page.addInitScript(() => {
      window.__lab = { lcp: 0, cls: 0, events: [] }
      new PerformanceObserver(list => { for (const e of list.getEntries()) window.__lab.lcp = e.startTime }).observe({ type: 'largest-contentful-paint', buffered: true })
      new PerformanceObserver(list => { for (const e of list.getEntries()) if (!e.hadRecentInput) window.__lab.cls += e.value }).observe({ type: 'layout-shift', buffered: true })
      new PerformanceObserver(list => { for (const e of list.getEntries()) if (e.interactionId) window.__lab.events.push(e.duration) }).observe({ type: 'event', buffered: true, durationThreshold: 16 })
    })
    await modernFixtures(page, 50)
    await page.goto(`${base}/tests/visual/index.html?route=${encodeURIComponent(scenario.path)}${scenario.anonymous ? '&anonymous' : ''}`)
    await page.getByRole('heading', { level: 1, name: new RegExp(scenario.heading) }).waitFor()
    await page.evaluate(() => document.fonts.ready)
    await page.waitForTimeout(300)
    const navigation = await page.evaluate(() => ({ ...window.__lab, bytes: performance.getEntriesByType('resource').reduce((sum, r) => sum + r.transferSize, 0) }))
    const interactions = {}
    if (scenario.name === 'personal') {
      await page.getByRole('checkbox', { name: 'Marcar comprado: Tomates' }).or(page.getByRole('button', { name: 'Marcar comprado: Tomates' })).click()
      await page.waitForTimeout(250)
      interactions.mark = await page.evaluate(() => Math.max(0, ...window.__lab.events.splice(0)))
      // 009 offers optional capture after marking. Dismiss it explicitly before
      // measuring the independent add-products action; keep legacy baselines.
      const capture = page.getByRole('button', { name: 'Ahora no', exact: true })
      if (label.startsWith('009')) await capture.waitFor()
      if (await capture.isVisible()) {
        await capture.click()
        await page.getByRole('dialog', { name: 'Datos de compra de Tomates' }).waitFor({ state: 'hidden' })
      }
      await page.evaluate(() => window.__lab.events.splice(0))
      await page.getByRole('button', { name: 'Añadir productos', exact: true }).click()
      await page.getByRole('dialog', { name: 'Añadir productos a la lista' }).waitFor()
      if (label.startsWith('009')) await page.getByRole('group', { name: 'Seleccionar producto' }).waitFor()
      await page.waitForTimeout(250)
      interactions.add = await page.evaluate(() => Math.max(0, ...window.__lab.events.splice(0)))
    }
    if (scenario.name === 'shared') {
      await page.getByRole('tab', { name: 'Gastos', exact: true }).click()
      await page.getByRole('heading', { name: 'Balance por persona' }).waitFor()
      await page.waitForTimeout(250)
      interactions.expenses = await page.evaluate(() => Math.max(0, ...window.__lab.events.splice(0)))
    }
    samples.push({ route: scenario.name, path: scenario.path, index, ...navigation, interactions })
    await context.close()
    process.stdout.write(`${label} ${scenario.name} ${index + 1}/${count}: LCP ${navigation.lcp.toFixed(0)} ms; CLS ${navigation.cls.toFixed(4)}\n`)
  }
} finally { await browser.close() }
const percentile = values => [...values].sort((a, b) => a - b)[Math.ceil(values.length * .75) - 1]
const summaries = cases.map(c => {
  const rows = samples.filter(s => s.route === c.name)
  return { route: c.name, n: rows.length, p75LcpMs: percentile(rows.map(r => r.lcp)), p75Cls: percentile(rows.map(r => r.cls)), p75Bytes: percentile(rows.map(r => r.bytes)), interactions: Object.fromEntries(['mark', 'add', 'expenses'].filter(key => rows.some(r => key in r.interactions)).map(key => [key, percentile(rows.map(r => r.interactions[key]))])) }
})
mkdirSync('../.playwright-mcp', { recursive: true })
writeFileSync(`../.playwright-mcp/008-performance-${label}.json`, JSON.stringify({ label, browser: 'Chromium', cpu: '4x', network: '1.6Mbps, 150ms', viewport: '390x844', samples, summaries }, null, 2))
process.stdout.write(JSON.stringify(summaries) + '\n')
