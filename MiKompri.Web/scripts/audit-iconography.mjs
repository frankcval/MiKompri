// Reproducible package/import and production payload audit; never reads .env or tokens.
import { readFileSync, readdirSync, writeFileSync } from 'node:fs'
import { join, relative } from 'node:path'
import { createHash } from 'node:crypto'
import { gzipSync } from 'node:zlib'
import { execFileSync } from 'node:child_process'

function files(dir) {
  return readdirSync(dir, { withFileTypes: true }).flatMap(e => e.isDirectory() ? files(join(dir, e.name)) : [join(dir, e.name)])
}
const sha256 = data => createHash('sha256').update(data).digest('hex')
const sources = files('src').filter(p => /\.(tsx?|css)$/.test(p)).sort()
const sourceHash = createHash('sha256')
const imports = []
const exports = new Set([...readFileSync('node_modules/react-icons/pi/index.d.ts', 'utf8').matchAll(/export declare const (\w+)/g)].map(m => m[1]))
for (const path of sources) {
  const source = readFileSync(path, 'utf8')
  sourceHash.update(path.replaceAll('\\', '/')).update('\0').update(source).update('\0')
  if (/import\s*\([^)]*react-icons|import\s*\*[^;]*react-icons/.test(source)) throw new Error(`Non-static icon import: ${path}`)
  for (const match of source.matchAll(/import\s*\{([^}]+)\}\s*from\s*['"](react-icons[^'"]*)['"]/g)) {
    if (match[2] !== 'react-icons/pi') throw new Error(`Unexpected icon family: ${path}`)
    const names = match[1].split(',').map(n => n.trim()).filter(Boolean)
    if (names.some(n => !exports.has(n))) throw new Error(`Unknown export: ${path}`)
    imports.push({ path: path.replaceAll('\\', '/'), names })
  }
}
const pkg = JSON.parse(readFileSync('node_modules/react-icons/package.json', 'utf8'))
const lock = JSON.parse(readFileSync('package-lock.json', 'utf8')).packages['node_modules/react-icons']
const notice = readFileSync('public/licenses/react-icons-phosphor.txt')
if (!readFileSync('dist/licenses/react-icons-phosphor.txt').equals(notice)) throw new Error('Icon notice missing from the production build')
const payload = files('dist').filter(p => /\.(js|css)$/.test(p)).sort().map(path => {
  const bytes = readFileSync(path)
  return { path: relative('dist', path).replaceAll('\\', '/'), bytes: bytes.length, gzip: gzipSync(bytes).length, sha256: sha256(bytes) }
})
const report = {
  baseCommit: execFileSync('git', ['rev-parse', 'HEAD'], { encoding: 'utf8' }).trim(),
  kind: 'production-build-working-tree-candidate', sourceSha256: sourceHash.digest('hex'),
  lockSha256: sha256(readFileSync('package-lock.json')),
  buildConfigSha256: sha256(['vite.config.ts', 'vite.chunks.ts'].map(p => p + '\0' + readFileSync(p, 'utf8')).join('\0')),
  package: { name: pkg.name, version: pkg.version, license: pkg.license, integrity: lock.integrity },
  notice: { path: 'licenses/react-icons-phosphor.txt', sha256: sha256(notice) },
  imports, files: payload,
  total: payload.reduce((s, f) => ({ bytes: s.bytes + f.bytes, gzip: s.gzip + f.gzip }), { bytes: 0, gzip: 0 }),
}
writeFileSync(process.argv[2] ?? '../specs/011-web-iconography/evidence/bundle-after.json', JSON.stringify(report, null, 2) + '\n')
console.log(JSON.stringify({ package: report.package, exports: imports.reduce((n, i) => n + i.names.length, 0), sourceSha256: report.sourceSha256, total: report.total }))
