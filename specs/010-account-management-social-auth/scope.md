# 010 — Account Management & Social Auth

Formalización Spec Kit al 30/10/2026, sin implementación ni configuración Azure/BD. [Spec](spec.md), [plan](plan.md), [tasks](tasks.md), [research actual](research.md) y [checklist](checklists/requirements.md). Esta página reemplaza el alcance preliminar como índice; las decisiones siguen propuestas para architecture review.

Objetivo: Google y email sin cuenta Microsoft, Microsoft adicional, External ID CIAM y UserId Guid interno inmutable. Preferir A (agregación de métodos en CIAM); B (varios bindings CIAM por UserId) solo por insuficiencia demostrada de A y ADR revisado. La antigua formulación «varias identidades externas» no impone B por anticipación. Nunca linking por email/perfil, nunca cambiar ownership o crear UserId nuevo en una migración autorizada.

Perfil propio y personalizaciones prevalecen; [modelo](data-model.md), [migración](migration.md), [amenazas](threat-model.md), [privacidad/eliminación](privacy-and-deletion.md), [contratos](contracts/account-and-identity.md), [ADR](../../docs/adr/ADR-004-ciam-identity.md) y [enmienda TP11 propuesta](constitution-amendment-proposal.md) detallan los límites. Nacimiento/teléfono/dirección se diseñan como futuro sin recogida inicial. Web primero; MAUI compatible y diferido.

Corrección de investigación: native authentication actualmente puede integrar social mediante un tramo browser-delegated; no afirmar exclusión total ni paridad de SDKs MAUI. Fuentes, contradicciones oficiales y gates de prueba están registrados en research. Las [doce aceptaciones](checklists/acceptance.md) siguen pendientes; ningún artefacto equivale a una prueba real ni a aprobación de constitución.
