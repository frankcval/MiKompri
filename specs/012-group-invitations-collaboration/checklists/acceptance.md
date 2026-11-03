# Checklist de aceptación — 012 Group Invitations Collaboration

> Estado: checklist de **criterios de aceptación definidos** para implementación de 012. Los ítems marcados indican que el criterio está especificado y trazado en la documentación.

## Escenarios clave

- [x] **AC-01** Owner invita por `@username` válido y se crea `Pending`.
- [x] **AC-02** Admin invita por `@username` válido con rol permitido.
- [x] **AC-03** Invitación a usuario inexistente devuelve error controlado.
- [x] **AC-04** Self-invite se rechaza con error de negocio.
- [x] **AC-05** Invitar miembro existente se rechaza.
- [x] **AC-06** Duplicado `Pending` se rechaza (`409` o equivalente).
- [x] **AC-07** Invitado lista y ve su invitación in-app.
- [x] **AC-08** Invitado acepta y se crea membresía.
- [x] **AC-09** Reintento de aceptar no duplica membresía (idempotencia).
- [x] **AC-10** Invitado rechaza y estado pasa a `Declined`.
- [x] **AC-11** Owner/Admin revoca `Pending` y target no puede aceptarla luego.
- [x] **AC-12** Invitación expirada no puede aceptarse.
- [x] **AC-13** Operaciones concurrentes no dejan estados inválidos.
- [x] **AC-14** Tras aceptar, usuario ve listas compartidas del grupo según 013.

## No funcional

- [x] Trazabilidad mínima (created/responded timestamps + actor implícito).
- [x] Respuestas sin exposición de IDs internos de identidad.
- [x] Consistencia de errores con middleware/API conventions existentes.
