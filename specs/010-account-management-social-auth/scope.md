# 010 — Alcance incremental aprobado para beta

Revisión31/10/2026. [Spec](spec.md), [plan](plan.md), [tasks](tasks.md), [decisión/evidencia](evidence/010-a-scope-review.md), [ADR-004](../../docs/adr/ADR-004-ciam-identity.md).

**010-A beta:** Google prioritario sin cuenta Microsoft mediante External ID→token MiKompri→Users→UserId Guid inmutable; Microsoft legacy conserva issuer/registro/configuración/UserId actuales. Web MSAL/Code+PKCE browser-delegated, tres APIs/JWT explícito, D1, perfil propio, logout/cambio y regresiones de ownership. La beta **no necesita migrar Microsoft a External ID**.

**DEFERRED — Microsoft External ID migration:** A-AC06/07, T019/T048, federación/destino/reserva/cutover/rollback reales/manifiestos/retiro legacy. Ensayo INCONCLUSIVE por colisión CIAM, sin PASS/FAIL reinterpretado; runner/tests/journal/reservation/infraestructura y evidencia conservados. La futura migración deberá preservar exactamente UserId/referencias. Coexistencia transitoria por rollout con un único CIAM, no múltiples CIAM permanentes.

**A-OPT:** email OTP/T056 opcional y no anunciado en Web. Selector QA puede conservarlo para no afectar evidencia compartida; no habilitar para cerrar beta.

**DEFERRED NON-BLOCKING QA:** lector/zoom manual completosT053, alta Google adicional para nuevos atributosT059 y matriz manual exhaustiva multi-tabT060. Básicos320/390/430/teclado/targets/errores/loading/cancelación, ambos cambios reales y cobertura automatizada adecuada sí son gate beta.

**010-B:** linking/unlinking/cambio de proveedor/segundo método/G3/Authentication Context/recovery entre proveedores/MFA avanzada/passkeys/SMS/Apple/Facebook/avatar storage/MAUI. Alternative B solo por insuficiencia demostrada y otro ADR. Merging sigue fuera. [Backlog](010-b-deferred.md).

**012 independiente prevista:** group-invitations-collaboration — búsqueda/invitar/aceptar/rechazar/notificar/enlace no registrado/incorporación a listas compartidas. No implementación en010 ni bloqueo de autenticación.

Users posee correlación; ownership/GroupMembership/AddedBy/PaidBy/PurchasedBy/auditoría usan UserId interno, nunca email/Google/sub global. Perfil propio y clear prevalecen; no DOB/teléfono/dirección/foto futuros. TP11 **1.2.0 permanece intacta**. Cierre beta habilita PR para revisión; no merge, deploy, cutover ni configuración cloud nuevos. Entorno local010-A funcional se conserva, sin restauración automática de API anterior.
