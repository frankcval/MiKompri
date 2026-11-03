// Execute the real synthetic audit/capture tools; never read account sessions or env files.
import { execFileSync, spawn } from 'node:child_process'
import { mkdirSync } from 'node:fs'
import { resolve } from 'node:path'
import { setTimeout } from 'node:timers/promises'
import { normalizeLcov } from './normalize-lcov.mjs'

const output = resolve('../specs/011-web-iconography/.local/sonar-coverage')
mkdirSync(output, { recursive: true })
const run = args => execFileSync(process.execPath, args, { stdio: 'inherit' })
run(['scripts/audit-iconography.mjs', resolve(output, 'bundle-final.json')])
const server = spawn(process.execPath, ['node_modules/vite/bin/vite.js', '--host', '127.0.0.1', '--port', '5181', '--strictPort'], { stdio: 'inherit' })
try {
  let ready = false
  for (let attempt = 0; attempt < 30; attempt++) {
    if (server.exitCode !== null) throw new Error('Isolated coverage server exited before readiness.')
    try { ready = (await fetch('http://127.0.0.1:5181', { signal: AbortSignal.timeout(1000) })).ok } catch { /* Startup only. */ }
    if (ready) break
    await setTimeout(1000)
  }
  if (!ready) throw new Error('Isolated coverage server did not become ready.')
  run(['scripts/capture-ui-review.mjs', 'http://127.0.0.1:5181', output])
  run(['scripts/diagnose-lcp.mjs', 'http://127.0.0.1:5181', 'coverage', resolve(output, 'diagnostic.json')])
} finally {
  server.kill()
}
normalizeLcov('coverage/lcov.info')
