# Aceptación — 010-A beta y trabajo diferido

Decisión de producto explícita31/10/2026: Google sin Microsoft vía External ID + continuidad Microsoft legacy. **Ocho aceptaciones obligatorias**;06/07 conservadas como DEFERRED — Microsoft External ID migration. Ningún PASS de migración. [Spec](../spec.md), [tasks](../tasks.md), [validación](../evidence/validation.md), [proveedores reales](../evidence/real-provider-acceptance.md).

## Casos beta y evidencia

| Caso | Criterio beta | Estado/evidencia |
|---|---|---|
| A-AC01 | Microsoft actual conserva ruta/UserId/tipos admitidos; sin alta CIAM sustituta | PASS: login normal y tres APIs200, Guid esperado |
| A-AC02 | Google sin cuenta Microsoft entra por selector External ID→Google→Web | PASS: titular confirma condición, alta Web y login actual reales |
| A-AC03 | Access token MiKompri en Users/ShoppingList/ProductCatalog, combinaciones explícitas | PASS: Google CIAM y Microsoft legacy200; no se exige Microsoft CIAM en beta |
| A-AC04 | Rechazar Google directo/ID/Graph, issuer/aud/tenant/scope/exp incorrectos | PASS: negativos reales tres APIs401 y suites firmadas |
| A-AC05 | Correlación exclusivamente Users, UserId canónico; no adopción por email/sub/perfil/client targetUserId | PASS: resolución real, negativos Application/API/PG ejecutados |
| A-AC06 | Igual UserId antes/después de traslado Microsoft y rollback, sin INSERT sustituto | DEFERRED — Microsoft External ID migration; INCONCLUSIVE por colisión CIAM, no bloquea beta |
| A-AC07 | Referencias/listas/grupos/memberships/gastos/compras/historial conservados antes/después/rollback | DEFERRED — Microsoft External ID migration; no manifiesto real aceptado, no bloquea beta |
| A-AC08 | Google nuevo obtiene UserId propio, alta única/concurrente, sin adoptar Microsoft por email | PASS beta: alta Google Web acreditada, UserId distinto/estable y negativos/concurrencia PG; repetición con otro tester/atributos T059 diferida según suficiencia documentada |
| A-AC09 | Google logout→Microsoft y Microsoft logout→Google sin mezcla de token/UserId/perfil/caché/sesión | PASS beta: ambas direcciones reales; requests tardías/TanStack/drafts/generación/cuenta/tenant en suites. Matriz manual exhaustiva T060 no acreditada ni bloqueante |
| A-AC10 | Perfil propio prevalece sobre claims; campos/clear sobreviven logout/login | PASS: dos relogin Google con seis campos y clear, sync/GET y tests de autoridad; atributos ausentes no se inventan |

- [x] A-AC01
- [x] A-AC02
- [x] A-AC03
- [x] A-AC04
- [x] A-AC05
- [ ] A-AC06 — DEFERRED — Microsoft External ID migration
- [ ] A-AC07 — DEFERRED — Microsoft External ID migration
- [x] A-AC08
- [x] A-AC09
- [x] A-AC10

