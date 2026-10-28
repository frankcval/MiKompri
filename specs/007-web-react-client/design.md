# Diseño web vigente — MiKompri

**Evolución planificada — 2026-10-28**: [008/design](../008-web-ux-modernization/design.md) propone modernización P0–P3. Este documento sigue siendo referencia de lo entregado; no se considera sustituido hasta implementación y evidencia. Las diferencias de navegación, intención de acceso y desmarcado se trazan en [008/traceability](../008-web-ux-modernization/traceability.md).

**Revisión**: 2026-10-27. **Alcance**: FR-014/017/018/032/033 del [spec](spec.md). Esta referencia sustituye las medidas y decisiones visuales de la propuesta del 2026-10-24; no elimina sus requisitos funcionales pendientes.

## Dirección y sistema visual

La compra es el contenido principal: nombres legibles, cantidades, progreso real y contexto de grupo. La portada de `design/prototypes/mikompri-home/` orienta la apariencia; su HTML, listeners, simulaciones y selector «Vista» no son una implementación funcional.

| Elemento | Decisión vigente |
| --- | --- |
| Marca y acción principal | `#006B68`, escala BrandVariants de Fluent UI v9 |
| Lienzo / superficie | `#FBFCF9` / `#FFFFFF` |
| Texto / secundario | `#172B3A` / `#526575` |
| Borde / selección suave | `#D8E1E8` / `#E7F1EB` |
| Fuente | Source Sans 3 local, pesos 400/600/700, fallback Segoe UI/sans-serif |
| Formato de fuente entregado | TTF con licencia SIL OFL y `font-display: swap`; WOFF2 es una optimización pendiente |
| Tipografía interior | Cuerpo y controles 16 px; subtítulos 20/24 px; h1 de 32 a 40 px, detalle de lista de 28 a 40 px |
| Portada | h1 de 40 a 64 px; conserva su propia composición |
| Espaciado | Base de 4 px: 4/8/12/16/20/24/32/40/48; gaps interiores de 16/24 px |
| Superficies | Paneles de 12 px de radio; selección/resúmenes de 8 px; controles de 4 px; sin sombra en paneles interiores |
| Interacción | Objetivo táctil mínimo 44 × 44 px, foco visible; navegación seleccionada con texto, indicador y `aria-current` |

Usar componentes y tokens Fluent antes de crear equivalentes. `theme.ts` define roles comunes; `AppSurface.module.css` y `PageHeader.tsx` reúnen presentación interior. `HomePage.module.css` y `MainLayout.module.css` acotan sus layouts. Los estilos compartidos no cambian reglas de negocio, autorizaciones ni estados de deshabilitado. El contraste de todas las combinaciones de controles y estados sigue siendo un gate pendiente: no se acredita con capturas ni con la paleta.

## Rutas y composición

- `/`: portada para visitantes; durante bootstrap se espera a MSAL; una sesión autenticada conduce a `/personal-lists`. Entrar lleva a `/login`; crear lista usa `/personal-lists`, protegida, y el formulario existente tras login. `/auth/callback` conserva el flujo MSAL.
- Las rutas de negocio siguen bajo `ProtectedRoute → MainLayout → Outlet`. Un `main`, cabecera por pantalla, salto al contenido y cinco destinos en el orden compartido actual: Perfil, Grupos, Listas personales, Listas compartidas, Catálogo. Cierre de sesión desde Perfil.
- Menú compacto mediante Drawer por debajo de 1024 px, también en tablet. Escape cierra y devuelve foco al disparador. Desde 1024 px: lateral de 224 px y contenido flexible; shell máximo de 1440 px. No hay barra inferior fija.
- Márgenes interiores: 16 px en móvil, 32 px desde 768 px y 40 px desde 1024 px. El contenido se adapta sin desbordamiento; no se fuerza una composición en dos columnas si no cabe.

## Aplicación por sección

