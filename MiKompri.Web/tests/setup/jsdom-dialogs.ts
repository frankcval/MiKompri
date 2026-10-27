import { createElement } from 'react'
import { vi } from 'vitest'

// The real modalizer depends on browser layout/focus that JSDOM cannot supply.
// Exercise real Fluent dialog content, state, forms and events as non-modal
// only in these unit tests. Playwright tests use the untouched modal Dialog
// and verify focus entry, Escape, restoration and responsive geometry.
vi.mock('@fluentui/react-components', async importOriginal => {
  const actual = await importOriginal<typeof import('@fluentui/react-components')>()
  return {
    ...actual,
    Dialog: (props: import('@fluentui/react-components').DialogProps) => createElement(actual.Dialog, { ...props, modalType: 'non-modal' }),
  }
})
