const key = 'mikompri.return-intent'
const lifetime = 24 * 60 * 60 * 1000

export function safeReturnTo(value: string): string | null {
  if (value.length > 2048 || !value.startsWith('/') || value.startsWith('//') || value.includes('\\') || [...value].some(char => char.charCodeAt(0) <= 32)) return null
  try {
    const url = new URL(value, 'https://mikompri.invalid')
    if (url.origin !== 'https://mikompri.invalid' || !/^\/(lists|personal-lists|shared-lists|groups|profile|product-catalog)(\/[0-9a-f-]{36})?$/.test(url.pathname)) return null
    const allowed = new Set(['tab', 'groupId', 'filter', 'create', 'returnTo'])
    for (const [name, entry] of url.searchParams) {
      if (!allowed.has(name)) url.searchParams.delete(name)
      else if (name === 'returnTo' && !safeReturnTo(entry)) url.searchParams.delete(name)
    }
    return url.pathname + url.search
  } catch { return null }
}

export function rememberReturnTo(value: string) {
  const path = safeReturnTo(value)
  if (path) { try { sessionStorage.setItem(key, JSON.stringify({ path, savedAt: Date.now() })) } catch { /* Storage can be unavailable. */ } }
}

export function consumeReturnTo(): string {
  try {
    const stored = JSON.parse(sessionStorage.getItem(key) ?? 'null') as { path: string; savedAt: number } | null
    sessionStorage.removeItem(key)
    return stored && Date.now() - stored.savedAt < lifetime ? safeReturnTo(stored.path) ?? '/lists' : '/lists'
  } catch { return '/lists' }
}

export function clearReturnIntent() { try { sessionStorage.removeItem(key) } catch { /* Optional storage. */ } }
