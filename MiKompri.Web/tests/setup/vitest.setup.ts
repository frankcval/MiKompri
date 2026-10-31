import '@testing-library/jest-dom'
import { afterEach, beforeEach, vi } from 'vitest'

// Auth tests configure CIAM explicitly; a developer's local QA cutover is not a fixture.
vi.stubEnv('VITE_CIAM_ENABLED', 'false')
// Match the synthetic AccountInfo fixtures independently of ignored .env.test files.
vi.stubEnv('VITE_ENTRA_TENANT_ID', '00000000-0000-0000-0000-000000000000')

// JSDOM has no layout: its zero-sized body makes Tabster treat every modal
// as being in a hidden iframe. Supply a viewport only; real geometry, focus
// trapping and responsiveness are covered separately by Playwright.
let restoreBodyRect: (() => void) | undefined
beforeEach(() => {
  sessionStorage.clear()
  const rect = vi.spyOn(document.body, 'getBoundingClientRect').mockReturnValue({
    x: 0, y: 0, top: 0, left: 0, right: 1024, bottom: 768,
    width: 1024, height: 768, toJSON: () => ({}),
  })
  restoreBodyRect = () => rect.mockRestore()
})
afterEach(() => restoreBodyRect?.())
