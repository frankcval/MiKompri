# 010 — Alcance incremental

Revisión30/10/2026, solo documentación. [Spec](spec.md), [plan](plan.md), [tasks](tasks.md), [decisión y trazabilidad](evidence/010-a-scope-review.md).

**010-A:** conservar Microsoft existente; Google prioritario sin cuenta Microsoft mediante External ID/token backend/tupla→Users→UserId; transición Microsoft controlada sin duplicados; Web MSAL/browser-delegated; perfil básico propio; logout/cambio de cuenta y regresiones de ownership. D1 antes de negocio productivo, sin exigirlo a primer login aislado.

**Email OTP:** elegible en el mismo A como opción no bloqueante, tras QA de T056 y aprobación; mismo CIAM/contrato/provisión. Login básico funciona en harness. Sin G3/linking ni modelo adicional. Si falla/retrasa Microsoft+Google, diferirlo y no mostrarlo.

**010-B:** segundo método, link/unlink/cambio de proveedor, recovery con alternativa, G3, Authentication Context/CA de linking, posibles providers/MFA avanzados y B arquitectónica solo por insuficiencia demostrada. [Backlog conservado](010-b-deferred.md). Merging sigue fuera.

UserId Guid inmutable y Users dueño de correlación. No identidades Google/email/sub en ownership; no asociación por email. Perfil propio prevalece sobre claims. Avatar iniciales; foto/storage, DOB/teléfono/dirección y reimportación UI fuera del primer incremento.

No se implementa ni configura nada por esta revisión. [Spikes](evidence/spike-summary.md) conservados, con G3 no demostrado; eso no bloquea el alcance A sin operaciones de métodos. [ADR](../../docs/adr/ADR-004-ciam-identity.md)/[TP11 propuesto](constitution-amendment-proposal.md) pendientes de aprobación; constitución vigente intacta.
