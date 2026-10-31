let generation = 0
let controller = new AbortController()
const resets = new Set<() => void>()
export const sessionStamp = () => generation
export const sessionSignal = () => controller.signal
export function assertCurrentSession(stamp: number) {
  if (stamp !== generation) throw new Error('La sesión ha cambiado. Vuelve a consultar tus datos.')
}
export function onSessionReset(callback: () => void) {
  resets.add(callback)
  return () => { resets.delete(callback) }
}
export function resetPrivateSession() {
  generation++
  controller.abort()
  controller = new AbortController()
  for (const reset of [...resets]) reset()
}
// Same-tab and cross-tab account changes invalidate in-flight private work.
if (typeof window !== 'undefined') window.addEventListener('storage', event => {
  if (event.key === 'mikompri.account-boundary') resetPrivateSession()
})
export function announceAccountBoundary() {
  resetPrivateSession()
  try { localStorage.setItem('mikompri.account-boundary', crypto.randomUUID()) } catch { /* Storage may be unavailable. */ }
}
