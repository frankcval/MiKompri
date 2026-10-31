# Decisión de alcance beta010-A — 31/10/2026

**Aprobación explícita del titular:** Google sin cuenta Microsoft mediante External ID es la prioridad funcional; Microsoft legacy permanece operativo. Microsoft→External ID deja de ser gate beta. TP11 **1.2.0 se conserva**, con interpretación incremental aceptada en [ADR-004](../../../docs/adr/ADR-004-ciam-identity.md).

## DoD vigente

Doce gates de producto: Google sin Microsoft, CIAM, token MiKompri, UserId interno, tres APIs, rechazo Google directo, continuidad legacy, aislamiento, perfil propio, D1, CI SHA final y cero secretos/PII sensible versionados. Se trazan en ocho A-AC01–05/08–10 y [tasks](../tasks.md).

**A-AC06/A-AC07 = DEFERRED — Microsoft External ID migration.** T019/T048 siguen abiertos sin completarlos; federación/destino/reserva/cutover/rollback/manifiestos/retiro legacy quedan futuros. [Ensayo INCONCLUSIVE por colisión CIAM](migration-rehearsal.md) conservado íntegro. No borrar tests, runner, journal/reservation ni infraestructura. No ejecutar una nueva migración ni usar email para adoptar al piloto.

## Suficiencia beta y riesgos aceptados

- Google real actual llega mediante selector/Google a Web y APIs200; ID token401; UserId estable/distinto de Microsoft y perfil/clear conservados. Alta nueva Google Web ya acreditada; otra alta tras nuevos atributos requiere tester admitido/autenticación manual del titular no disponible en esta sesión. Se acepta evidencia existente para beta, con T059 QA posterior, sin prometer nombres si el CIAM no los entrega.
- Ambos cambios reales Google↔Microsoft más tests de requests tardías/TanStack/drafts/generación/cuenta/tenant cierran sesión beta. La matriz manual exhaustiva multi-tab permanece T060, riesgo residual y no blocker.
- Básicos320/390/430 sin overflow, targets44px, Tab, nombres/mensajes/loading/cancelación pasan por observación y suites. Lector/zoom completos T053 siguen sin ejecutar; QA posterior no bloqueante.
- D1 real200→403→401→200 y503 con Users caído corresponde a código/configuración relevante sin cambios; no repetir suspensiones. Árboles git contrastados con a9ca37d y candidato6dd53e0; esta revisión modifica únicamente documentación.
- No configuración Azure/CIAM, métodos, secretos, credenciales, bases/modeloEF ni código de negocio nuevos; entorno QA funcional se deja activo. Microsoft conserva issuer/app/configuración/usuarios y compatibilidad. Coexistencia transitoria con un único CIAM.

EmailT056 sigue A-OPT/no anunciado en Web.010-B completo fuera. Invitaciones/grupos se reservan para012-group-invitations-collaboration independiente, sin ampliar010. No cambian completitud007/008/009 ni budgets.

## Auditoría y publicación

Base remota6dd53e0080bd7547ce167752883e56f6430e9ebc coincide con rama local; main...HEAD=0/12. Cinco workflows success de esa base. Se escanean secretos/enlaces/IDs, se valida Spec Kit y se verifica **de nuevo** CI del commit documental final antes de crear/actualizar el PR. No usar el SHA base como CI final. [Validación](validation.md), [aceptación](../checklists/acceptance.md), [proveedores](real-provider-acceptance.md).

## Revisión histórica conservada

<details>
<summary>Revisión30/10 y reconciliaciones anteriores: no definen gates beta vigentes</summary>

# Revisión de alcance — 010-A Microsoft + Google

> Actualización2026-10-30: aprobación explícita de010-A/ADR/TP11 y transición; TP11 **1.2.0 aplicada**. **010-A READY FOR IMPLEMENTATION**. El resto conserva la propuesta/revisión previa; [registro actual](implementation.md).

**Fecha:** 2026-10-30. **Rama:** `010-account-management-social-auth`. **Base:** `010d298daae4815e6db00b88ab00e507b4c78b3e`. Revisión documental posterior al spike, sin implementar ni configurar recursos. Hay cambios del ensayo QA anterior en el árbol; se conservan, sin atribuirlos a esta ejecución.

## 1. Alcance obligatorio 010-A

Conservar Microsoft/Entra existente; añadir Google prioritario sin cuenta Microsoft mediante External ID y access token MiKompri. Users mantiene tupla externa confiable→UserId Guid inmutable. Diseñar y ensayar transición Microsoft con igualdad exacta de UserId y todas sus referencias, sin linking general. Web MSAL.js/Code+PKCE browser-delegated, JWT/scopes en tres APIs, perfil básico propio, sesión aislada/logout y regresiones de propiedad. [Spec](../spec.md), [plan](../plan.md).

Perfil: Username público opt-in distinto del login, DisplayName, GivenName/FamilyName opcionales, PreferredLanguage y preferencias tipadas con finalidad actual. Claims inicializan; nunca pisan personalizaciones ni borrados voluntarios. Iniciales; sin fotos/storage ni PII futura.

