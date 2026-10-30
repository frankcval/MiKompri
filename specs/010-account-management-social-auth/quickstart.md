# Quickstart — revisión de arquitectura 010

Esta ejecución solo formaliza. No ejecutar scripts de provisión, migraciones EF/database update, llamadas Graph mutadoras, nuevos registros, credenciales ni login/proveedores reales sin autorización posterior.

## Recorrido de revisión

1. Leer [spec](spec.md), [auditoría actual](current-state-audit.md) y [research](research.md). Contrastar fecha y aplicabilidad external tenant, no B2B/B2C.
2. Revisar [ADR-004](../../docs/adr/ADR-004-ciam-identity.md), A/B y G1–G7; aprobar por separado spike QA y decisión final.
3. Revisar [TP11 propuesto](constitution-amendment-proposal.md) sin alterar la constitución vigente.
4. Contrastar [modelo](data-model.md), [migración](migration.md), [contratos](contracts/account-and-identity.md), [amenazas](threat-model.md) y [privacidad/borrado](privacy-and-deletion.md).
5. Usar [plan](plan.md), [tasks](tasks.md) y [doce casos de aceptación](checklists/acceptance.md). Ningún test de proveedor está ejecutado por existir estas listas.

## Comprobación documental local

Desde la raíz, con PowerShell:

```powershell
./.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks
./.specify/scripts/powershell/check-prerequisites.ps1 -Json -PathsOnly
git diff --check
git status --short
```

Deben resolver 010; verificar `.specify/feature.json` con `feature_directory: specs/010-account-management-social-auth`. Revisar enlaces locales/anchors y trazabilidad de 26 FR/ocho SC/doce AC. No ejecutar `setup-plan` sobre documentos ya formalizados para evitar sobrescribirlos con plantilla.

## QA futuro, después de revisión y autorización

Preparar tenant external separado, registro recurso backend común, clientes SPA/broker/MAUI cuando corresponda y proyectos de proveedor. Valores/secretos fuera de Git; no reutilizar sesiones/productivo. Capturar metadatos y claims **redactados** por emisor/cliente/scope. Probar G1/G2/G3 antes de fijar modelo A/B. Email y Google deben ser reales; Microsoft organizacional legado y MSA personal son pruebas distintas.

Ensayar migración sobre PostgreSQL Docker con dataset desechable y manifiesto de referencias; después QA real autorizado. Evidencia: SHA, SDK/versiones, entorno, pasos/resultado, fallos y comparación de IDs, sin tokens/OTP/authorization codes/PII. Ejecutar suites por capas, Web lint/typecheck/build, E2E controlados y aceptación separada. No convertir falta de recursos en skip aprobado. MAUI sigue fuera de implementación inicial.
