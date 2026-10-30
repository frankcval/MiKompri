# Implementación 010-A — registro del candidato

2026-10-30; inicio sobre0559a46, rama010-account-management-social-auth.

## A0: aprobación y constitución

El titular aprueba expresamente ADR-004 para010-A, transición Microsoft acotada, enmienda TP11 1.2.0, D1 y Web MSAL/PKCE; autoriza implementación y commits. No aprueba010-B, producción, merge ni retiro definitivo legacy. Se aplica su autorización explícita; no se inventa issue/PR aprobado. El PR de la rama mantiene revisión posterior.

Proceso de .github/agents/speckit.constitution.agent.md: constitución leída, sin extensions.yml/hooks; TP11 ampliado MINOR1.1.0→1.2.0, fecha2026-10-30, sync report y templates spec/plan/tasks propagados. No existe templates/commands; prompts remiten a agentes y no fijan otro proveedor. Otros principios/gates permanecen.

**010-A READY FOR IMPLEMENTATION**. Ninguna tarea funcional se completa por esta aprobación. Este registro se ampliará con fallos esperados, implementación y resultados del candidato.

## Validación y aceptación

Pendiente de implementar/ejecutar. Harness Google/OTP previo no es aceptación de aplicación. EmailT056 permanece A-OPT deshabilitado. Toda evidencia G3 se conserva, sin retomar CA.

A0 validado: Spec Kit -Json -RequireTasks -IncludeTasks resuelve010, exit0; git diff --check exit0. T007/T008/T009/T010 cerradas por aprobación explícita y TP11/templates aplicados, no por tests de proveedor. Restantes tareas abiertas.
