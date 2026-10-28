import { test } from 'node:test'
import assert from 'node:assert/strict'
import { spawnSync } from 'node:child_process'
import { mkdtempSync, rmSync, writeFileSync } from 'node:fs'
import { tmpdir } from 'node:os'
import { join, resolve, sep } from 'node:path'
import { fileURLToPath } from 'node:url'

const script = fileURLToPath(new URL('./check-integrated-env.mjs', import.meta.url))
const cleanEnv = () => Object.fromEntries(Object.entries(process.env).filter(([key]) => !key.startsWith('MIKOMPRI_')))
const run = env => spawnSync(process.execPath, [script], { env, encoding: 'utf8' })
function prepared(t) {
  const base = resolve(tmpdir())
  const dir = mkdtempSync(join(base, 'mikompri-ci-config-'))
  t.after(() => {
    assert.ok(resolve(dir).startsWith(base + sep + 'mikompri-ci-config-'))
    rmSync(dir, { recursive: true, force: true })
  })
  const owner = join(dir, 'owner.json'); const member = join(dir, 'member.json')
  for (const path of [owner, member]) writeFileSync(path, JSON.stringify({ cookies: [], origins: [], sessionStorage: {} }))
  // Presence-only fixtures. These values never authenticate or run browser tests.
  return { ...cleanEnv(), MIKOMPRI_INTEGRATED_URL: 'https://qa.example.test', MIKOMPRI_OWNER_TOKEN: 'do-not-log-owner-fixture', MIKOMPRI_MEMBER_TOKEN: 'do-not-log-member-fixture', MIKOMPRI_OWNER_STORAGE: owner, MIKOMPRI_MEMBER_STORAGE: member }
}

test('missing real configuration fails with actionable names and no stack trace', () => {
  const result = run(cleanEnv())
  assert.equal(result.status, 1)
  assert.match(result.stderr, /MIKOMPRI_INTEGRATED_URL/)
  assert.match(result.stderr, /MIKOMPRI_OWNER_TOKEN/)
  assert.equal(result.stdout, '')
  assert.doesNotMatch(result.stderr, /at checkIntegratedEnvironment|at ModuleJob/)
})
test('local prerequisites allow the existing local API defaults', t => {
  const result = run(prepared(t))
  assert.equal(result.status, 0, result.stderr)
  assert.match(result.stdout, /Prerrequisitos de integración presentes/)
})
test('a missing exported session fails without logging tokens', t => {
  const env = prepared(t)
  env.MIKOMPRI_OWNER_STORAGE += '.missing'
  const result = run(env)
  assert.equal(result.status, 1)
  assert.match(result.stderr, /MIKOMPRI_OWNER_STORAGE \(archivo no disponible\)/)
  assert.ok(!result.stderr.includes(env.MIKOMPRI_OWNER_TOKEN))
  assert.ok(!result.stderr.includes(env.MIKOMPRI_MEMBER_TOKEN))
})
test('CI acceptance requires explicit service URLs', t => {
  const result = run({ ...prepared(t), MIKOMPRI_REQUIRE_API_URLS: '1' })
  assert.equal(result.status, 1)
  for (const key of ['MIKOMPRI_SHOPPING_API', 'MIKOMPRI_USERS_API', 'MIKOMPRI_CATALOG_API']) assert.ok(result.stderr.includes(key))
})
test('complete CI prerequisites pass the presence check', t => {
  const result = run({ ...prepared(t), MIKOMPRI_REQUIRE_API_URLS: '1', MIKOMPRI_SHOPPING_API: 'https://shopping.example.test', MIKOMPRI_USERS_API: 'https://users.example.test', MIKOMPRI_CATALOG_API: 'https://catalog.example.test' })
  assert.equal(result.status, 0, result.stderr)
})
test('whitespace credentials do not satisfy the prerequisites', t => {
  const result = run({ ...prepared(t), MIKOMPRI_OWNER_TOKEN: '  ' })
  assert.equal(result.status, 1)
  assert.match(result.stderr, /MIKOMPRI_OWNER_TOKEN/)
})
