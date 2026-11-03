import { afterEach, test } from 'node:test'
import assert from 'node:assert/strict'
import { mkdtempSync, readFileSync, rmSync, writeFileSync } from 'node:fs'
import { tmpdir } from 'node:os'
import { join, resolve, sep } from 'node:path'
import { normalizeLcov } from './normalize-lcov.mjs'

const directories = []
function report(contents) {
  const directory = mkdtempSync(join(tmpdir(), 'mikompri-lcov-'))
  directories.push(directory)
  const file = join(directory, 'lcov.info')
  writeFileSync(file, contents)
  return file
}
afterEach(() => {
  for (const directory of directories.splice(0)) {
    assert.ok(resolve(directory).startsWith(resolve(tmpdir()) + sep + 'mikompri-lcov-'))
    rmSync(directory, { recursive: true })
  }
})
test('normalizes source paths without changing measured line or branch hits', () => {
  const hits = 'DA:2,0\nDA:3,1\nBRDA:3,0,0,1\nend_of_record\n'
  const file = report('SF:src/example.ts\n' + hits)
  normalizeLcov(file)
  assert.equal(readFileSync(file, 'utf8'), 'SF:MiKompri.Web/src/example.ts\n' + hits)
})
test('normalization is idempotent for repeated tooling runs', () => {
  const file = report('SF:src/example.ts\nDA:1,1\nend_of_record\n')
  normalizeLcov(file)
  const first = readFileSync(file, 'utf8')
  normalizeLcov(file)
  assert.equal(readFileSync(file, 'utf8'), first)
})
test('rejects sources outside the repository and leaves the report intact', () => {
  const original = 'SF:../../outside.ts\nDA:1,1\nend_of_record\n'
  const file = report(original)
  assert.throws(() => normalizeLcov(file), /outside the repository/)
  assert.equal(readFileSync(file, 'utf8'), original)
})
