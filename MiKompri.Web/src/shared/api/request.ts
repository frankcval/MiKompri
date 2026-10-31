import { assertCurrentSession, sessionStamp, sessionSignal } from '../../auth/session/sessionBoundary'
import { localDateHeaders } from '../dates/localDate'
export const errorMessages: Record<number, string> = {
  400: 'Revisa los datos indicados y vuelve a intentarlo.',
  401: 'Tu sesión ha caducado. Inicia sesión de nuevo para continuar.',
  403: 'No tienes permiso para esta acción. Vuelve a tus listas o consulta con la persona que administra el grupo.',
  404: 'Este recurso ya no está disponible. Vuelve a tus listas.',
  409: 'Hay un conflicto con los datos actuales. Actualiza la vista y revisa los cambios antes de continuar.',
  412: 'El perfil ha cambiado. Actualiza los datos antes de guardar de nuevo.',
  428: 'Actualiza el perfil antes de guardar los cambios.',
  503: 'El servicio no está disponible temporalmente. Conservamos tus datos; inténtalo de nuevo.',
}

export async function sessionFetch(input: RequestInfo | URL, init?: RequestInit): Promise<Response> {
  const stamp = sessionStamp()
  const response = await fetch(input, { ...init, signal: init?.signal ? AbortSignal.any([init.signal, sessionSignal()]) : sessionSignal() })
  assertCurrentSession(stamp)
  const json = response.json.bind(response)
  response.json = async () => { const result: unknown = await json(); assertCurrentSession(stamp); return result }
  return response
}

export async function responseError(response: Response): Promise<Error & { status: number }> {
  let message = errorMessages[response.status] ?? 'No se pudo completar la operación. Inténtalo de nuevo.'
  if (response.status === 400) {
    try {
      const body = await response.json() as { errors?: { message: string }[] | Record<string, string[]> }
      const values = Array.isArray(body.errors) ? body.errors.map(e => e.message) : Object.values(body.errors ?? {}).flat()
      if (values.length) message = values.join(' ')
    } catch { /* A useful fallback also covers empty responses. */ }
  }
  return Object.assign(new Error(message), { status: response.status })
}

export async function request<T>(url: string, token: string, method = 'GET', body?: unknown, headers?: Record<string, string>): Promise<T> {
  const stamp = sessionStamp()
  const dates = body !== null && typeof body === 'object' && ('purchasedOn' in body || 'effectiveDate' in body) ? localDateHeaders() : {}
  let response: Response
  try {
    response = await sessionFetch(url, { method, headers: { ...dates, ...headers, Authorization: `Bearer ${token}`, Accept: 'application/json', ...(body === undefined ? {} : { 'Content-Type': 'application/json' }) }, ...(body === undefined ? {} : { body: JSON.stringify(body) }) })
  } catch { assertCurrentSession(stamp); throw new Error('No hay conexión con el servicio. Revisa tu conexión y actualiza antes de repetir una operación de guardado.') }
  assertCurrentSession(stamp)
  if (!response.ok) throw await responseError(response)
  const value = response.status === 204 ? undefined as T : await response.json() as T
  assertCurrentSession(stamp)
  return value
}

export const describeError = (error: unknown) => error instanceof Error ? error.message : 'No se pudo completar la operación. Inténtalo de nuevo.'
