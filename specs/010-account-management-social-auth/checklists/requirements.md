# Specification Quality Checklist — 010-A beta

Revisión documental31/10/2026 con decisión de producto explícita. TP11 **1.2.0 aplicada y conservada**; [ADR-004](../../../docs/adr/ADR-004-ciam-identity.md), [spec](../spec.md), [plan](../plan.md), [aceptación](acceptance.md), [decisión](../evidence/010-a-scope-review.md).

- [x] Alcance obligatorio Google sin Microsoft vía External ID + Microsoft legacy operativo; no G3/linking.
- [x] A-AC01–05/08–10 son ocho casos beta;06/07 conservados DEFERRED — Microsoft External ID migration, no PASS.
- [x] T019/T048 abiertos/diferidos, fuera de gates obligatorios; tests/runner/journal/reservation/evidencia no eliminados.
- [x] T055 evalúa alcance31/10; no exige traslado real ni autorización de despliegue para PR.
- [x] UserId inmutable/Users dueño de correlación; permisos y referencias íntegros, no adopción por email/perfil/sub global.
- [x] TP11 no revertida: coexistencia transitoria con un CIAM, legacy conservado; futura migración exige origen/UserId/referencias/rollback.
- [x] D1 por request completo y JWT real/sintético; sin Graph por request/Redis/bus/CAE supuesto.
- [x] MSAL.js/Code+PKCE browser-delegated; perfil básico/clear/autoridad propia e iniciales, sin PII futura/storage.
- [x] Ambos cambios reales + tests tardíos/caché/drafts/generación/cuenta/tenant suficientes beta; manual exhaustiva diferidaT060.
- [x] Básicos UX mobile-first obligatorios; lector/zoom completosT053 y alta adicional de atributosT059 diferidos no bloqueantes, sin fingir ejecución.
- [x] EmailT056=A-OPT no anunciado,010-B abierto;12 casos originales trazados,26FR/ochoSC conservados.
- [x] Invitaciones/grupos fuera de010, iniciativa independiente012 prevista.
- [x] Evidencia histórica/G3/INCONCLUSIVE conservada; no nuevos tenants/apps/credenciales/BD ni ejecución de traslado por reorganizar alcance.
- [x] Entorno local010-A con ambos accesos se conserva; no restauración automática incompatible.

La revisión de enlaces, IDs y Spec Kit se ejecuta antes del commit. Aceptación técnica/producto en acceptance.md y validation.md; la checklist documental no inventa pruebas de proveedor, migración, lector o zoom. Los cinco workflows del SHA exacto final deben pasar antes de publicar el PR como listo. Sin merge/deploy automático.
