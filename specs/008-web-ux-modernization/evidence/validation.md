# Evidencia de implementación — 2026-10-28

Revisión de origen: `f362713`; cambios locales de 008 sobre esa revisión, validación sobre el árbol de trabajo; el usuario autorizó después commit y push en `007-web-react-client`. No se publica la web en remoto. El estado de cada tarea se conserva en [tasks](../tasks.md). Esta evidencia separa implementación, pruebas controladas e integración con identidades reales.

## Resultado entregado

- Autorización en el pipeline de aplicación y en políticas por recurso: personales propias, membresía activa, roles conocidos, ítems pertenecientes a la lista y lista compartida activa para escritura. Las rutas genéricas no permiten convertir una personal en compartida ni eludir los contratos específicos.
- Compra reversible mediante asignación explícita e idempotente. Desmarcar conserva gastos y balances. Se mantienen el POST anterior y la distinción ItemId/ProductId.
- CRUD personal y gestión compartida con confirmación, permisos, autoría y restricción 409 al eliminar historia financiera. Owner/Admin corrigen gastos; Member registra su compra/pago, sin poder corregir o borrar gastos.
- Precio estimado desconocido distinto de cero mediante `IsPriceKnown`; migración aditiva con valor inicial `true`, sin reinterpretar registros existentes.
- Mis listas, grupos con personas/roles, pestañas Compra/Gastos/Personas en URL, navegación móvil y cuenta. Se conservan rutas anteriores. Portada e interior usan el mismo componente de compra; la demo permanece aislada.
- Búsqueda y alta contextual de producto; creación e incorporación como dos pasos. Un fallo de incorporación reutiliza el producto creado; una respuesta incierta exige revisión explícita antes de permitir otro intento.
- Intención de retorno interna validada y consumida una vez, borradores en sessionStorage por cuenta/contexto con caducidad de 24 h, limpieza al salir/cambiar de cuenta y ninguna repetición automática de escritura tras reautenticación.
- Historial de gastos editable según rol, importes/personas del backend y propuestas que explican que no ejecutan pagos. Perfil distingue releer de sincronizar con Microsoft.
- Fuentes WOFF2 locales con licencia original y rutas diferidas. Se mantiene Fluent UI; no se instala otro sistema visual ni un servicio externo.

## Entorno y comandos

Windows/PowerShell, Node 24.22.1, npm 11.19.1, SDK .NET 10.0.402 compilando net8.0, Docker Desktop con PostgreSQL 16. Chromium, Firefox y WebKit instalados en la caché ignorada del proyecto. Frontend React 18, React Router 7.18.5 y Vitest 5.0.5. Dos workers para evitar saturación local.

| Comprobación | Resultado comprobado | Evidencia local ignorada |
| --- | --- | --- |
| Frontend unit/component/integration | 107/107, 29 archivos, cero omitidas | `.playwright-mcp/008-final-vitest.log` |
| Compilación web | Correcta; entrada JS ~452 kB, ~124 kB gzip | `.playwright-mcp/008-final-build.log` |
| Oxlint | Correcto | `.playwright-mcp/008-final-lint.log` |
| Dominio ShoppingList | 40/40 | `.playwright-mcp/008-domain-tests.log` |
| Aplicación ShoppingList | 33/33 | `.playwright-mcp/008-application-tests.log` |
| API con EF InMemory | 68/68 | `.playwright-mcp/008-final-api.log` |
| API con PostgreSQL real aislado | 68/68, migraciones incluidas | `.playwright-mcp/008-final-postgres.log` |
| Dependencias npm | Auditoría sin vulnerabilidades tras actualizaciones acotadas | `npm audit` reproducible con el lockfile |
| Docker frontend | Imagen compilada; rutas SPA 200, harness de pruebas 404 | `.playwright-mcp/008-docker-web-final.log` |
| Docker API | Compilación completa en contenedor con SDK oficial 8.0-noble correcta; servicio local healthy/health 200 | `.playwright-mcp/008-docker-api-final.log`, `008-api-update.log` |

El primer intento PostgreSQL posterior a la compilación fallida de Docker perdió conexión con su contenedor; no se contó como aprobado. Se arrancó de nuevo exclusivamente el contenedor QA identificado por nombre/etiqueta y la repetición completa pasó. Cada factory crea y elimina su propia base con nombre aleatorio; no usa la base de datos del usuario.

