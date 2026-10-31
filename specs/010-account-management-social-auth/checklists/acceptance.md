# Aceptación — 010-A y trazabilidad 010-B

**Estado31/10:** aceptación integrada parcial; [validación técnica](../evidence/validation.md) y [login Google real confirmado](../evidence/authentication-recovery-2026-10-31.md). Se conservan las casillas de aceptación abiertas hasta cerrar todos los pasos del candidato y su evidencia; el traslado Microsoft real no está acreditado. Un PASS en harness no sustituye esos recorridos. [Spec](../spec.md), [tareas](../tasks.md), [alcance](../evidence/010-a-scope-review.md).

## Diez casos obligatorios de 010-A

| Caso | Evidencia requerida sobre candidato | Requisitos / tareas |
| --- | --- | --- |
| A-AC01 Microsoft actual sigue entrando | Login real por ruta existente, sin redirección que cree otra cuenta; tipos actuales conservados | US1; FR-001/011; T002/023/047 |
| A-AC02 Google nuevo sin Microsoft | Titular confirma que no necesita cuenta Microsoft; login Web normal mediante CIAM | US1; FR-001/018; T002/024/047 |
| A-AC03 Access token válido MiKompri | Ambos recorridos aceptados por Users, ShoppingList y ProductCatalog bajo su matriz explícita; CIAM en Google y tras traslado Microsoft; excepción legado durante coexistencia | US5; FR-020/021; T020/021/051 |
| A-AC04 Rechazo Google directo | Token Google directo401; añadir negativos Graph, ID token SPA, issuer/audience/tenant erróneos, exp y scope ausente/incorrecto | US5; FR-020/021; T020/051 |
| A-AC05 Identidad resuelta a UserId | Tuple confiable se resuelve solo en Users; sin email/sub global ni targetUserId elegido por cliente; destino ocupado deniega | US2/5; FR-003/004/005/007; T013–018/051 |
| A-AC06 Microsoft conserva exactamente UserId | Manifiesto antes/después de traslado a CIAM y rollback, igualdad exacta del Guid, no INSERT sustituto | US2; FR-003/010/011; T014/017/019/048 |
| A-AC07 Datos conservan mismo propietario | Listas/grupos/memberships/gastos/compras/AddedBy/PaidBy/PurchasedBy/historial comparados antes/después y tras rollback, sin pérdida de compras posteriores | US2; FR-003/022; T019/043/048 |
| A-AC08 Google nuevo obtiene nuevo UserId | Alta intencional única e idempotente/concurrente; no apropiación de cuenta Microsoft por email coincidente | US1/2; FR-004/005; T013/017/018/047 |
| A-AC09 Logout/cambio no mezcla datos | A→B, respuesta tardía, multi-tab/recarga y cambio de issuer; limpiar tokens/caché/borradores y generación de sesión | US5; FR-018/023/024; T025/051 |
| A-AC10 Perfil personalizado prevalece | Claims inicializan; cambiar DisplayName/nombres/idioma propios, repetir login/GET/sync, conservar personalizaciones y borrados voluntarios | US4; FR-013/014/017; T034/036/038/050 |

- [ ] A-AC01
- [ ] A-AC02
- [ ] A-AC03
- [ ] A-AC04
- [ ] A-AC05
- [ ] A-AC06
- [ ] A-AC07
- [ ] A-AC08
- [ ] A-AC09
- [ ] A-AC10

Google/token del harness aporta evidencia parcial a02/03/04, no sustituye estos recorridos ni permite marcar tareas. Mantener Microsoft legado no cierra06/07: deben ensayarse traslado y rollback.

## Email OTP opcional

- [ ] **A-AC11:** si T056 se aprueba, una persona nueva sin Microsoft entra por email OTP en la aplicación; mismo CIAM/contrato y UserId estable entre accesos. Alta/login posterior, OTP inválido/expirado/reutilizado y aislamiento de sesión probados. Sin linking/G3.
- [ ] Decisión explícita de habilitar tras QA o dejar deshabilitado/diferido. Ningún fallo de esta opción bloquea Microsoft + Google.

## Gates de entrega A

- [ ] ADR/TP11 y transición ratificados; no retirar Microsoft a usuarios no migrados.
- [ ] AR-01–03 D1 antes de negocio productivo: suspensión con JWT vigente, caída Users503 sin allow anterior, reactivación/corte anti-resurrección y métricas ProductCatalog con budgets vigentes. Única canónica A; pruebas de varios bindings solo si en el futuro existen.
- [ ] AR-04 Web: Code+PKCE+MSAL.js browser-delegated. SDK/Native Auth/deep links MAUI se difieren.
- [ ] Negativos de login/provisión/migración/perfil/sesión y seguridad/privacidad aplicables A; recuperación explícita en proveedor original.
- [ ] Suites Domain/Application/API/PostgreSQL, Web unit/component/integration/E2E, lint/typecheck/build; CI del SHA exacto y modelo EF coherente.
- [ ] QA móvil320/390/430, teclado/lector/zoom y errores/cancelación/return intent reales donde corresponda; no alterar budgets ni cerrar gates de otras features.

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
| FR-009 parte traslado | T014/017/019[A] |
| FR-013/014/015/016/017 | T012/034–038/050[A]; fotos/reimportación T039/058[B] |
| FR-019 | T054[futuro MAUI] |
| FR-023/024 parte login | T020/025/026/041/042/046/051[A] |
| FR-025 | Minimización T006/012/046[A]; eliminación T040[B] |

SC-001→10 A-AC; SC-002→06/07; SC-003→04/05/08 y negativos; SC-004→10; SC-005→03/04+D1/regresiones; SC-006→09 y gate UX; SC-007→limitación de recovery en proveedor(A)/AC-12(B); SC-008→ADR/TP11/transición. Ninguna aceptación B condiciona cerrar A.
