# ADR-004 — CIAM incremental, UserId y transición Microsoft

**Estado:** Aceptado para 010-A por aprobación explícita del titular el 2026-10-30; TP11 1.2.0 aplicada. 010-B/D3, despliegue y merge no autorizados por esta aprobación.
**Fecha:** 2026-10-30; alcance beta ratificado2026-10-31. **Incremento:** [010-A](../../specs/010-account-management-social-auth/spec.md).
[Research](../../specs/010-account-management-social-auth/research.md) · [Revisión de alcance](../../specs/010-account-management-social-auth/evidence/010-a-scope-review.md)

## Contexto

El acceso Microsoft/Entra existente debe continuar. Google debe permitir utilizar MiKompri sin cuenta Microsoft. El spike acredita login Google/OTP y token CIAM en un harness aislado; no acredita todavía alta de dominio, migración Microsoft ni linking seguro. La auditoría inicial encontró una tupla canónica y sincronización que sobrescribía perfil; la implementación010-A ya separa autoridad propia y claims. Cambiar de tenant sin correlación autorizada crearía duplicados.

## Alternativas arquitectónicas e incrementos

**Alternativa A:** External ID autentica y agrega proveedores; Users correlaciona principalmente la tupla CIAM `(tid, oid)` con un `UserId`. Para login básico no se necesita gestionar una colección propia de métodos. La capacidad de linking futuro no queda aprobada por esta elección.

**Alternativa B:** varias identidades externas por UserId en Users. Aumenta unicidad, revocación, recuperación y reconciliación. Solo reconsiderar si una necesidad obligatoria demuestra insuficiencia de A; no introducirla en 010-A.

**010-A/010-B son incrementos de producto**, distintos de esas alternativas: 010-B difiere gestión de métodos, no selecciona automáticamente la alternativa B.

## Decisión aceptada para010-A beta

1. Conservar la ruta Microsoft/Entra actual durante la transición; añadir Google mediante un único External ID. Google autentica; External ID emite el access token MiKompri; ninguna API acepta un token Google directo.
2. Mantener `UserId : Guid` inmutable y todas sus referencias: listas, grupos, memberships, compras, gastos, auditoría e historia. Correlación por identidad de emisor confiable, nunca por email, nombre, username, teléfono, avatar o sub global.
3. Preferir una única tupla canónica activa en User, reutilizando índices y capacidades existentes. No crear colección genérica de proveedores, broker de pruebas ni plataforma de account operations para el MVP.
4. **Migración Microsoft diferida, sin gate beta** por aprobación explícita31/10. Mantener issuer/app registration/configuración/usuarios Microsoft actuales. El [runbook futuro](../../specs/010-account-management-social-auth/migration.md) conserva origen inmutable, reserva, misma fila/UserId/referencias, manifiestos y rollback. A-AC06/07 y T019/T048 no se completan; ensayo INCONCLUSIVE por colisión CIAM. No ejecutar traslado ni retirar legacy en esta beta; tampoco sustituirlo por Microsoft→Google/email.
5. Durante coexistencia, aceptar solo combinaciones explícitas por emisor de issuer/tenant/audience/scopes/clientes en las tres APIs; no producto cartesiano de allowlists. Un recurso backend lógico común y la misma audience por emisor para las tres APIs; no OBO ni tokens sociales directos. Impedir auto-provisión de candidatos Microsoft no reconciliados en la ventana de migración.
6. Web: Authorization Code + PKCE + MSAL.js browser-delegated. Google primero y Microsoft disponible; no mostrar métodos no acreditados. MAUI productivo/elección Native Auth se difieren hasta verificar SDK/versión/retorno reales.
7. Perfil propio básico: Username, DisplayName, GivenName, FamilyName, PreferredLanguage y preferencias funcionales tipadas. Claims inicializan; no pisan personalizaciones. Avatar inicial con iniciales; fotos propias/del proveedor, almacenamiento y reimportación explícita se difieren.
8. Recuperación delegada al proveedor original. Sin reemplazo de identidad desde MiKompri, ni linking, segundo método, unlink, merging o recuperación entre proveedores.

### Email OTP

Elegible como **opción 010-A**, no obligatorio. El harness demuestra login básico mediante el mismo CIAM/token; no requiere G3 ni un modelo adicional para registrar un usuario nuevo. T056 exige QA de aplicación, negativos, sesión y correlación antes de habilitarlo. Si no pasa, diferir sin bloquear Microsoft + Google. OTP primario no es segundo factor MFA.

### D1 — se conserva para acceso productivo protegido

Suspensión prácticamente inmediata en el siguiente control autoritativo tras commit Users; no esperar exp del JWT. Estado de cuenta/canónica por request en las tres APIs, sin allow cacheado entre requests: Users local, consumidores por contrato mínimo. Suspendido403; estado no resoluble503. Corte de emisión/versionado server-side para no revivir tokens al reactivar. No cancelar retroactivamente requests ya autorizadas.

