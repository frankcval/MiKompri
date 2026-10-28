import { existsSync } from 'node:fs'
export function checkIntegratedEnvironment() {
  const names = ['MIKOMPRI_INTEGRATED_URL', 'MIKOMPRI_OWNER_TOKEN', 'MIKOMPRI_MEMBER_TOKEN', 'MIKOMPRI_OWNER_STORAGE', 'MIKOMPRI_MEMBER_STORAGE']
  const missing = names.filter(name => !process.env[name])
  for (const name of ['MIKOMPRI_OWNER_STORAGE', 'MIKOMPRI_MEMBER_STORAGE']) if (process.env[name] && !existsSync(process.env[name])) missing.push(`${name} (archivo no disponible)`)
  if (missing.length) throw new Error(`Integración real no configurada: ${missing.join(', ')}. No se sustituye por una sesión simulada.`)
}
if (process.argv[1]?.endsWith('check-integrated-env.mjs')) { checkIntegratedEnvironment(); process.stdout.write('Prerrequisitos de integración presentes.\n') }
