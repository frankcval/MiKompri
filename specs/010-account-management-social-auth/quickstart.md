# Quickstart — revisión y QA de 010-A

Código implementado en la rama, aceptación QA pendiente. TP11 1.2.0 aplicada con aprobación explícita. Este documento no autoriza producción ni nuevas configuraciones cloud. [Resultados](evidence/validation.md) y [recursos QA autorizados](evidence/010a-qa-resource-plan.md).

## Recorrido

1. Revisar [alcance](scope.md), [spec](spec.md), [plan](plan.md), [tareas](tasks.md) y [decisión](evidence/010-a-scope-review.md).
2. Revisar aprobación de [ADR-004](../../docs/adr/ADR-004-ciam-identity.md), [TP11 aplicada](../../.specify/memory/constitution.md) y [transición Microsoft](migration.md). No modificar de nuevo constitución ni reabrir G3 para el MVP.
3. Contrastar [auditoría](current-state-audit.md), [research](research.md), [modelo](data-model.md), [contratos](contracts/account-and-identity.md), [amenazas](threat-model.md) y [privacidad](privacy-and-deletion.md).
4. Usar [aceptación](checklists/acceptance.md): diez casos A obligatorios, email opcional y trazabilidad de casos anteriores a [010-B](010-b-deferred.md).
5. G1/G3 y linking se conservan como investigación B. No exigirlos para Microsoft + Google ni repetir transición CA en nombre del MVP.

## Verificación documental

Desde la raíz con PowerShell:

```powershell
./.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks
./.specify/scripts/powershell/check-prerequisites.ps1 -Json -PathsOnly
git diff --check
git status --short
```

Deben resolver010 y conservar los IDs originales de tareas/requisitos. No ejecutar setup-plan sobre documentos existentes. Estas comprobaciones no ejecutan aceptación funcional.

## Orden posterior a aprobación

- Primer login aislado: Microsoft existente + Google sin Microsoft, MSAL.js/PKCE, tokens saneados y rechazos de tokens incorrectos. No necesita broker/linking/G3 ni D1 completo para ese ensayo.
- Users/JWT: tupla de confianza, unicidad y bootstrap seguro, perfil que no sobrescribe; probar transición Microsoft con manifiesto exacto UserId/ownership y rollback. Coexistir no basta como aceptación de migración.
- Antes de acceso productivo protegido: D1 en las tres APIs, tests negativos de scope/issuer/audience/estado/Users caído y regresiones de negocio.
- Web: Google prioritario y Microsoft visible; limpieza de caché/drafts/respuestas tardías en logout/cambio de cuenta; perfil con iniciales.
- Email OTP solo si T056 pasa QA real del mismo CIAM/contrato sin ampliar arquitectura. Si falla, diferir sin bloquear core.
- Ejecutar suites por capas, Web lint/typecheck/build, E2E y QA real con SHA/entorno/versiones/pasos/resultados sin PII ni tokens. MAUI, fotos y recuperación entre proveedores se difieren.

Recuperación en A: proveedor original; ningún soporte MiKompri sustituye una identidad perdida mediante email coincidente. Los recursos QA existentes siguen sujetos al inventario y limpieza documentados; esta revisión no cambia su estado.

## QA local actual

La configuración fuente `.env.example` conserva CIAM deshabilitado. Microsoft usa siempre su registro/authority/scope actuales. Para QA, usar configuración privada por entorno y scope del recurso CIAM, nunca secretos en VITE. En las tres APIs fijar combinaciones por emisor, clientes permitidos y UsersApi; `Authentication:ExternalId:Enabled` y `BusinessEnabled` solo se habilitan en QA tras controles JWT/D1. No sustituir issuer/audience por los de Google.

El ensayo actual reutiliza origen `http://localhost:5173` y su callback `/auth/callback`, APIs QA28082/28080/28083 y PostgreSQL55432 con volumen dedicado. El namespace de sesión distingue QA. Mantener respaldo privado de `.env.local` original y restaurarlo al terminar. No basta con reiniciar Vite si las APIs QA están detenidas: comprobar `/health` en las tres antes de la prueba. No cambiar callbacks legacy ni crear otra app para arreglar un fallo del selector.

Desde `/login`, «Continuar con Google» abre el selector alojado CIAM; elegir Google allí. Se retira domain_hint por AADSTS90023 observado en QA. No recargar la página antigua del error. Microsoft conserva su propia ruta. El flujo QA heredado aún incluye OTP del spike; T056 sigue sin aceptación y no se anuncia en Web ni se promueve ese flujo a producción. Un nombre inicial `unknown` procede del registro CIAM observado; el usuario puede editarlo con «Guardar perfil». Login/sync posteriores deben preservar esa personalización, sin intentar obtener nombres por email.

No activar traslado Microsoft desde la Web: solo runner administrativo QA con equivalencia acreditada y altas pausadas según [runbook](../../tools/identity-migration/README.md). El ensayo real sigue pendiente; no tocar las bases originales para obtener evidencia. Las pruebas reproducibles están en [validación](evidence/validation.md).
