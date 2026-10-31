import { useEffect, useState, useSyncExternalStore, type Dispatch, type SetStateAction } from 'react'
import { onSessionReset, sessionStamp } from './sessionBoundary'
const prefix = 'mikompri.draft.'
let currentAccount: string | undefined

export function clearDrafts() {
  currentAccount = undefined
  try {
    for (const key of Object.keys(sessionStorage)) if (key.startsWith(prefix)) sessionStorage.removeItem(key)
    sessionStorage.removeItem('mikompri.draft-account')
  } catch { /* Optional storage. */ }
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
  const generation = useSyncExternalStore(onSessionReset, sessionStamp, sessionStamp)
  const key = JSON.stringify([account, scope])
  const [state, setState] = useState(() => ({ key, generation, value: account ? readDraft(account, scope, fallback) : fallback }))
  let current = state
  if (state.key !== key || state.generation !== generation) {
    current = { key, generation, value: state.generation !== generation ? fallback : account ? readDraft(account, scope, fallback) : fallback }
    setState(current)
  }
  useEffect(() => {
    if (account && current.key === key && current.generation === generation && sessionStamp() === generation)
      writeDraft(account, scope, current.value)
  }, [account, scope, key, generation, current])
  const setValue: Dispatch<SetStateAction<T>> = value => setState(before => {
    if (sessionStamp() !== generation || before.key !== key || before.generation !== generation) return before
    return { ...before, value: typeof value === 'function' ? (value as (previous: T) => T)(before.value) : value }
  })
  return [current.value, setValue] as const
}