## 2. Diferido 010-B

Segundo método, link/unlink, cambio de proveedor, último método, recovery por alternativa, broker/AccountOperation/ProofReceipt, G3/step-up, Authentication Context/CA de linking y MFA avanzada. Avatar propio/proveedor/reimportación, borrado operativo y MAUI productivo también se difieren. Apple/Facebook/passkeys/SMS y tipos Microsoft nuevos no se activan por disponibilidad teórica. Alternative B solo tras insuficiencia demostrada de A y otro ADR; account merging sigue fuera. [Backlog conservado](../010-b-deferred.md).

## 3. Proveedores incluidos

| Proveedor | Decisión |
| --- | --- |
| Microsoft/Entra actual | Obligatorio, disponible para usuarios existentes y tipos admitidos actualmente. Mantener ruta hasta cutover acreditado; no ampliar implícitamente a cualquier tenant/MSA |
| Google | Obligatorio y prioritario. Google→External ID→access token MiKompri→tupla CIAM→Users→UserId; no necesita cuenta Microsoft |
| Email OTP | Opcional elegible de A tras T056; mismo CIAM/token/modelo, nueva alta simple sin linking/G3 |
| Otros | Diferidos; no coste/credenciales/proveedor nuevo por esta revisión |

No mostrar métodos no acreditados, también en el flujo del IdP. Advertir que quien ya usa Microsoft debe mantener esa ruta para conservar su cuenta mientras no exista traslado autorizado.

## 4. Email OTP

**Sí puede incluirse sin arquitectura adicional**, como login básico de usuario nuevo en el mismo CIAM. El harness ya demuestra registro/acceso/reauth con OTP, pero no aceptación integrada completa. T056 exige alta/login posterior, OTP incorrecto/expirado/reutilizado, correlación estable y aislamiento de sesión. Incluir solo tras QA y aprobación; si falla o retrasa, dejar deshabilitado/diferir sin bloquear Google+Microsoft. No agregar Google+email al mismo usuario en este incremento. OTP primario no es MFA.

## 5. ADR

[ADR-004](../../../docs/adr/ADR-004-ciam-identity.md) propone alternativa A mínima para login, una canónica activa y registro acotado de traslado Microsoft. Sin colección genérica de identidades ni broker preventivo. D2 Web permanece; D1 antes de negocio productivo; D3 queda como requisito futuro sensible. Estado **Propuesto**, no aceptado.

La transición usa federación del mismo origen Microsoft acreditado por identidad inmutable, no prueba arbitraria de Microsoft+Google. La [guía oficial](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-entra-id-federation-customers) permite proponer precreación federada; la equivalencia real/issuer y la preservación del UserId siguen requiriendo ensayo. Sin prueba suficiente se detiene cutover y conserva Microsoft, nunca se deduce por email.

## 6. TP11

[Propuesta](../constitution-amendment-proposal.md) 1.1.0→1.2.0 MINOR, sin aplicar. Permite CIAM/Google y coexistencia acotada, mantiene UserId, correlación Users, recurso común, confianza explícita, permisos por contexto y cero cross-DB. Linking/unlinking tienen garantías condicionales para cuando se ofrezcan; no son obligación del primer incremento. Email OTP no se convierte en obligación constitucional.

## 7. Qué bloquea realmente el MVP

**Para iniciar el cambio transversal:** ratificar ADR-004 y propuesta TP11, con contrato de confianza/coexistencia y transición Microsoft acotada (T007/T008/T009/T010). Es un único bloque de aprobación arquitectónica pendiente, no un bloqueo G3. Después, aplicar el proceso constitucional autorizado según gobernanza antes de cambiar la identidad productiva.

**Para entregar el producto:** implementar y acreditar las tareas A: Microsoft actual, Google real, resolución/alta única, JWT/scopes, transición/rollback con mismos IDs, perfil/sesión y negativos/regresiones. D1 es obligatorio antes de negocio productivo protegido; no bloquea primer login aislado. Son trabajo futuro planificado, no nuevas incertidumbres arquitectónicas que obliguen a completar B antes de empezar. [Tasks](../tasks.md), [diez aceptaciones](../checklists/acceptance.md).

## 8. Qué deja de bloquear A

T003/T004, T027–033, T039/T040, T049/T052/T054 y T057/T058 se conservan abiertos y diferidos. No se exige resolver Google reauth/G3, activar CA/contexto, mutar Graph identities, implementar último método o recuperación alternativa, probar MAUI ni desplegar avatar para entregar Microsoft+Google. T056 email opcional puede omitirse. Partes sensibles de tareas anteriores se trazan en tasks; no se borran requisitos ni se marca nada completo.

El spike sigue mostrando Google G3 **NOT RELIABLE FOR G3**, candidato MFA social **INCONCLUSIVE** y c1 con política deshabilitada sin prueba MFA. **ALTERNATIVE A REQUIRES CHANGES** continúa siendo el dictamen del alcance completo de agregación; no se cambia a PASS. La [restauración](g3-restoration-verification.json) queda intacta: defaults activos, políticas disabled, contexto sin publicar. No hay nuevo desafío ni cambio cloud.

