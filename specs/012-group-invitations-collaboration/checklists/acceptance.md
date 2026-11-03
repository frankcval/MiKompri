# Checklist de aceptación — 012 Group Invitations Collaboration

## Escenarios clave

- [ ] **AC-01** Owner invita por `@username` válido y se crea `Pending`.
- [ ] **AC-02** Admin invita por `@username` válido con rol permitido.
- [ ] **AC-03** Invitación a usuario inexistente devuelve error controlado.
- [ ] **AC-04** Self-invite se rechaza con error de negocio.
- [ ] **AC-05** Invitar miembro existente se rechaza.
- [ ] **AC-06** Duplicado `Pending` se rechaza (`409` o equivalente).
- [ ] **AC-07** Invitado lista y ve su invitación in-app.
- [ ] **AC-08** Invitado acepta y se crea membresía.
- [ ] **AC-09** Reintento de aceptar no duplica membresía (idempotencia).
- [ ] **AC-10** Invitado rechaza y estado pasa a `Declined`.
- [ ] **AC-11** Owner/Admin revoca `Pending` y target no puede aceptarla luego.
- [ ] **AC-12** Invitación expirada no puede aceptarse.
- [ ] **AC-13** Operaciones concurrentes no dejan estados inválidos.
- [ ] **AC-14** Tras aceptar, usuario ve listas compartidas del grupo según 013.

## No funcional

- [ ] Trazabilidad mínima (created/responded timestamps + actor implícito).
- [ ] Respuestas sin exposición de IDs internos de identidad.
- [ ] Consistencia de errores con middleware/API conventions existentes.
