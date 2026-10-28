import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { resolve } from 'node:path'
// Separate laboratory artifact. Never shipped in dist/.
export default defineConfig({
  root: process.cwd(), plugins: [react()],
  build: { outDir: resolve(process.cwd(), '../.playwright-mcp/performance-dist'), emptyOutDir: false, rolldownOptions: { input: resolve(process.cwd(), 'tests/visual/index.html') } },
})
