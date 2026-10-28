import { existsSync } from 'node:fs'
export function checkIntegratedEnvironment() {
  const names = ['MIKOMPRI_INTEGRATED_URL', 'MIKOMPRI_OWNER_TOKEN', 'MIKOMPRI_MEMBER_TOKEN', 'MIKOMPRI_OWNER_STORAGE', 'MIKOMPRI_MEMBER_STORAGE']
  if (process.env.MIKOMPRI_REQUIRE_API_URLS === '1') names.push('MIKOMPRI_SHOPPING_API', 'MIKOMPRI_USERS_API', 'MIKOMPRI_CATALOG_API')
  const missing = names.filter(name => !process.env[name]?.trim())
  for (const name of ['MIKOMPRI_OWNER_STORAGE', 'MIKOMPRI_MEMBER_STORAGE']) if (process.env[name] && !existsSync(process.env[name])) missing.push(`${name} (archivo no disponible)`)
  if (missing.length) throw new Error(`Integración real no configurada: ${missing.join(', ')}. Prepara el entorno y las sesiones descritos en MiKompri.Web/README.md.`)
}
if (process.argv[1]?.endsWith('check-integrated-env.mjs')) {
  try { checkIntegratedEnvironment(); process.stdout.write('Prerrequisitos de integración presentes.\n') }
  catch (error) { process.stderr.write(`${error.message}\n`); process.exitCode = 1 }
}