ProductCatalog incorpora dependencia Users aun cuando antes no resolvía UserId en cada operación. Medir latencia/disponibilidad con budgets vigentes. Sin Graph por request, Redis, bus, CAE supuesto ni claims como reemplazo del estado. Implementación completa **antes de acceso productivo protegido**, no requisito para el primer login aislado.

### D3 — se conserva como diseño futuro 010-B

Prueba reciente por operación <=300s, operación <=600s; doble control aplicable, nonce/propósito, un solo uso y alternativa válida antes de unlink. No interpretar token recién emitido, iat, prompt=login, MFA histórica o c1 como prueba suficiente. Linking/unlinking/recovery no se publican en 010-A; por ello G1/G3 no bloquean ese incremento. [Revisión completa](../../specs/010-account-management-social-auth/architecture-review.md).

## Evidencia y límites

Estado31/10: código010-A en la rama, [validación técnica](../../specs/010-account-management-social-auth/evidence/validation.md). Navegador de pruebas completa Google SSO sin OTP tras retirar domain_hint y usar selector CIAM alojado; Microsoft permanece legacy. El desafío adicional del titular y atributos iniciales requieren completar diagnóstico. [Traslado real no acreditado](../../specs/010-account-management-social-auth/evidence/migration-rehearsal.md): no ejecutar cutover ni añadir un desvío Web experimental para suplir falta de equivalencia. La configuración/provisión QA no acredita aceptación global ni autoriza producción.

[Resumen del spike](../../specs/010-account-management-social-auth/evidence/spike-summary.md): Google sin Microsoft y OTP básicos funcionan en harness; rechazo de token Google directo acreditado para el caso probado. No hay migración ni propiedad de dominio aceptadas.

Google reauth dio auth_time nuevo con selección/consentimiento: **NOT RELIABLE FOR G3**. OTP G3 **INCONCLUSIVE**. Probes Graph de agregación no prueban control de métodos, evitan por sí solos carreras ni impiden último método. El [ensayo MFA](../../specs/010-account-management-social-auth/evidence/g3-email-mfa-qa.md) no ejecutó desafío social; c1 firmado con política deshabilitada no demuestra MFA. [Restauración](../../specs/010-account-management-social-auth/evidence/g3-restoration-verification.json): defaults activos, políticas deshabilitadas, contexto sin publicar. No retomar transiciones CA para entregar login básico.

El veredicto histórico **ALTERNATIVE A REQUIRES CHANGES** corresponde al alcance completo con linking; no se convierte en PASS ni obliga a implementar B para login básico.

## Consecuencias

Menor complejidad propia y dominio independiente del proveedor. Beta exige Google CIAM sin Microsoft, continuidad legacy, JWT/MSAL, perfil, aislamiento y D1. Coexistencia es un rollout transitorio con un único CIAM; transición Microsoft no ensayada sigue como riesgo/evolución futura, no bloqueo beta. Un usuario que acceda con otro proveedor puede representar una cuenta distinta; advertir a usuarios Microsoft que usen su ruta existente. Nunca deduplicar automáticamente por email.

Dependencias/costes: operación CIAM y proveedor Google, configuración aprobada por entorno, consumo MAU y condiciones vigentes, QA real y operación de transición. Ninguna compra, SMS, trial, MFA adicional o almacenamiento de fotos es requisito de este alcance. No prometer coste total cero.

## Aprobación y orden

Alcance, transición acotada y TP11 1.2.0 aprobados expresamente el 2026-10-30; [registro A0](../../specs/010-account-management-social-auth/evidence/implementation.md). Orden beta ratificado31/10: Google CIAM + Microsoft legacy; correlación/alta; APIs y D1; perfil/sesión; ocho A-AC obligatorios, CI final y PR. Migración/rollout por cohortes futuros se revisan por separado. Email OTP opcional en paralelo tras QA.

G3, linking, recuperación entre proveedores y MAUI permanecen [010-B](../../specs/010-account-management-social-auth/010-b-deferred.md). El spike histórico no autoriza 010-B ni producción; esta implementación se limita a 010-A.

## Interpretación incremental TP11 — decisión31/10

TP11 permanece **1.2.0**, sin reversión ni nueva enmienda. External ID sigue aprobado como CIAM y Google lo usa en010-A; legacy puede coexistir durante beta. El MUST de preservar UserId/origen/reserva/referencias/rollback se exige **cuando se haga** el traslado futuro; no obliga a ejecutarlo antes de beta. No múltiples CIAM permanentes.

Ambas direcciones reales y tests de caché/drafts/requests tardías/generación/cuenta/tenant bastan para gate de sesión beta sin matriz manual exhaustiva. UX320/390/430/teclado/targets/errores/loading/cancelación obligatoria; lector/zoom completos se registran como QA posterior. Email opcional,010-B y012 no bloquean este cierre. Ninguna decisión autoriza merge/deploy/cutover.
