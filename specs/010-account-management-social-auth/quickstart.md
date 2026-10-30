# Quickstart — revisión e implementación posterior de 010-A

Esta ejecución modifica documentación únicamente. No provisión cloud, migraciones EF, registros, secretos ni pruebas mutadoras.

## Recorrido

1. Revisar [alcance](scope.md), [spec](spec.md), [plan](plan.md), [tareas](tasks.md) y [decisión](evidence/010-a-scope-review.md).
2. Ratificar [ADR-004](../../docs/adr/ADR-004-ciam-identity.md), [TP11 propuesto](constitution-amendment-proposal.md) y [transición Microsoft](migration.md). Constitución permanece intacta hasta aprobación explícita.
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