El primer build del SDK Docker falló con sistema de archivos de solo lectura y después con `exec format error`. La reproducción mínima con la imagen SDK 8.0/amd64 también falló en `/bin/sh`, antes de ejecutar código del proyecto. Se comprobó inicialmente el runtime con publicación local. Tras recuperar espacio y Docker se completó **la compilación íntegra en contenedor**, usando el mismo Dockerfile con `--build-arg SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:8.0-noble`. El argumento permite escoger un SDK oficial compatible sin cambiar el runtime ASP.NET 8 ni los proyectos; el valor predeterminado sigue siendo SDK 8.0. La caché local de esa imagen predeterminada conserva un fallo del entorno. La imagen anterior está etiquetada `mikompri.shoppinglist.api:before-ux-008`; solo se recreó ShoppingList y la migración es aditiva.

## Cobertura y límites

`ResourceAuthorizationTests` incluye propietario ajeno, anónimo, Member, no miembro, miembro retirado, lista cerrada, ItemId distinto de ProductId, repetición del estado, estimación nula frente a cero, conservación de balances al desmarcar y bloqueo del borrado de historia. Los tests originales de roles y scopes continúan en la suite API. La autenticación y el contrato Users se sustituyen por dobles en WebApplicationFactory: PostgreSQL real acredita persistencia/migraciones, **no Entra ni la integración entre servicios con dos personas**.

Playwright controlado cubre 320/390/430/768/1280 px, compra/descompra con foco estable, edición, alta contextual, grupos/personas, corrección/borrado de gastos, rutas/estados y axe en tres motores. Los resultados finales y defectos se añaden al [checklist](../checklists/acceptance.md). Los fallos iniciales de retorno de foco se corrigieron en el producto; la carrera de respuestas demoradas se corrigió en el fixture. Dos invocaciones simultáneas compartieron un directorio de trazas y produjeron ENOENT; esos intentos no se cuentan como evidencia satisfactoria.

