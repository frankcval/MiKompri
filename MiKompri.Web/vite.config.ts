import react from '@vitejs/plugin-react'
import { resolve } from 'node:path'
import { defineConfig } from 'vite'
import { presentationChunks } from './vite.chunks.ts'

export default defineConfig({
  plugins: [react()],
  build: { rolldownOptions: { output: { codeSplitting: presentationChunks } } },
  resolve: {
    alias: {
      '@': resolve(import.meta.dirname, './src'),
    },
  },
  server: {
    port: 5173,
  },
  preview: {
    port: 4173,
  },
})
