# Specification Quality Checklist — 010-A

Fecha2026-10-30. Revisión de **documentación**, no cumplimiento funcional. [Spec](../spec.md), [plan](../plan.md), [aceptación](acceptance.md), [informe](../evidence/010-a-scope-review.md).

## Comprobaciones documentales

- [ ] Alcance obligatorio Microsoft existente+Google sin Microsoft, sin G3/linking como dependencia.
- [ ] Email OTP elegible, QA previo y deshabilitado hasta aprobación; no bloquea núcleo.
- [ ] IDs T001–T055 conservados, futuras tareas visibles, T056–058 añadidas y ninguna tarea de producto cerrada.
- [ ] Historias, 26FR, ochoSC y diez A-AC obligatorios trazados; onceavo opcional y doce AC previos reconciliados.
- [ ] UserId canónico, permisos/ownership íntegros, no correlación por email/perfil/sub global.
- [ ] Transición Microsoft obligatoria diseñada con origen inmutable, reserva, cohortes, bloqueo de duplicados, no migrados y rollback; coexistencia no equivale a migración aceptada.
- [ ] Alternativa A simple preferida; no colección de identidades/AccountOperation/ProofReceipt para login básico. B requiere insuficiencia demostrada y nueva revisión.
- [ ] D1 autoritativo antes de negocio productivo; no exige completar D1 para primer login aislado. Sin Graph por request/Redis/bus/CAE.
- [ ] Web MSAL.js/Code+PKCE browser-delegated; MAUI/Native Auth no se fijan ni bloquean A.
- [ ] Perfil básico, autoridad propia y minimización; iniciales, no foto/storage/DOB/teléfono/dirección.
- [ ] Recuperación original del proveedor, sin identidad sustituta desde soporte/email.
- [ ] ADR propuesto y propuesta TP11 1.2.0; constitución vigente intacta.
- [ ] Evidencia G3 preservada, sin convertir c1 en MFA ni resultados INCONCLUSIVE en PASS.
- [ ] Enlaces/Spec Kit/formato y archivos protegidos verificados según informe; esta lista requiere revisión humana antes de marcarla.

## Decisión pendiente indispensable para iniciar identidad

- [ ] Ratificar ADR-004 y propuesta TP11, incluyendo contrato de confianza/coexistencia y transición Microsoft acotada (T007/T008/T009/T010). Aplicación constitucional posterior autorizada conforme a gobernanza.

Las tareas de integración/QA/migración/D1/perfil son trabajo de implementación/entrega del MVP, no evidencia de que la documentación impida comenzar después de esa ratificación. G1/G3/linking, email, fotos, MAUI y recuperación entre proveedores no bloquean iniciar ni entregar el núcleo Microsoft + Google.

**Dictamen de inicio:** ver informe de alcance. Esta checklist no marca ninguna aceptación productiva, tarea previa ni aprobación arquitectónica.