Axe mantiene todas las reglas. Solo se excluyen del resultado exigido los nodos `<i data-tabster-dummy>` de la regla aria-hidden-focus, falsos positivos de los centinelas de Fluent/Tabster documentados en [Microsoft Tabster #288](https://github.com/microsoft/tabster/issues/288). Se adjunta el JSON íntegro sin filtrar. No se usa esta excepción para controles o contenido. Las comprobaciones de teclado/foco siguen siendo obligatorias. No acredita por sí sola lector de pantalla, zoom, teclado virtual o dispositivo físico.

La URL facilitada fue `http://localhost:5173/lists`. La herramienta de navegador conectado falló al inicializar su kernel con `os error 3`; no se pudo heredar la sesión abierta. No se recibieron archivos de sesión de dos cuentas ni tokens de prueba. El preflight de integración real falla expresamente al faltar esos recursos. Los siete placeholders antiguos se sustituyeron por escenarios ejecutables en una configuración separada; no llevan `test.skip`. El acceso interactivo inicial con Entra/PKCE y las sesiones exportadas siguen siendo verificaciones distintas.

## Requisitos frente a lo construido

| Requisitos | Resultado observable | Expectativa pendiente |
| --- | --- | --- |
| FR-001 | Spec Kit existente, tareas y trazabilidad 007→008 | Cierre global solo con evidencia restante |
| FR-002/003/027 | Políticas de identidad, recurso, rol y membresía en aplicación; genéricos y dedicados | Ejecución Entra/servicios con dos cuentas |
| FR-004/005 | CRUD personal y gestión compartida; validación, confirmación y restricciones financieras | Matriz completa de fallos y recorrido real tras recarga |
| FR-006/007 | Compra reversible idempotente, pending por ítem y balances intactos | Recuperación completa ante toda respuesta incierta en entorno real |
| FR-008/009 | Grupo, personas, roles y autoría; texto de miembro anterior si falta el nombre | Nombres históricos completos y comprensión de casos con varios antiguos miembros |
| FR-010/011 | Buscar/crear/incorporar dentro de la lista, precio desconocido y saga recuperable | Matriz completa de duplicado/timeout y E2E compartida real |
| FR-012/013 | Retorno interno permitido, TTL, aislamiento, limpieza y sin replay | Callback, cambio de cuenta y recuperación interactiva con Entra |
| FR-014/015 | Mensajes normalizados, reintento y perfil releer/sincronizar diferenciados | Todos los códigos por cada operación y evaluación humana de mensajes |
| FR-016/017/018 | Formulario RHF/Zod, personas permitidas, historial corregible por rol y balances backend; no pagos | Escenarios reales de miembros retirados y comprensión del reparto |
| FR-019/020/021/022 | Mis listas, rutas antiguas, filtros/pestañas URL, demo compartida, navegación móvil y foco | Validación de comportamiento en dispositivo/teclado virtual real |
| FR-023 | Axe y recorridos de teclado/foco en tres motores y cinco tamaños | Lector, zoom 200 % y revisión manual ampliada |
| FR-024 | WOFF2, carga diferida y medición comparable | Cumplimiento completo del presupuesto SC-008 |
| FR-025/026 | Suites separadas y preflight real obligatorio, protocolo de piloto | Dos cuentas reales, CI ejecutada, Docker SDK sano y diez participantes |
| FR-028 | Se mantiene el alcance: sin invitaciones/pagos/OCR/push/offline completo | Ninguna capacidad excluida se simula |

## Pendientes de aceptación

- Ejecutar la configuración integrada con dos cuentas Entra de prueba y sesiones exportadas tras login real, en un entorno de datos desechables. No publicar sesiones ni tokens en Git, logs o trazas.
- Ejecutar los workflows en GitHub; el build íntegro local de API y web ya pasa. La configuración CI escrita localmente no equivale a una ejecución remota ni resuelve los prerrequisitos Entra del job integrado.
- Completar lectura de pantalla, zoom 200 %, teclado virtual, safe areas físicas y matriz ampliada de error/recuperación.
- Acreditar los umbrales de rendimiento con las muestras de [performance](performance.md); una reducción del bundle no basta.
- Ejecutar el [piloto](pilot.md) con al menos diez personas. No se han reclutado personas ni calculado satisfacción con pruebas técnicas.

No se declara cerrada 008 ni los gates completos de 007 por entregar código.

Revisión de CI tras el primer push: GitHub rechazó el workflow web por usar `runner.temp` en `jobs.env`, donde ese contexto no está disponible. Se sustituyó por rutas temporales únicas con `github.run_id`. Los dos workflows modificados pasan actionlint 1.7.12 (binario oficial y SHA-256 verificado; instalado solo en carpeta QA ignorada). [Ejecución que detectó el error](https://github.com/frankcval/MiKompri/actions/runs/40757986924). El siguiente push vuelve a activar las comprobaciones; esto no elimina los prerrequisitos Entra del job integrado.

## Revisión final antes de commit/push

107 pruebas frontend, 40 de dominio, 33 de aplicación y 68 de API satisfactorias; las 68 también con PostgreSQL 16. Tras optimizar se repitieron 27 casos de modernización/axe en tres motores y 12 casos a 390 px. Las 87 pruebas distintas del inventario controlado se cubren con selecciones complementarias; los intentos fallidos se conservan como tales. Build/tipos y lint correctos; auditoría npm cero vulnerabilidades. El navegador de pruebas sin sesión redirige la URL real `/lists` a `/login`; no acredita dos identidades Entra.

La revisión añadió cobertura y corrección para conservar el precio al editar solo cantidad; `price: null` explícito lo deja desconocido y `price: 0` continúa conocido. El cuerpo de purchase-state exige `isPurchased`. La creación genérica con groupId comprueba identidad/propiedad/membresía antes de persistir. El borrado de lista identifica correctamente el tipo de recurso en auditoría.

Las variantes web derivadas se llaman MiKompri Sans, conservan copyright/OFL y se generan mediante `scripts/build-fonts.py`; la cobertura latina suma ~76 kB y otros alfabetos usan los archivos completos. La licencia y aviso también se distribuyen en `public/fonts/`.

La compilación agotó C: y bloqueó Docker. Se conservó la caché npm trasladándola a `E:/Projects/MiKompri/.playwright-mcp/008-npm-cache-backup`, sin cambiar configuración global ni borrar paquetes. Se reinició Docker Desktop y su distribución Docker de WSL; después se arrancaron los contenedores originales de Users/ProductCatalog/ShoppingList/PostgreSQL. Solo se recreó ShoppingList para aplicar la implementación; los demás conservaron sus imágenes/configuración. No se eliminaron volúmenes del usuario. Los contenedores QA propios se eliminaron tras verificar su etiqueta. Los servicios originales quedaron saludables.

SC-008 permanece abierto: portada 1.876 ms, índices/detalles 2.512/2.584/2.692 ms, marcado 216 ms y añadir 344 ms. Se publican las tres series sin rebajar umbrales. Entra de dos cuentas, accesibilidad manual ampliada, piloto y matriz completa de recuperación conservan sus tareas pendientes. No hay cierre global de 007/008.
