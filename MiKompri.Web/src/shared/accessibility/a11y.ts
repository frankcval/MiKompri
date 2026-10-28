export function focusElementById(elementId: string): boolean {
  if (typeof document === 'undefined') {
    return false
  }

  const target = document.getElementById(elementId)
  if (!target) {
    return false
  }

  target.focus()
  return true
}

export function withAccessibleName(baseLabel: string, context?: string): string {
  const safeBase = baseLabel.trim()
  if (!context || context.trim().length === 0) {
    return safeBase
  }

  return `${safeBase} - ${context.trim()}`
}
