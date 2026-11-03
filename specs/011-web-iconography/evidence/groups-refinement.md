# Refinamiento Grupos — cierre011

Base remota auditada cf83bff; [HEAD/CI inicial](head-review.json). Source/lock del candidato en bundle-final.json; resultados finales se registran aparte, no se certifican por tests antiguos.

## Contrato efectivo

MiKompri.Users.Application/Dtos/GroupDto.cs ya contiene MyRole y MemberCount; GetMyGroupsQueryHandler asigna rol desde membresía y count de Memberships. GET /api/v1/groups ya los serializa. groupsApi.ts añade únicamente memberCount?:number al tipo frontend; no nuevo endpoint ni modificación HTTP/backend. Si faltan datos, GroupMetadata no inventa rol ni número. No se hace una consulta de miembros por tarjeta.

## Presentación

Cabecera y Crear grupo compactos, tarjetas papel/banda teal/icono groups, nombre/espacio/rol/personas y entrada Ver grupo con un solo enlace semántico. Empty state útil, una tarjeta intencional y grid de diez en desktop/una columna móvil. Formulario en Dialog Fluent: validación/Enter/loading/error/draft/query invalidation conservados, foco vuelve al disparador o CTA estable si desaparece el empty state tras crear. Detalle presenta nombre/contexto real, entrada a listas y Personas de Users. No se añade administración nueva.

**Group lifecycle and invitations intentionally out of scope for 011.** Sin Eliminar, Abandonar, Transferir, Invitar, roles editables, placeholders disabled ni acciones ficticias.012 sigue independiente; ciclo de vida/eliminación se definirá en iniciativa futura cuando exista contrato/reglas aprobadas, sin implementarlo aquí.

## Evidencia visual

[Manifest de capturas](groups-screenshots.json): antes cf83bff/Chromium y candidato/tres motores, con fixtures sintéticos consistentes (3personas en resumen y detalle). No datos personales ni imágenes privadas.

- [Móvil antes](groups-head-chromium-groups-mobile.png) / [móvil después](groups-final-chromium-groups-mobile.png).
- [Desktop antes](groups-head-chromium-groups-desktop.png) / [desktop después](groups-final-chromium-groups-desktop.png).
- [Detalle móvil](groups-final-webkit-group-detail-mobile.png), [vacío](groups-final-firefox-groups-empty.png).

Revisión: papel/tinta/teal/radios/espaciado de011, diferencia de notas de compra por banda/icono/contexto colaborativo, información contextual visible en vez de hyperlinks flotantes. Nombre y CTA envuelven, controles44, sin overflow en22capturas; no se afirma igualdad de píxel entre motores.

## Comprobaciones

Tres tests nuevos de componente fallaron antes de implementar y luego3/3 PASS. Pasada dirigida de Grupos/iconografía en los tres motores:54/54 PASS; incluye vacío/uno/diez, nombre largo, rol/count, detalle, creación/error/Enter/restauración, cancelación del Drawer mientras carga y toque emulado. Suites finales del SHA publicable se registran en validación final, no se infieren de esta pasada.
