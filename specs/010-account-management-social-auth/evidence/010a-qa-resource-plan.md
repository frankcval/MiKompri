# Recursos de aceptación 010-A

2026-10-30. El titular aprueba implementación y ensayo de la cohorte Microsoft de 010-A. Este plan precede las nuevas mutaciones; no amplía G3/CA ni autoriza producción.

- Reutilizar tenant CIAM, API/SPA, Google y user flow QA del spike. Se añaden únicamente redirects localhost5173 de la Web normal; se conserva55412. Sin nuevos tenants, facturación, SMS, licencias o trials.
- APIs y PostgreSQL16 aislados, contenedores etiquetados010a-qa/tests, puertos loopback28080/28082/28083/55432, datos tmpfs. La BD previa solo se lee para obtener la tupla y UserId canónicos del piloto; no se copian datos personales ni se escriben las bases existentes. El fixture QA no acredita por sí solo su historial original.
- `.env.local` Web se respalda byte a byte en `.local/` ignorado y se restaura después del ensayo. Configuración CIAM y negocio nueva deshabilitada por defecto en fuente; habilitada únicamente en estas APIs QA tras pruebas JWT/D1.
- Para federación Microsoft, si no existe configuración QA reutilizable: un registro **nuevo y exclusivo QA** `MiKompri 010A QA Microsoft federation` en el tenant Microsoft origen, single-tenant, callbacks HTTPS exactos del CIAM QA. No modificar registros actuales. Service principal con asignación requerida, únicamente usuario piloto; consentimiento delegado de ese usuario para openid/profile/email/User.Read, sin permisos de aplicación/directorio.
- Si se requiere secreto confidencial para esa federación, duración máxima hasta2026-11-06; solo configuración CIAM y copia local cifrada DPAPI del titular, nunca texto plano, stdout, Git, SPA ni evidencia. No crear credenciales para las APIs. Inventario público/snapshot privado en `.local/`, excluido también de Docker.
- En CIAM, proveedor OIDC exclusivo del origen Microsoft aprobado y asociación al flujo QA; sin tocar Google/email/defaults/CA. Primero pausar altas en Users QA; no permitir que la identidad piloto desconocida cree otro UserId. Reservar solo tras comprobar issuer/issuerAssignedId inmutables reales, nunca email. Sin equivalencia acreditada, abortar cutover y mantener Microsoft legacy.

Limpieza/reversión: restaurar configuración Web y ruta Microsoft legacy; rollback del journal sin Down ni cambio de ownership. Conservar recursos QA necesarios hasta cerrar aceptación; registrar registro/SP/credencial/proveedor nuevos para retirarlos antes del vencimiento QA2026-11-06. Borrar exclusivamente los inventariados, retirando primero la dependencia CIAM. No automatizar borrado de tenants o recursos ajenos.

Fuentes reconsultadas: [federación Entra](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-entra-id-federation-customers), [OIDC CIAM](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-custom-oidc-federation-customers). La federación requiere cliente confidencial; su ejemplo de issuer Graph debe contrastarse con el objeto real. No es linking autoservicio.

## Actualización 31/10

La federación Microsoft no acreditó el origen/objeto destino requerido: no se ejecutó reserva/cutover/rollback real. Su asociación al flujo QA se retiró conservando app/proveedor/credencial QA inventariados hasta limpieza; Google y OTP del spike permanecen. Microsoft Web usa exclusivamente legacy. No hubo cambios CA/G3/defaults.

Tras perder los fixtures tmpfs al reiniciar, se sustituye exclusivamente el PostgreSQL QA por un volumen dedicado `mikompri-010a-postgres-data`. Contenedores QA usan restart unless-stopped; datos originales intactos. No asumir continuidad de usuarios/historia QA del30/10. [Diagnóstico y estado actual](authentication-recovery-2026-10-31.md). Se mantiene fecha de revisión/retirada QA2026-11-06, sin borrado automático ni ampliación de credenciales.

### Continuación acotada del31/10

Se reutilizan tenant/apps/credenciales anteriores. Se conserva un nuevo destino federado Microsoft QA sin correo/credenciales/roles, inventariado privadamente para diagnóstico y retiro antes del06/11; no acreditado por login y no vinculado en Users. El proveedor Microsoft temporal se retira del flujo y se restaura provisioning. No borrar ese destino ni otros objetos por una coincidencia de email.

En el flujo QA se seleccionan únicamente Display Name/Given Name/Surname además de Email: guardado/relectura tras recarga y layout comprueban los tres opcionales. Reversión: retirar solo esos tres atributos si fuese necesaria; no tocar cuentas/perfiles existentes. Falta alta Google nueva para acreditar importación; unknown anterior no se rellena automáticamente. No se amplía PII ni permisos Graph de negocio. Email OTP del spike se conserva porque retirarlo afectaría los consumidores/evidencia anteriores; la Web no ofrece email como ruta propia y T056 sigue abierto.
