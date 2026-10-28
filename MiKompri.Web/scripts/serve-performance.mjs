import { createServer } from 'node:http'
import { readFileSync, existsSync, statSync } from 'node:fs'
import { resolve, extname, sep } from 'node:path'
import { gzipSync } from 'node:zlib'
// Laboratory static server, bound to loopback. Not an application backend.
const root = resolve(process.argv[2])
const port = Number(process.argv[3] ?? 5175)
const mime = { '.html': 'text/html', '.js': 'application/javascript', '.css': 'text/css', '.woff2': 'font/woff2', '.ttf': 'font/ttf', '.svg': 'image/svg+xml' }
createServer((req, res) => {
  let path = resolve(root, '.' + new URL(req.url, 'http://localhost').pathname)
  if (!path.startsWith(root + sep)) { res.writeHead(403).end(); return }
  if (!existsSync(path) || !statSync(path).isFile()) { res.writeHead(404).end(); return }
  const headers = { 'Content-Type': mime[extname(path)] ?? 'application/octet-stream', 'Cache-Control': 'no-store' }
  let body = readFileSync(path)
  if (/\.(js|css|html|svg)$/.test(path) && req.headers['accept-encoding']?.includes('gzip')) { body = gzipSync(body); headers['Content-Encoding'] = 'gzip' }
  res.writeHead(200, headers); res.end(body)
}).listen(port, '127.0.0.1', () => process.stdout.write(`Laboratory static server listening on 127.0.0.1:${port}\n`))
