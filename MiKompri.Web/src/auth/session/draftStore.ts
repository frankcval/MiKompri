import { useEffect, useState } from 'react'
const prefix = 'mikompri.draft.'
let currentAccount: string | undefined

export function clearDrafts() {
  try { for (const key of Object.keys(sessionStorage)) if (key.startsWith(prefix)) sessionStorage.removeItem(key) } catch { /* Optional storage. */ }
}

export function selectDraftAccount(account: string) {
  try {
    const previous = currentAccount ?? sessionStorage.getItem('mikompri.draft-account')
    if (previous && previous !== account) clearDrafts()
    currentAccount = account
    sessionStorage.setItem('mikompri.draft-account', account)
  } catch { /* Forms also work without storage. */ }
}

export function readDraft<T>(account: string, scope: string, fallback: T): T {
  try {
    selectDraftAccount(account)
    const entry = JSON.parse(sessionStorage.getItem(`${prefix}${account}.${scope}`) ?? 'null') as { value: T; time: number } | null
    return entry && Date.now() - entry.time < 86400000 ? entry.value : fallback
  } catch { return fallback }
}

export function writeDraft<T>(account: string, scope: string, value: T) {
  try { selectDraftAccount(account); sessionStorage.setItem(`${prefix}${account}.${scope}`, JSON.stringify({ value, time: Date.now() })) } catch { /* In-memory draft remains usable. */ }
}

export function discardDraft(account: string, scope: string) { try { sessionStorage.removeItem(`${prefix}${account}.${scope}`) } catch { /* Optional storage. */ } }

export function useDraft<T>(account: string, scope: string, fallback: T) {
  const [value, setValue] = useState(() => account ? readDraft(account, scope, fallback) : fallback)
  useEffect(() => { if (account) writeDraft(account, scope, value) }, [account, scope, value])
  return [value, setValue] as const
}
