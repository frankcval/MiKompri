// Closing an overlay must not undo a newer, explicit focus choice.
export function restoreFocusAfterClose(trigger: HTMLElement | null) {
  const previous = document.activeElement
  requestAnimationFrame(() => {
    const current = document.activeElement
    if (trigger?.isConnected && (current === document.body || current === previous)) trigger.focus()
  })
}
