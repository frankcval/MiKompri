import { afterEach, expect, it, vi } from 'vitest'
import { restoreFocusAfterClose } from '../../../src/shared/ui/restoreFocusAfterClose'

afterEach(() => { document.body.replaceChildren(); vi.restoreAllMocks() })

it('does not steal focus when the user has already moved to another control', () => {
  let frame!: FrameRequestCallback
  vi.spyOn(window, 'requestAnimationFrame').mockImplementation(callback => { frame = callback; return 1 })
  const trigger = document.createElement('button')
  const closing = document.createElement('button')
  const next = document.createElement('button')
  document.body.append(trigger, closing, next)
  closing.focus()
  restoreFocusAfterClose(trigger)
  closing.remove()
  next.focus()
  frame(0)
  expect(next).toHaveFocus()
})

it('restores the trigger when closing the overlay leaves focus on the body', () => {
  let frame!: FrameRequestCallback
  vi.spyOn(window, 'requestAnimationFrame').mockImplementation(callback => { frame = callback; return 1 })
  const trigger = document.createElement('button')
  const closing = document.createElement('button')
  document.body.append(trigger, closing)
  closing.focus()
  restoreFocusAfterClose(trigger)
  closing.remove()
  frame(0)
  expect(trigger).toHaveFocus()
})