## 9. Riesgos y dependencias residuales

- La misma persona puede tener distintas cuentas si se registra con otro proveedor; no hay deduplicación por email ni recuperación cruzada. UI debe comunicar el uso de la cuenta Microsoft existente; soporte no reasigna identidad perdida.
- La transición Microsoft aún no está probada en dominio. Antes de cortar: origen inmutable, reserva única, guard contra auto-provisión, manifiesto y rollback. Coexistencia por sí sola no acredita T048 ni autoriza confianza legacy indefinida.
- Dos emisores temporales requieren matrices estrictas, sin combinar issuer viejo con audience nueva. Tres APIs conservan el mismo recurso lógico y autorización de negocio; no tokens sociales directos.
- D1 añade dependencia Users a ProductCatalog y fail-closed; medir budgets/disponibilidad antes de habilitar negocio protegido, sin Graph por request/Redis/bus/CAE.
- SyncClaims actual sobrescribe perfil y la provisión necesita separar resolución/alta/traslado; no presentarlo como capacidad ya conectada. La aprobación no equivale a corrección implementada.
- CIAM/Google requieren configuración por entorno y QA real posterior autorizado. Revisar consumo MAU/condiciones y responsabilidades operativas; no presumir coste total cero ni añadir SMS/trial/licencia como requisito. Recursos QA anteriores conservan plan de limpieza previsto antes de2026-11-06, sin tarea automática creada.
- Recuperación depende del proveedor original. Pérdida total puede impedir acceso; no prometer restitución desde MiKompri. Email OTP, si se habilita, añade sus propios riesgos de abuso/entrega y negativos QA, no un modelo nuevo.

## 10. Orden recomendado

1. A0: ratificar ADR/TP11, matriz de confianza y transición acotada; revisar amenazas/coste/perfil mínimos.
2. A1: Microsoft existente + Google en login aislado MSAL/PKCE, contrato CIAM y negativos de token, sin G3 ni D1 completo.
3. A2: correlación/alta idempotente y traslado Microsoft administrado; probar IDs/ownership/rollback y guard de provisión. Microsoft conserva ruta hasta cutover acreditado.
4. A3: integrar tres APIs y D1 antes de negocio productivo, regresiones/permisos/latencia.
5. A4: perfil básico y sesión/logout/cambio de cuenta, iniciales y autoridad por campo.
6. A5: diez aceptaciones reales, suites/CI del candidato, revisión y rollout por cohortes. T056 email opcional en paralelo si no retrasa; B se prioriza aparte.

## Validación de esta revisión

- Spec Kit: `check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks` y `-Json -PathsOnly`, ambos exit0 y FEATURE_DIR correcto010. PathsOnly devuelve BRANCH vacío en esta sesión; `git branch --show-current` confirma independientemente `010-account-management-social-auth` y HEAD base indicado arriba. No se cambia el script ni la selección activa.
- Verificación local de 29 Markdown: 228 enlaces locales y dos anchors comprobados, cero rotos.
- 58 tareas únicas, todas abiertas; T001–T055 conservadas una sola vez y T056–T058 añadidas. Seis historias, 26 FR y ocho SC con IDs únicos. Diez A-AC obligatorios y A-AC11 opcional; doce AC originales trazados sin aceptaciones ficticias.
- SHA256 de 52 archivos protegidos comparados con el inicio: cero cambios. Incluye constitución, feature.json, toda la evidencia previa salvo la ampliación interpretativa de spike-summary y todos los archivos fuente del harness aislado.
- `git diff --check` sin errores. Los cambios de esta ejecución son solo documentación; los cambios de harness/QA que ya estaban en el árbol son anteriores y se conservaron. No se ejecutan suites funcionales en una revisión documental ni se presentan pruebas del spike como nuevas.

Archivos de esta revisión: spec, plan, tasks, scope, research, architecture-review, data-model, migration, contracts/account-and-identity, ADR-004, propuesta constitucional, quickstart, privacy-and-deletion, threat-model, checklists/acceptance y requirements, ampliación final de spike-summary, nuevo010-b-deferred y este informe. No producción, cloud, BD, EF, secretos o configuración nueva; sin cambios de completitud007/008/009.

**010-A NOT READY FOR IMPLEMENTATION** — único bloqueo indispensable previo: aprobación explícita de ADR-004/propuesta TP11 y transición Microsoft acotada descrita en ellos. G3, linking, email opcional y MAUI no bloquean Microsoft+Google.

## Ratificación010-A — 31/10/2026

El estado previo de este documento es histórico. El titular aprobó010-A/ADR-004/TP11 el30/10; implementación y validación se revisan ahora sobre a9ca37d, manteniendo010-B diferida. Auditoría remota inicial46cf coincidente con origin,0/9 respecto a main; fuentes vigentes en research.md, evidencia actual en implementation.md/validation.md/real-provider-acceptance.md. No se usa la aprobación para cerrar migración real ni accesibilidad, y no se modifica aceptación007/008/009. T001 ratifica auditoría/alcance, no certifica entrega.

</details>
