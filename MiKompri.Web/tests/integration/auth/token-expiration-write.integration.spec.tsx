import { describe, expect, it, vi } from 'vitest'
import { executeAuthenticatedWrite } from '../../../src/auth/session/authSessionService'

describe('token expiration during write', () => {
  it('dispara reautenticación guiada cuando una operación de escritura retorna 401', async () => {
    const onUnauthorized = vi.fn(async () => Promise.resolve())

    await expect(
      executeAuthenticatedWrite(
        async () => {
          throw { status: 401 }
        },
        async () => 'token-abc',
        onUnauthorized,
      ),
    ).rejects.toBeDefined()

    expect(onUnauthorized).toHaveBeenCalledTimes(1)
  })
})
