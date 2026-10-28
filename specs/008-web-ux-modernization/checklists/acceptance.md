# Acceptance gates P0–P3

Estado: aceptación parcial a 2026-10-28. [Evidencia](../evidence/validation.md), [rendimiento](../evidence/performance.md) y [piloto pendiente](../evidence/pilot.md). Una casilla cerrada no cierra las verificaciones de otros gates; autorización API con identidad controlada y Entra real están separadas.

## P0 — Recursos y autorización

- [x] A001 Baseline y prerequisitos reproducibles; deuda 007 reconciliada sin perder historial.
- [x] A002 Matriz rutas/DTO/roles/scopes confirmada y contratos nuevos implementados/OpenAPI local comprobado.
- [x] A003 Sin sesión, propietarios/grupos cruzados, ítems ajenos y miembros retirados no leen/escriben; roles permitidos sí. Evidencia API con identidad/Users controlados, también PostgreSQL real; A020 mantiene Entra pendiente.
- [x] A004 Endpoints genéricos protegidos; T132 con evidencia independiente de T133.
- [x] A005 Tests dominio/aplicación/API afectados y Docker satisfactorios. Build completo de API con SDK oficial 8.0-noble mediante argumento y build web; persistencia PostgreSQL verificada. Detalles del fallo de caché local y recuperación en evidencia.

## P1 — Recorridos completos

- [ ] A006 Personal CRUD, confirmación, recarga y segunda lista intacta.
- [x] A007 Compra/descompra persistida e idempotente, pending por producto, sin cambios financieros. API verifica también body obligatorio, ItemId y balances.
- [ ] A008 Grupos con miembros/roles/listas y autoría; compartida opera según matriz, historia financiera preservada.
- [ ] A009 Crear/incorporar sin salir, duplicados/timeout/error parcial recuperables y desconocido distinto de cero legado.
- [ ] A010 Destino seguro tras acceso, creación desde portada, borrador por cuenta/lista, TTL/logout/sin replay.
- [ ] A011 Estados/HTTP/red en cada pantalla con siguiente acción, sin datos perdidos ni detalles internos.
- [ ] A012 Perfil releer/sincronizar diferenciados y errores recuperables.
- [ ] A013 Gastos registrar/corregir/eliminar según rol, participantes revisados, retirados históricos y cálculo backend.

## P2 — Experiencia coherente

- [ ] A014 Prototipo revisado móvil primero y navegación elegida por evidencia; enlaces antiguos/contexto URL conservados.
- [x] A015 Mis listas sin falso vacío ante fallo parcial; grupos/productos/cuenta accesibles.
- [x] A016 Demo/compras comparten lenguaje/componentes y solo capacidades entregadas, sin escritura real desde ejemplo.
- [ ] A017 Foco/fila estables, controles legibles y no tapados, nombres largos y todos los estados.
- [x] A018 Tokens y textos consistentes; WOFF2/lazy loading medidos con licencia conservada. Presupuesto completo continúa en A022.

## P3 — Evidencia y cierre

- [x] A019 CI controlada con selección smoke no vacía, tres motores, informes/trazas y Docker satisfactorios. [Job verificado](https://github.com/frankcval/MiKompri/actions/runs/40758498760/job/123063736625). La aceptación Entra es independiente y continúa en A020.
- [ ] A020 Harness e integración Entra/multiusuario separados; tests obligatorios ejecutados con persistencia/roles reales.

- [ ] A021 Teclado/zoom/lector/contraste/táctil/teclado virtual/breakpoints y tres motores verificados, cero bloqueantes.
- [ ] A022 SC-008 con perfil/muestras/percentiles y baseline publicados, sin atribuir datos de laboratorio a campo.
- [ ] A023 Piloto ≥10 y SC-003/004/005/007 medidos con denominadores/ayudas/errores.
- [x] A024 Defectos corregidos y verificación pertinente repetida, sin relajar criterio para aprobar. Retorno de foco, cuerpo booleano obligatorio y fixtures demorados; los veinte recorridos agregados de estado se separaron por pantalla conservando aserciones.
- [ ] A025 FR/SC trazables, deuda 007 cerrada individualmente solo cuando corresponde y límites históricos conservados.
- [ ] A026 Documentación describe capacidades reales; fuera de alcance sin simulaciones; constitución y gates verificados, sin despliegue implícito.

La aceptación Entra se ejecuta explícitamente en `acceptance-mikompri-web-entra.yml` una vez configurados los recursos, o con la suite integrada local. Un CI de push verde no acredita A020; sigue pendiente una ejecución real satisfactoria sobre el commit de entrega.
