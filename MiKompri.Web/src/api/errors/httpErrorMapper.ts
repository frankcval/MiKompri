export type HttpErrorCode = 400 | 401 | 403 | 404 | 409 | 503 | 'unknown'

export type AppHttpError = {
  status: HttpErrorCode
  message: string
  details?: unknown
}

const defaultMessages: Record<HttpErrorCode, string> = {
  400: 'La solicitud no es válida.',
  401: 'Tu sesión no es válida. Inicia sesión nuevamente.',
  403: 'No tienes permisos para realizar esta acción.',
  404: 'No se encontró el recurso solicitado.',
  409: 'Existe un conflicto con el estado actual del recurso.',
  503: 'El servicio no está disponible temporalmente. Intenta de nuevo.',
  unknown: 'Ocurrió un error inesperado.',
}

export function mapHttpError(status: number, details?: unknown): AppHttpError {
  if (status in defaultMessages) {
    const typedStatus = status as Exclude<HttpErrorCode, 'unknown'>
    return {
      status: typedStatus,
      message: defaultMessages[typedStatus],
      details,
    }
  }

  return {
    status: 'unknown',
    message: defaultMessages.unknown,
    details,
  }
}
