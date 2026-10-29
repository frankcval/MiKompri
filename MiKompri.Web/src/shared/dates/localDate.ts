/** A civil date chosen by the user; never convert it through UTC. */
export function localDate(now = new Date()): string {
  return `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`
}

export function localDateHeaders(now = new Date()): Record<string, string> {
  return { 'X-Client-Utc-Offset-Minutes': String(-now.getTimezoneOffset()) }
}
