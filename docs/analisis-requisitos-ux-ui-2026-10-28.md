# Requisitos, implementación web y plan de evolución UX/UI

Fecha: 28 de octubre de 2026.

> Documento histórico del estado inspeccionado ese día. Las implementaciones posteriores de 008 y 009, integradas en main, resuelven parte de los gaps aquí descritos; no leer esta matriz como diagnóstico vigente. Consultar el [estado actual](../README.md#-estado-actual-del-proyecto), [tareas 008](../specs/008-web-ux-modernization/tasks.md) y [revisión 009](../specs/009-shopping-experience-completion/evidence/review-corrections.md). El historial no acredita por sí solo el cierre de aceptación pendiente.

## Alcance y evidencia

Revisión del estado local de las especificaciones 002–005 y 007, README, diseño vigente, tareas, componentes, clientes HTTP y pruebas E2E. Inspección visual de las capturas existentes `home-1280.png` y `app-private-shopping-390.png`, generadas el día anterior. Las capturas no constituyen una nueva comprobación en navegador.

No se han ejecutado nuevas pruebas funcionales, mediciones con usuarios, instalaciones o despliegues. Las cifras de pruebas citadas corresponden a evidencia histórica registrada en `specs/007-web-react-client/tasks.md`. La presencia de código no certifica funcionamiento integrado, seguridad o satisfacción de usuarios.

## Dictamen

La web implementa una parte significativa del MVP, pero no satisface todavía todos sus requisitos ni demuestra sus criterios de éxito. Portada, tema, selección por URL, creación de listas, incorporación de productos y consulta de reparto tienen una base implementada. Faltan operaciones de gestión cotidiana, membresías visibles y validación integrada. La promesa de compra colaborativa es más amplia que la experiencia disponible.

No se asigna un porcentaje global: requisitos de seguridad, operaciones CRUD y validación humana no son unidades equivalentes y no deben promediarse como si lo fueran.

## Matriz de cobertura

Los estados describen evidencia local, no una certificación productiva.

| Referencia 007 | Expectativa | Implementación observada | Evaluación |
| --- | --- | --- | --- |
| FR-001–004 | SPA React/TypeScript/Vite/Fluent y MSAL | Stack e integración presentes | Base implementada; autenticación real pendiente de verificación integrada |
| FR-007 | Acceso, salida y restauración | MSAL, guard y salida presentes; E2E de autenticación omitido | Implementado parcialmente verificado |
| FR-008 | Mostrar y refrescar perfil | Consulta y presentación; no hay acción explícita de sincronización con el proveedor | Parcial; aclarar si refrescar significa releer o sincronizar claims |
| FR-009 | Crear/listar grupos y ver membresías | GroupsPage crea y lista nombres; no presenta un detalle de miembros/roles | Parcial: falta una tarea exigida por la web |
| FR-010 | Crear, consultar, actualizar y eliminar listas personales e ítems | Alta, selección, incorporación y marcado; cliente sin operaciones de actualización/borrado | Incumplimiento funcional pendiente |
| FR-011 | Operar compartidas por rol y grupo | Selección nominal, creación, detalle y alta de productos; sin marcado compartido ni edición/borrado completos | Parcial; bloqueo de autorización T132 |
| FR-012 | Gastos, balances y propuesta de liquidación | Selección de producto y personas; consulta de resultados backend | Implementado en el recorrido básico; roles/multiusuario e históricos pendientes |
| FR-013 | Consulta de catálogo/mercados/historial y alta de productos | Pantalla y formulario presentes; regreso a lista permitido | Cobertura implementada en alcance; flujo fragmentado y validación integrada pendiente |
| FR-014 | Cinco destinos de navegación | MainLayout conserva los cinco destinos | Implementado; simplificar navegación requiere actualizar criterio vigente |
| FR-015/016 | Estados explícitos y errores accionables | Componentes compartidos y tratamiento desigual por feature; grupos colapsa errores en mensajes genéricos | Parcial; hace falta matriz pantalla × estado × recuperación |
| FR-017/018 | Responsive y accesibilidad básica | CSS adaptativo, foco, diálogos y evidencias controladas | Evidencia parcial; contraste, zoom, lector y entorno integrado pendientes |
| FR-032 | Portada pública y destino personal tras login | App y LoginPage conducen al índice personal | Implementado según requisito actual |
| FR-033 | Sistema visual compartido | Tema, Source Sans 3 y CSS Modules comunes | Base implementada; no equivale a cierre de accesibilidad |
| FR-034 | Índice/detalle, alta contextual, Compra/Gastos y URL | Estructura y pestañas implementadas | Cobertura acotada; las limitaciones expresas no cierran FR-010/011 |
| FR-023 / SC-007 | Pruebas de flujos críticos | Pruebas controladas; varios E2E integrados explícitamente omitidos | No acreditado por completo |

Fuentes: `MiKompri.Web/src/features/`, `src/auth/guards/ProtectedRoute.tsx`, `src/app/layout/MainLayout.tsx`, `src/app/router/routes.tsx`, `tests/e2e/` y `specs/007-web-react-client/`.

## Distinciones de alcance

- El README describe capacidades del conjunto de la plataforma; no certifica su disponibilidad en la web.
- Las specs 002–005 son propietarias de reglas y capacidades backend. Que una operación exista en API no demuestra que la persona pueda completarla en UI.
- Editar/desactivar productos y administrar mercados/precios están expresamente fuera del alcance web actual. Su ausencia no es un incumplimiento de FR-013.
- Agregar una membresía mediante UserId no equivale a disponer de invitaciones. No proponer una interfaz de invitación sin contrato público correspondiente.
- Desmarcar, pagos reales, offline completo y notificaciones no son entregables pendientes del MVP actual. Desmarcar puede proponerse como nueva capacidad con contrato y criterios propios.
- El destino personal tras login cumple FR-032. Recuperar un enlace protegido o abrir directamente la creación es una mejora propuesta, no un fallo contra el texto actual.
- FR-034 mantiene bloqueado el marcado compartido hasta T132. Cumplir esa restricción es correcto, pero el recorrido colaborativo global sigue incompleto frente a 005 FR-004 y la promesa de portada.

## Dependencia prioritaria: autorización del marcado

El handler genérico `MarkItemAsPurchasedCommandHandler` carga la lista por ID y la modifica sin comprobar propietario, membresía o rol en ese handler. El cliente personal solo lo llama para elementos procedentes de `/mine`, pero esa selección no constituye autorización del servidor. T132 sigue abierto y debe cubrir tanto el endurecimiento personal como el uso compartido. Esto es un hallazgo de código, no una prueba de explotación remota.

T133 documenta una corrección distinta, ItemId frente a ProductId. No debe reabrirse como si siguiera sin corregir ni utilizarse para dar por resuelta la autorización.

Antes de habilitar nuevas escrituras: matriz operación/ruta/actor/recurso/permiso, protección en servidor y pruebas negativas sin sesión, entre propietarios y entre grupos, incluyendo miembros inactivos y roles aplicables. Revisar también los endpoints genéricos que se pretendan reutilizar para editar o borrar.

## Expectativas de uso aún no demostradas

La última evidencia de UX privada registrada informa 96 pruebas Vitest satisfactorias y 16 Playwright satisfactorias con 7 omitidas. Son resultados históricos; no se han reproducido en esta revisión. El harness controla sesión y red y no sustituye Entra/PKCE ni escrituras multiusuario reales.

SC-001 exige que al menos el 90% de usuarios piloto acceda y llegue al panel en menos de dos minutos. SC-002 exige que al menos el 85% complete sin asistencia crear lista compartida, registrar gasto y consultar reparto. No hay en lo revisado una medición que permita cerrar esos resultados. SC-003/004/006/007 requieren evidencia adicional de estados, mensajes, accesibilidad y E2E, no solo componentes existentes.

## Diagnóstico UX/UI

La identidad visual ya conecta portada e interior: verde petróleo, superficies claras y Source Sans 3. El siguiente salto consiste en mejorar jerarquía e interacción.

En las capturas revisadas, la demostración de portada ofrece casillas compactas, cantidades secundarias y progreso. El detalle móvil real dedica una proporción importante a cabecera, regreso, panel, confirmación y resumen antes de los productos. Cada fila conserva un botón de marcado con texto repetido. Esta diferencia hace que la demostración parezca más directa que el producto disponible.

Cambios propuestos:

1. Usar un componente de lista común para demostración y producto, con adaptadores de datos y capacidades diferentes; la demostración seguirá aislada de escrituras reales y no mostrará funciones no entregadas.
2. Diseñar el detalle móvil alrededor de los productos, con cabecera breve y progreso compacto. Mantener contexto, foco y espacio suficiente para los controles.
3. Priorizar Mis listas; conservar acceso a personales, compartidas, grupos, catálogo y cuenta. Prototipar una navegación móvil más directa antes de cambiar rutas o criterios.
4. Resolver añadir con búsqueda y opción Crear y añadir en contexto. Si crear producto funciona y añadir falla, conservar el producto creado y permitir reintentar únicamente la incorporación.
5. Mostrar membresías y roles por nombre, abrir las listas del grupo desde su detalle y ofrecer acciones según contratos y permisos reales.
6. Introducir edición de cantidad/precio y borrado confirmado con recuperación coherente. Desmarcado queda como ampliación explícita, no como simulación local.
7. Hacer legibles los gastos: separar estimación, importe pagado y saldo; expresar quién debe a quién, con detalles contables secundarios. Los cálculos permanecen en backend.
8. Proponer pagador actual y participantes habituales solo cuando sea válido por permisos y con revisión visible. No incluir personas silenciosamente ni confundir comprador con pagador.
9. Reducir «ítem», mensajes repetidos y detalles de configuración en pantallas de usuario. Errores concretos, próximos pasos y conservación de formularios.
10. Mantener marca y tipografía; centralizar tokens. Incorporar iconos consistentes si mejoran identificación, sin sustituir etiquetas necesarias.

## Plan de cambios y aceptación

| Orden | Entrega | Requisitos/tareas | Criterio verificable |
| --- | --- | --- | --- |
| P0 | Consolidar matriz de requisitos y resolver ambigüedades | FR-008/010/011/032/034; T111/T118 | Cada requisito tiene operación, pantalla, contrato, prueba y estado; mejoras nuevas se distinguen de deuda existente |
| P0 | Endurecer operaciones backend reutilizadas | T132; 005 FR-003/004/014 | Pruebas de identidad/propiedad/rol y rechazo entre usuarios/grupos; marcado personal y compartido autorizado |
| P1 | Completar gestión personal | FR-010; T112 | Renombrar/eliminar lista; editar/eliminar producto; cambios persisten tras recarga y no afectan a otra lista |
| P1 | Completar recorrido colaborativo | FR-009/011/012; T113/T114 | Ver miembros y roles, acceder a listas del grupo, comprar y registrar gasto según rol; no miembro recibe rechazo |
| P1 | Alta de producto dentro de lista | FR-013/034 y ampliación UX | Buscar, crear y añadir sin perder lista/cantidad; fallo intermedio no genera duplicados ni falso éxito |
| P1 | Recuperación de sesión y errores | FR-007/015/016; T115 | Borradores sobreviven a error; reautenticación vuelve al destino permitido sin repetir escrituras automáticamente |
| P2 | Prototipo y sistema de componentes | FR-014/017/018/033 | Portada, acceso, índice, detalle y gastos comparten componentes; propuesta revisada en móvil y escritorio |
| P2 | Aplicar navegación y modo compra | Cambio explícito de FR-014/032 si procede | Acción frecuente accesible; navegación por teclado, Atrás, URL y foco se conservan; sin acciones ocultas por barras fijas |
| P2 | Optimizar carga | Tareas técnicas derivadas del perfil de rendimiento | Medir bundle/carga; evaluar división por rutas y WOFF2; comparar antes/después sin degradar fuentes o navegación |
| P3 | Validación integrada y humana | SC-001–007; T116–118 | E2E obligatorios ejecutados, matriz de roles, estados, teclado, contraste, zoom y piloto documentados |

Orden de dependencias: autorización → operación real → recorrido completo → componentes/presentación → validación. El prototipo visual puede prepararse durante el cierre funcional, pero su interfaz final solo activará capacidades acreditadas.

## Recursos y herramientas

Mantener React, Fluent UI v9, React Query, React Hook Form, Zod y Playwright ya instalados. Aprovechar el tema y componentes existentes antes de incorporar otro sistema visual.

Proponer `@axe-core/playwright` para accesibilidad automatizada complementada con evaluación manual. Storybook es opcional si se va a mantener un catálogo de componentes y estados. Una familia de iconos es opcional. Un prototipo navegable local puede validar arquitectura y textos sin una dependencia de diseño nueva.

No instalar paquetes en esta fase de análisis. Verificar compatibilidad, mantenimiento y coste antes de cualquier incorporación; la revisión de recomendaciones externas del análisis previo no sustituye una comprobación al instalar.

## Primera entrega recomendada

Un recorrido vertical con dos personas de prueba: acceso → crear grupo/lista → consultar miembros → añadir producto nuevo → marcar compra → registrar gasto → interpretar reparto. Incluir corrección de cantidad y un error recuperable. Registrar persistencia, permisos y comprensión, además de capturas.

No declarar la feature completa por apariencia, número de pruebas o casillas documentales. Cerrar cada requisito con evidencia de su propia aceptación y separar pruebas controladas, integración real y resultados con usuarios.
