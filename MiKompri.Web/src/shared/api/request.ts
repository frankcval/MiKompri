import { localDateHeaders } from '../dates/localDate'
export const errorMessages: Record<number, string> = {
  400: 'Revisa los datos indicados y vuelve a intentarlo.',
  401: 'Tu sesión ha caducado. Inicia sesión de nuevo para continuar.',
  403: 'No tienes permiso para esta acción. Vuelve a tus listas o consulta con la persona que administra el grupo.',
  404: 'Este recurso ya no está disponible. Vuelve a tus listas.',
  409: 'Hay un conflicto con los datos actuales. Actualiza la vista y revisa los cambios antes de continuar.',
  503: 'El servicio no está disponible temporalmente. Conservamos tus datos; inténtalo de nuevo.',
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

export async function request<T>(url: string, token: string, method = 'GET', body?: unknown): Promise<T> {
  const dates = body !== null && typeof body === 'object' && ('purchasedOn' in body || 'effectiveDate' in body) ? localDateHeaders() : {}
  let response: Response
  try {
    response = await fetch(url, { method, headers: { ...dates, Authorization: `Bearer ${token}`, Accept: 'application/json', ...(body === undefined ? {} : { 'Content-Type': 'application/json' }) }, ...(body === undefined ? {} : { body: JSON.stringify(body) }) })
  } catch { throw new Error('No hay conexión con el servicio. Revisa tu conexión y actualiza antes de repetir una operación de guardado.') }
  if (!response.ok) throw await responseError(response)
  return response.status === 204 ? undefined as T : await response.json() as T
}

export const describeError = (error: unknown) => error instanceof Error ? error.message : 'No se pudo completar la operación. Inténtalo de nuevo.'
