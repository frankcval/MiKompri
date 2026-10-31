# Implementación 010-A — registro del candidato

2026-10-30; inicio sobre0559a46, rama010-account-management-social-auth.

## A0: aprobación y constitución

El titular aprueba expresamente ADR-004 para010-A, transición Microsoft acotada, enmienda TP11 1.2.0, D1 y Web MSAL/PKCE; autoriza implementación y commits. No aprueba010-B, producción, merge ni retiro definitivo legacy. Se aplica su autorización explícita; no se inventa issue/PR aprobado. El PR de la rama mantiene revisión posterior.

Proceso de .github/agents/speckit.constitution.agent.md: constitución leída, sin extensions.yml/hooks; TP11 ampliado MINOR1.1.0→1.2.0, fecha2026-10-30, sync report y templates spec/plan/tasks propagados. No existe templates/commands; prompts remiten a agentes y no fijan otro proveedor. Otros principios/gates permanecen.

**010-A READY FOR IMPLEMENTATION**. Ninguna tarea funcional se completa por esta aprobación. Este registro se ampliará con fallos esperados, implementación y resultados del candidato.

## Validación y aceptación

Pendiente de implementar/ejecutar. Harness Google/OTP previo no es aceptación de aplicación. EmailT056 permanece A-OPT deshabilitado. Toda evidencia G3 se conserva, sin retomar CA.

A0 validado: Spec Kit -Json -RequireTasks -IncludeTasks resuelve010, exit0; git diff --check exit0. T007/T008/T009/T010 cerradas por aprobación explícita y TP11/templates aplicados, no por tests de proveedor. Restantes tareas abiertas.

## A1 y primeros controles Users

Web: tests iniciales de rutas/boundary fallan por módulos aún ausentes; tras implementar, auth unit/component33/33 y typecheck exit0. Regresión existente esperaba login sin prompt: actualizada a select_account, opción explícita para evitar selección silenciosa de cuenta. Nuevos tests de botones Google primero/Microsoft conservado y configuración incompleta añadidos; aceptación real pendiente. domain_hint=google se verifica en documentación oficial de métodos External ID (issuer acceleration), no google.com/B2C.

Vitest5 no encuentra tests con el antiguo --dir en scripts unit/component/integration; se corrigen a filtros de rutas. No se omiten tests para lograr verde.

Users: dos negativos fallaron en ejecución antes del cambio (claims sobrescribían nombre; AssignCanonicalIdentity permitía cambio), luego7/7 invariantes/perfil/estado pasan. Nuevas firmas de perfil/estado/reserva produjeron errores de compilación esperados antes de implementarlas. Application35/35 tras separar resolve/provision, hacer fallback explícito y bloquear candidatos reservados. No se interpreta esta validación como aceptación PostgreSQL/cloud/migración completa.

## A2/A3: evidencia técnica local (aceptación humana aún pendiente)

- Domain21/21, Application36/36 (incluye test que falló por PII en logging antes de retirar destructuring).
- Users API51/51 antes de añadir PostgreSQL/firmas completas; ejecución con PostgreSQL54/54, sin skips.
- PostgreSQL Docker16 aislado, puerto55432/loopback y tmpfs:3/3. Ocho altas concurrentes convergen en un Guid; reserva bloquea alta candidata; cutover/rollback mantienen UserId/grupo/membresía y perfil; destino ocupado/origen no acreditado niegan. Verificador externo sustituido: NO prueba federación Microsoft real.
- JWT16/16 criptográficos y13/13 por pipeline HTTP real en cada una de Users/ShoppingList/ProductCatalog (RSA sintético, sin proveedor real). Se corrigió inicialización anticipada de configuración por options diferidas; no se debilitó validación para tests.
- D1 Users: negativos reprodujeron acceso200 de suspendido y ausencia de access-status antes del cambio. Endpoint de lectura ahora no aprovisiona; suspensión403, reactivación no revive token sin iat. ProductCatalog8/8 en GET/mutación/fallo/incompleto/no caché entre requests. ShoppingList reutiliza UserId de D1 por request.
- Migración aditiva20261030212332_AccountExperience010A, journal específico y locks transaccionales compartidos con provisioning. Sin cambios de BD local existente ni cloud de identidad. Runner administrativo QA compilado0warnings/0errores; no nuevo endpoint/linking.

## A4: perfil y sesión en curso

