# Checklist de aceptación — 013 Shared Lists UX Completion

> Esta checklist se completa durante implementación/validación de 013. En esta entrega de formalización queda como objetivo de aceptación.

## A. Navegación y contexto

- [ ] AC01: Group → Shared Lists → Shared List y Lists → Shared List convergen al mismo recurso (`/shared-lists/:listId`) sin inconsistencias.
- [ ] AC02: Deep-link/reload/back-forward conserva recurso + tab + contexto.
- [ ] AC03: Contexto visible en detalle: nombre lista, grupo, estado, progreso y estimación.

## B. Listado compartido

- [ ] AC04: Tarjetas muestran nombre, grupo, pendientes, estimación y estado histórico cuando aplique.
- [ ] AC05: Estados vacío/una/múltiples/multi-grupo/error parcial/loading verificados.
- [ ] AC06: Nombres largos y distintos tamaños de pantalla no rompen layout.

## C. Creación compartida

- [ ] AC07: Crear lista no exige `GroupId` manual y refleja flujo grupo→nombre→creación.
- [ ] AC08: Escenarios sin grupos / grupo único / múltiples grupos validados.
- [ ] AC09: Errores de permiso o validación muestran feedback útil sin perder contexto.

## D. Detalle Compra

- [ ] AC10: Añadir/editar/eliminar/marcar/desmarcar funciona por permisos Owner/Admin/Member.
- [ ] AC11: Lista cerrada bloquea escrituras y mantiene consulta.
- [ ] AC12: 100 ítems, unknown price y nombres largos mantienen usabilidad.

## E. Detalle Gastos

- [ ] AC13: Distinción explícita entre estimación de productos y gasto real registrado.
- [ ] AC14: Estados vacío/registros/balance/propuesta/error/permisos verificados.
- [ ] AC15: Registro/corrección/eliminación de gasto respeta permisos backend reales.

## F. Detalle Personas

- [ ] AC16: Presentación clara de miembros y rol (Owner/Admin/Member).
- [ ] AC17: Sin funciones falsas de invitación o administración de membresía.

## G. Accesibilidad y responsive

- [ ] AC18: Teclado, foco visible, Escape, restauración de foco en diálogos y semántica de tabs correctos.
- [ ] AC19: 320/390/430/768/1280 validados en listing + detail + tabs.
- [ ] AC20: Targets 44x44, reflow, forced-colors y reduced-motion sin regresión.

## H. Rendimiento y regresión

- [ ] AC21: No se introducen queries duplicadas/waterfalls evitables en flows compartidos.
- [ ] AC22: Deuda LCP previa se mantiene separada; no se reporta como resuelta por 013.

## I. Scope guard

- [ ] AC23: No se introducen capacidades de 012 dentro de 013.
- [ ] AC24: 013 asume como precondición la membresía ya resuelta por 012 y no duplica invitaciones (invitar/aceptar/rechazar/revocar).
- [ ] AC25: No se altera estado histórico de specs 005/007/008/009/010/011.
