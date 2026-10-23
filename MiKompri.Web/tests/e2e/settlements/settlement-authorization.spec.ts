import { test } from '@playwright/test'

test.describe('settlement authorization', () => {
  test.skip('usuario sin permisos recibe 403 en settlement', async () => {
    // Requiere usuarios de prueba con membresías diferenciadas.
  })
})