| Sección | Entregado | Pendiente que no debe confundirse con estilo |
| --- | --- | --- |
| Portada | Hero, ejemplo local marcado como ficticio, pasos, escenarios, FAQ y cierre | No ofrece autenticación, invitaciones ni pagos simulados |
| Acceso y perfil | Superficie de acceso, cabecera, datos de perfil y estados con formato común | Validación integrada Entra/PKCE y recuperación completa ante 401 |
| Grupos | Creación y listado con jerarquía y superficies comunes | Membresías completas e invitación solo si existe contrato público |
| Listas personales | Índice sin formularios permanentes, creación breve, selección en URL, incorporación en diálogo, filas/progreso y marcado personal | CRUD completo, autor disponible y composición lateral opcional prevista en T109; hardening servidor del marcado en T132 |
| Compartidas | Selector por nombre, creación breve, detalle `/shared-lists/:listId`, alta de ítems y pestañas Compra/Gastos en query | Marcado compartido bloqueado por T132; edición/borrado de ítems y matriz completa de roles |
| Catálogo e historial | Alta de producto, selector de mercado por nombre, fechas e importes legibles | Edición/desactivación y gestión de mercados/precios fuera del alcance actual |
| Reparto | Formulario con miembros por nombre, balances y propuestas backend, sin superficies anidadas redundantes | Flujo real multiusuario, antiguos miembros, otras divisas y trabajo RHF pendiente |
| Estados | Componentes compartidos y estilo de avisos | Recuperación y mensajes específicos de cada HTTP en todas las vistas, T115 |

El ejemplo de portada y el harness `tests/visual/` están separados de las APIs reales. El progreso de la lista personal es una proyección de ítems ya recibidos, no una regla financiera. Los balances y transferencias se formatean sin recalcular el reparto. El catálogo admite alta de productos desde la ampliación del 2026-10-25; mercados, precios y productos existentes siguen en consulta.

## Verificación y cierre

Evidencia inicial en [tasks.md](tasks.md), T120–T126: 81 pruebas Vitest y 11 E2E correctos/7 omitidos. La ampliación de UX privada se registra separadamente en T128–T132 y en [quickstart.md](quickstart.md), con sus resultados y límites; no confundir ambas ejecuciones. El harness controla sesión/red solo en pruebas y no se incluye en el bundle productivo.

Esta evidencia cubre Chrome, teclado básico, foco, navegación y estados controlados. Siguen pendientes zoom 200%, teclado virtual, lector de pantalla, contraste completo, fronteras de breakpoints, matriz de permisos y los E2E integrados. No cierra SC-001/002/006/007 ni la spec completa y no autoriza despliegues.

## Recorrido privado por lista — 2026-10-27 (FR-034)

La propuesta mantiene Source Sans 3, #006B68, lienzo #FBFCF9, superficie #FFFFFF, texto #172B3A/#526575 y borde #D8E1E8; base 4 px, objetivos 44 px. El protagonismo pasa del formulario a la compra: índice con resúmenes y una acción Crear lista; creación breve en Dialog RHF+Zod; al guardar se abre el ID devuelto. Detalle con regreso al índice, nombre/contexto, progreso y acción Añadir productos en diálogo. En móvil el detalle ocupa la página; la URL conserva selección. No se elige la primera lista implícitamente.

```text
Índice:  Listas personales / compartidas       Crear lista
         [Nombre · abrir lista] [Nombre · abrir lista]
Detalle: Volver a listas
         Nombre de la lista           Añadir productos
         Grupo / Personal · progreso
         Compra | Gastos (solo compartida, pestaña en URL)
         Pendientes / Comprados       Cantidad · precio estimado
```

El patrón de pasos/tabs viene del prototipo, pero no se trasladan invitación, pagos ni login ficticios. La lista personal permite marcar como comprado mediante POST existente, solo sobre una lista autorizada por /mine y un ítem suyo; no hay desmarcado ni actualización optimista falsa. Se mantiene el orden DOM y el foco tras marcar. Errores preservan datos, bloquean duplicados y anuncian el resultado. Los formularios visibles de alta usan los productos reales; catálogo vacío ofrece crear producto y volver al detalle (personal o compartido). Los gastos se consultan y registran dentro de la misma lista compartida; tab y groupId quedan en URL y funcionan con recarga/historial.

**Contratos y límites**: GET/POST shared-lists e items verifican membresía en sus handlers. El endpoint genérico mark-as-purchased no comprueba propietario/membresía/rol en su handler: no se habilita para compartidas, se explica su indisponibilidad y se registra el prerrequisito backend, sin cambiar permisos. La protección de selección personal en UI no sustituye autorización servidor; sigue pendiente hardening del endpoint genérico. No se amplía a edición/borrado ni se declara cerrada la matriz de roles. Settlement utiliza los endpoints/participantes existentes sin recalcular importes. Comprobar lectura, carga/vacío/error/401/403, formularios y URLs, teclado/retorno de foco, cinco anchos y regresión.