El PASS de08 usa evidencia real de nueva alta Web y código relevante idéntico, no solo harness ni existencia de tests. La alta adicional tras configurar nombres opcionales requiere otra cuenta QA admitida/autenticada por su titular; se acepta la evidencia actual para beta sin garantizar nuevos claims. [Decisión de suficiencia](../evidence/real-provider-acceptance.md#suficiencia-de-alta-google-para-beta).

## Gates de entrega beta

- [x] ADR-004/TP11 1.2.0 y coexistencia aprobados; Google usa un CIAM y Microsoft legacy no se retira.
- [x] D1 cuenta actual:200→suspender403→reactivar token viejo401→token iat posterior200; Users caído503, sin allow inter-request. No repetir mutaciones si código/configuración iguales.
- [x] Web Code+PKCE+MSAL.js browser-delegated, callbacks/errores seguros y cuenta activa confiable.
- [x] Negativos y privacidad de login/provisión/perfil/sesión A; sin secretos/PII sensible versionados; recovery en proveedor original.
- [x] Suites y builds ejecutados, EF sin cambios, Docker; CI de los cinco workflows del SHA final se confirma antes de abrir/marcar listo el PR. Los hashes actuales y runs se entregan en su descripción/informe final.
- [x] Básicos UX:320/390/430 sin overflow, botones44px, navegación teclado y mensajes/loading/cancelación/return intent cubiertos por observación y suites.

Lector/zoom manual completos=T053 y matriz manual exhaustiva multi-tab=T060 **DEFERRED NON-BLOCKING QA**, no probados. No bloquean beta por decisión explícita si básicos/suites pasan. Nuevos atributos de otra alta=T059, mismo estado. No se alteran budgets ni aceptaciones007/008/009.

## Email OTP opcional

- [ ] A-AC11/T056 — A-OPT; no habilitado como método Web beta. Falta QA de alta/login/OTP inválido/expirado/reutilizado y sesión. Selector CIAM QA puede conservar OTP heredado para no afectar evidencia; no es método vinculado.

## Fuera de alcance

010-B completo permanece abierto. Invitaciones/grupos son012-group-invitations-collaboration independiente. Sin merge/deploy ni ejecución de migración Microsoft.

## Trazabilidad de los doce casos anteriores

Se conservan los requisitos futuros en [010-B](../010-b-deferred.md). Repartir no equivale a completar:

| Caso original | Destino |
| --- | --- |
| AC-01 Google sin Microsoft | A-AC02/08 |
| AC-02 Email sin Microsoft | A-AC11 opcional; deja de ser gate del núcleo |
| AC-03 Migración conserva UserId | A-AC06 |
| AC-04 Listas/grupos/gastos conservados | A-AC07 |
| AC-05 Añadir otro método | B, T003/027–032/049 |
| AC-06 Mismo email no hace linking | A-AC05/08 impiden correlación automática; pruebas del flujo linking en B/T049 |
| AC-07 Último método no eliminable | B, T028/030/049 |
| AC-08 Nombre/avatar propio prevalece | Campos A-AC10; foto propia/proveedor B/T039/058 |
| AC-09 Token CIAM/no Google directo | A-AC03/04 |
| AC-10 B no vincula identidad a A | Resolución/negativos A-AC05/08; operación de linking B/T027/028/049 |
| AC-11 Logout/cambio cuenta | A-AC09 |
| AC-12 Recuperación conserva UserId | B/T033/052; A solo delega recuperación al proveedor original |

AR-05–06 y ampliación G3 contexto/MFA quedan pendientes B/T004: prueba reciente <=300s, operación <=600s, bootstrap/factor/último método, replay y control alternativo. El [ensayo MFA](../evidence/g3-email-mfa-qa.md) queda INCONCLUSIVE y c1 con política deshabilitada no prueba MFA. No requieren repetirse para A.

## Trazabilidad FR / SC

| Requisitos | Trabajo |
| --- | --- |
| FR-001/012/018/020/021 | T002/005/020–024/047/051; email T056 opcional |
| FR-002/026 | T001/007/008/055; agregación futura T003[B] |
| FR-003/004/005/007/010/011/022 | T013–019/043/048 |
| FR-006/008 y partes de009/024 | T004/027–033/049/052[B] |
| FR-009/010 traslado futuro | T014/017 técnicos conservados; T019/T048 DEFERRED MICROSOFT MIGRATION |
| FR-013/014/015/016/017 | T012/034–038/050[A]; fotos/reimportación T039/058[B] |
| FR-019 | T054[futuro MAUI] |
| FR-023/024 parte login | T020/025/026/041/042/046/051[A] |
| FR-025 | Minimización T006/012/046[A]; eliminación T040[B] |

SC-001→ocho A-AC beta; SC-002→06/07 diferidos; SC-003→04/05/08 y negativos; SC-004→10; SC-005→03/04+D1/regresiones; SC-006→09 y gate UX; SC-007→limitación de recovery en proveedor(A)/AC-12(B); SC-008→ADR/TP11/coexistencia/CI final. Ninguna aceptación B condiciona cerrar A.
