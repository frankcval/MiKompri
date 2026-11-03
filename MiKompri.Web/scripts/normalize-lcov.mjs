import { readFileSync, writeFileSync } from 'node:fs'
import { isAbsolute, relative, resolve, sep } from 'node:path'
import { pathToFileURL } from 'node:url'

// Change source paths only: coverage hits/branches must remain exactly as measured.
export function normalizeLcov(file) {
  const root = resolve('..')
  const workspace = relative(root, resolve('.')).split(sep).join('/') + '/'
  const result = readFileSync(file, 'utf8').replace(/^SF:(.*)$/gm, (_, source) => {
    const value = source.trim().replaceAll('\\', '/')
    const path = relative(root, value.startsWith(workspace) ? resolve(root, value) : resolve(value))
    if (isAbsolute(path) || path === '..' || path.startsWith('..' + sep)) throw new Error('Coverage source is outside the repository.')
    return 'SF:' + path.split(sep).join('/')
  })
  writeFileSync(file, result)
}

if (process.argv[1] && import.meta.url === pathToFileURL(resolve(process.argv[1])).href) {
  for (const file of process.argv.slice(2)) normalizeLcov(file)
}
