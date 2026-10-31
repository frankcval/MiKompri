# 010-B — Gestión de métodos diferida

30/10/2026. Backlog de producto, no implementación ni decisión automática de arquitectura. No confundir **incremento010-B** con **Alternative B** (modelo de varios bindings). Se prefiere A para la correlación simple de010-A sin haber validado agregación/linking.

## Funcionalidades que no bloquean010-A

US3: segundo método, Google+email tras registro, link/unlink, último método, cambio de proveedor, recuperación mediante otra identidad; AccountOperation/ProofReceipt, broker, mutaciones Graph y reconciliación. G3/step-up, Authentication Context y CA de linking permanecen investigación de esta iteración futura.

FR-006/008 y parte de FR-009/014/019/024/025 de [spec](spec.md) se difieren. Avatar propio/proveedor/storage, reimportación explícita UI y eliminación operativa quedan aquí o en su futura iniciativa aprobada. MAUI se formalizará en006. Apple/Facebook/passkeys/SMS/tipos Microsoft adicionales y MFA avanzada no se activan por estar soportados; tampoco bloquean A. Email OTP básico puede adelantarse opcionalmente con T056 sin implementar estas capacidades.

Account merging sigue fuera: dos UserIds existentes no se fusionan como solución de linking o migración.

## Requisitos conservados para reanudación

1. Linking: usuario A autenticado, consentimiento y prueba reciente del origen y candidato; servidor deriva UserId, nunca email/perfil. Ningún candidato auto-provisiona otro User.
2. Unlink: probar un método que permanece, inventario vigente y protección de último método bajo concurrencia; MFA/contacto no cuenta como login alternativo.
3. Recuperación: alternativa ya vinculada y acreditada mantiene UserId; no exigir proveedor perdido, no inventar sustitución por email ni recuperación sin control.
4. D3: edad<=300s al confirmar, operación<=600s, nonce/state/PKCE y propósito/sesión/versiones/un solo uso. No ampliar límites por tokens nuevos, iat, cookies, selector, contexto c1 o MFA histórica.
5. A agregador necesita demostrar login real tras agregar/quitar, colisiones y permisos mínimos; Graph identities editable no equivale a flujo seguro. Alternative B solo si un bloqueo de A está demostrado y se aprueba otro ADR; no programarla preventivamente.
6. Cualquier CA/Authentication Context nuevo requiere alcance QA y protección/restauración revisados. No reactivar recursos de spike por reorganizar documentos.

## Evidencia histórica que se conserva

[Spikes](evidence/spike-summary.md), [G3](evidence/assurance-spike.md), [revisión step-up](evidence/g3-step-up-review.md), [ensayo email MFA](evidence/g3-email-mfa-qa.md), [c1 sin política](evidence/g3-context-with-disabled-policy.json), [restauración](evidence/g3-restoration-verification.json), [agregación](evidence/aggregation-spike.md). G3 de ruta Google anterior NOT RELIABLE FOR G3; candidato email MFA social INCONCLUSIVE. No reinterpretar esos resultados como PASS porque A cambie de alcance.

## Backlog y aceptación futura

T003/004/027–033/039/040/049/052/054/057/058 permanecen abiertos en [tasks](tasks.md). AC originales05/07/12, parte de06/08/10 y AR-05–06 aplican aquí. La decisión de cierre010-A no los completa ni cancela.

Antes de implementar B: nueva priorización, revisión del ADR/modelo/contratos, evidencia G1/G3 aplicable y autorización de configuración/datos. El protocolo administrado Microsoft→External ID se difiere por separado; no equivale a migración autoservicio entre proveedores ni convierte automáticamente ese trabajo en010-B.

## Migración Microsoft diferida por decisión de beta31/10

**DEFERRED — Microsoft External ID migration**, independiente de la gestión de métodos010-B. A-AC06/07 y T019/T048 siguen abiertos; no bloquean Google CIAM+Microsoft legacy. Se conservan [runbook](migration.md), [ensayo INCONCLUSIVE](evidence/migration-rehearsal.md), tests, runner, journal/reservation y preparación técnica. Federación/destino/reserva/cutover/rollback/manifiestos/retiro legacy requieren una iteración posterior y preservarán exactamente UserId/referencias. No reabrir CA/G3 ni adoptar por email para cerrarlos.

## QA posterior no bloqueante

T053: lector/zoom manual completos. T059: repetición de alta Google con otro tester nunca registrado para nuevos atributos. T060: matriz manual exhaustiva multi-tab/requests tardías. Ninguno se marca probado por diferir; controles básicos de UX, ambos cambios reales y suites siguen siendo obligatorios beta.

T056 continúa A-OPT y Web no anuncia email. Se conserva el selector OTP QA porque comparte configuración/evidencia con el spike; no se ofrece como método vinculado.

## Iniciativa independiente prevista012

`012-group-invitations-collaboration`: buscar usuario, invitar, aceptar/rechazar, notificar, enlaces para no registrados e incorporación a listas compartidas. No implementar ni ampliar010; no bloquea autenticación.