Tests de perfil API reprodujeron versión ignorada y campos ausentes; luego pasan con If-Match/412/428, campos omitidos preservados, clear explícito y claims sin sobrescritura. Editor de perfil reproduce ausencia de campos antes de implementarlo; iniciales, sin foto ni gestión de métodos. Orden por defecto usa criterios existentes de listas, no nuevas preferencias sin uso.

Corregidos selección de cuenta activa vs primera cuenta cacheada, evento acquireTokenSilent tardío que no puede cambiar cuenta activa, múltiples cuentas sin selección, cache/drafts por entorno/cliente/tenant/cuenta, cancelación/generación y respuesta JSON tardía. MSAL fijado a versiones ya instaladas3.30.0/2.2.0. Regresiones Web completas siguen en ejecución; fixtures deben representar cuenta seleccionada y calentar módulo lazy para no medir compilación fría como latencia de UI. No se amplían budgets ni retries.

## Preparación de aceptación real

Reutilizar exclusivamente tenant/apps CIAM QA del spike. Lectura Graph de SPA QA200: solo redirect localhost55412 existente, sin cambios todavía. Plan: añadir únicamente localhost5173/auth/callback y localhost5173/ preservando55412; snapshot privado .local/010a-spa-before.json. Mantener Microsoft con sus registros/redirects actuales. Google usa domain_hint=google; no activar email UI/T056 ni CA/G3.

Web QA usará temporalmente origen5173 con respaldo privado/restauración de .env.local y APIs aisladas28080/28082/28083 en PostgreSQL desechable. No repuntar/modificar BD existentes ni producción; separar namespace de caché por entorno. Configuración backend nueva permanecerá disabled por defecto y BusinessEnabled solo se habilitará en QA tras controles A3. No nuevos tenants/apps/secretos/permisos por esta preparación.

Se detectan discos C/E con1/1.5GB libres. Limpieza únicamente de build cache Docker no utilizado recupera17.23GB internos; imágenes/contenedores/volúmenes conservados. Artefactos privados excluidos también del contexto Docker.

## Continuación 31/10 y corrección de autenticación

Código técnico en `de89da5` y Web/perfil/sesión en `78b4a23`, publicados en la rama. Resultado local: .NET406/406 (incluye PostgreSQL4/4), Web137/137 y Playwright controlado120/120 en tres motores; EF sin cambios de modelo pendientes en las tres APIs. No son aceptación del traslado Microsoft real. Se mantienen abiertos sus gates y los recorridos reales incompletos.

El reinicio y el error Google comunicado por el titular se documentan en [recuperación de autenticación](authentication-recovery-2026-10-31.md). El texto anterior `domain_hint=google` registra la preparación histórica: se ha retirado ese parámetro del código por el rechazo AADSTS90023 observado. La ruta actual usa selector CIAM alojado. Tras la corrección: auth49/49, Web138/138 y lint/typecheck/build exit0; pendiente confirmación humana del recorrido nuevo y CI del nuevo SHA. No se cierra tarea por existencia de código/test.

## Revisión de cierre del31/10 — candidato a9ca37d

Auditoría inicial:46cf0babcbe20677a42eb9dd120d90adbf307642 coincidía con origin/010 (0/0); origin/main...HEAD=0/9. Los cinco workflows de ese SHA terminaron success. Se revisan tareas, ADR/TP11, research, migración, perfil, JWT/MSAL y evidencia; no se atribuye al candidato nuevo una CI anterior. .specify/feature.json sigue correctamente en010. No cambian completitud007/008/009 ni alcance010-B.

Correcciones acotadas: prueba auth primero falla esperando prompt=login; Google abre ahora acceso CIAM explícito, Microsoft conserva select_account. El texto explica el segundo botón Google alojado. No se usa hint ni se promete reautenticación upstream/G3. Tres nuevas pruebas PostgreSQL de idempotencia, fallo previo al traslado y rollback con nuevas escrituras pasan. El harness QA estaba roto por falta de referencia a MiKompri.Identity: falla antes con3 errores, después compila y CI Users añade su build. No cambia JWT/D1/modelo EF productivo en esta continuación.

[Pruebas finales](validation.md), [proveedores/perfil](real-provider-acceptance.md), [negativos y D1](security.md) y [migración/restauración](migration-rehearsal.md) contienen los resultados. Las mutaciones cloud se limitaron al flujo/objeto federado QA descritos antes de ejecutarlas; no se reabren CA/G3, no hay nuevos secretos/roles/tenants ni cutover de usuarios. El registro temporal closure-2026-10-31 se integra aquí y en los documentos existentes para evitar duplicación.
