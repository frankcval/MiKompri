# Specification Quality Checklist — 010

Fecha: 2026-10-30. Estos checks evalúan **artefactos de diseño**, no la feature implementada. [Spec](../spec.md), [plan](../plan.md), [aceptación futura](acceptance.md).

## Completitud documental comprobada

- [x] Historias priorizadas con pruebas independientes, escenarios Given/When/Then, edge cases y criterios medibles.
- [x] Objetivo Google sin Microsoft y email sin Microsoft explícitos; Microsoft adicional, Apple/otros investigados.
- [x] Auditoría del código actual y separación de garantías existentes vs propuestas; 006 inexistente documentada.
- [x] Research oficial vigente con fecha/fuentes y discrepancia native/social registrada; sin extrapolar Q&A/B2C a CIAM.
- [x] A/B comparadas, A preferente condicionada a evidencia; B solo si A no satisface requisitos. No tomar recurso Graph como prueba operativa.
- [x] UserId/ownership/grupos/gastos/historia inmutables; no linking por email/perfil/sub global.
- [x] Migración autorizada, coexistencia/no migrados, carreras, rollback y anti-duplicación definidos.
- [x] Doble prueba reciente, unlink último método, recovery y seguridad del broker/Graph contemplados.
- [x] Perfil/username/login/autoridad/sync/avatar diferenciados, datos futuros minimizados y eliminación diseñada.
- [x] JWT/issuer/audience/scopes/registros Web/MAUI y permisos de negocio separados.
- [x] Mapa TP11 de 13 reglas, propuesta SemVer/enmienda y ADR con estado Propuesto, sin editar constitución.
- [x] Ocho fases y tareas abiertas con rutas, tests antes de reglas y gates de QA real; doce casos obligatorios trazados.
- [x] Límites de ejecución respetados: solo documentos/selección de feature Spec Kit; sin código, Azure, credenciales, BD ni cambios de tareas anteriores.

## Decisiones y aceptación pendientes (no bloquean revisar el diseño, sí ejecutarlo)

- [ ] Ratificar G1: A funciona con Google-only→email, métodos y colisiones; o elegir B mediante ADR revisado.
- [ ] Ratificar G2/G3: email OTP vs password/SSPR/passkey y prueba de frescura/step-up/recovery reales; Microsoft personal validado.
- [ ] Aprobar G4–G7: plan/cohortes/retirada, SDK/configuración, PII/retención/coste y ADR/enmienda.
- [ ] Ejecutar todos los AC y negativos, migraciones/rollback y revisión de seguridad/privacidad.

**Dictamen documental:** `010 READY FOR ARCHITECTURE REVIEW`. No significa READY FOR IMPLEMENTATION ni habilitación de tenants/producto. No hay un vacío oculto que impida presentar alternativas: los gates abiertos tienen prueba/decisión, responsable de revisión y tareas identificadas.
