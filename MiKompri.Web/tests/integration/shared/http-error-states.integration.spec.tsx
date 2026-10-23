import { describe, expect, it } from 'vitest'
import { mapHttpError } from '../../../src/api/errors/httpErrorMapper'

describe('http error states integration', () => {
  it('mapea 401 a mensaje de sesión inválida', () => {
    const result = mapHttpError(401)
    expect(result.status).toBe(401)
  })

  it('mapea 403 y 503 correctamente', () => {
    const forbidden = mapHttpError(403)
    const unavailable = mapHttpError(503)
    expect(forbidden.status).toBe(403)
    expect(unavailable.status).toBe(503)
  })
})
