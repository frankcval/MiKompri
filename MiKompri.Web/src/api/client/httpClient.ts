import { mapHttpError, type AppHttpError } from '../errors/httpErrorMapper'

export async function httpGet<T>(url: string, token?: string): Promise<T> {
  const response = await fetch(url, {
    headers: {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      Accept: 'application/json',
    },
  })

  if (!response.ok) {
    let details: unknown = undefined
    try {
      details = await response.json()
    } catch {
      details = undefined
    }

    throw mapHttpError(response.status, details) as AppHttpError
  }

  return (await response.json()) as T
}
