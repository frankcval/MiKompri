# Quickstart y protocolo de validación

## Estado y prerrequisitos

Plan e implementación local del 2026-10-28. Ver [evidencia actual](evidence/validation.md), [rendimiento](evidence/performance.md) y [piloto pendiente](evidence/pilot.md). Pasan 107 pruebas frontend y 40/33/68 pruebas de dominio/aplicación/API; estas últimas también con PostgreSQL aislado. Entra con dos cuentas, revisión manual ampliada y piloto siguen pendientes. Los resultados del 27 de octubre en 007 se conservan como históricos.

- Node/npm compatibles con lockfile/Vite, .NET SDK de la solución, Docker/Compose y navegadores Playwright.
- APIs y PostgreSQL reproducibles conforme Docker/Compose existentes. Configurar variables públicas web según `MiKompri.Web/README.md`; no introducir secretos en bundle.
- Tenant Entra de prueba, cliente web/redirects/scopes/audience existentes y cuentas con roles requeridos. Sin tenant no se cierra integración; no alterar políticas del tenant para evitar autenticación.
- Dos grupos, dos propietarios y miembros Owner/Admin/Member, no miembro y retirado; recursos aislados identificables por ejecución. No borrar datos del usuario.
- Contratos nuevos de [contracts](contracts/api-and-permissions.md) implementados/OpenAPI/testeados antes de usar controles.

## Continuar con Spec Kit

La feature activa se guarda en `.specify/feature.json`. Desde raíz, los comandos locales revisan artefactos sin instalar la CLI:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks
powershell -NoProfile -ExecutionPolicy Bypass -File .specify/scripts/powershell/setup-tasks.ps1 -Json
```

`ExecutionPolicy Bypass` se limita al proceso que ejecuta scripts locales revisados; no cambia la política del equipo. La integración actual está preparada para Copilot. En este entorno se siguen sus instrucciones/plantillas/scripts; no se afirma haber invocado comandos slash no disponibles ni instalado la CLI.

Antes de implementar, leer spec/plan/tasks/constitución y el análisis de consistencia del cierre de esta preparación. No ejecutar setup-plan para sobrescribir un plan trabajado; el script actual conserva archivos existentes. No iniciar otro feature para la misma deuda.

## Comprobaciones de implementación

Desde `MiKompri.Web`:

```powershell
npm ci
npm run lint
npm run typecheck
npm run test:unit
npm run test:component
npm run test:integration
npm run build
npx playwright install
npm run test:e2e
```

`npm test` incluye unit/component/integration locales. Playwright por defecto selecciona portada, visual y modernización con fixtures; `PLAYWRIGHT_ALL_BROWSERS=1` añade Firefox/WebKit. Instalar los tres motores. En este equipo la caché está en `E:/Projects/MiKompri/.playwright-browsers`, mediante `PLAYWRIGHT_BROWSERS_PATH`. Evitar invocaciones simultáneas que compartan directorio de trazas. `npm run test:e2e:integrated` usa configuración separada y preflight obligatorio: no convierte ausencia de credenciales en un skip satisfactorio. Variables y formato de sesión en [README web](../../MiKompri.Web/README.md). El job controlado ya pasó en GitHub; la aceptación Entra no pasó por falta de entorno y sesiones.

La CI de push/PR y la aceptación real se programan por separado. `acceptance-mikompri-web-entra.yml` se invoca manualmente cuando QA y credenciales están preparados; exige también URLs explícitas de las tres APIs. Sus prerrequisitos se comprueban antes de instalar dependencias. GitHub muestra el workflow manual una vez incorporado a la rama predeterminada; mientras tanto puede ejecutarse la suite local. Se exige resultado real satisfactorio del mismo commit antes de cerrar 008, independientemente de que la CI normal esté verde.

Desde raíz:

```powershell
docker compose config --quiet
docker compose up -d --build
dotnet test test/MiKompri.ShoppingList.Domain.Tests/MiKompri.ShoppingList.Domain.Tests.csproj --configuration Release
dotnet test test/MiKompri.ShoppingList.Application.Tests/MiKompri.ShoppingList.Application.Tests.csproj --configuration Release
dotnet test test/MiKompri.ShoppingList.Api.Tests/MiKompri.ShoppingList.Api.Tests.csproj --configuration Release
docker build -f MiKompri.Web/Dockerfile -t mikompri-web:ux-review .
git diff --check
```

Preparar entorno aislado/puertos antes de `compose up`; no resetear datos o volúmenes del usuario. La implementación inicial recreó ShoppingList local y conserva copia etiquetada de la imagen anterior. La caché SDK predeterminada falló; el build íntegro en contenedor se completó con el argumento SDK_IMAGE apuntando al SDK oficial 8.0-noble. En la revisión posterior de CI se corrigió también el rechazo de identidad incompleta de Users API: 31 pruebas de API, 32 de aplicación y 12 de dominio pasan, sin cambios de esquema. ProductCatalog no se modificó. Integración con PostgreSQL real no sustituye identidad Entra ni comunicación real Users con dos cuentas. No se desplegó ni publicó ninguna imagen en remoto.

## Recorridos obligatorios

| ID | Escenario | Esperado |
| --- | --- | --- |
| V01 | Sin sesión, owner ajeno, ítem cruzado, no miembro/inactivo, roles | Rechazo de lectura/escritura indebida, recurso intacto |
| V02 | Personal crear → añadir → corregir → marcar → desmarcar → renombrar → borrar | Persistencia/otra lista intacta, confirmación/foco |
| V03 | Grupo → personas/roles → crear lista → incorporar → compra por segunda persona | Permisos/autoría reales y lectura persistida |
| V04 | Producto nuevo y error tras creación/timeout/duplicado | Reconciliación, productId reutilizado, sin duplicados |
| V05 | Enlace protegido/CTA → Entra → retorno; expirar → draft → reauth | Destino interno permitido, cuenta aislada, envío explícito |
| V06 | Gasto → balance → corrección/borrado → consulta histórica | Cálculo backend y roles, historia retirada preservada |
| V07 | 400/401/403/404/409/503/red por pantalla aplicable | Mensaje/acción, datos conservados y sin falso éxito |
| V08 | URLs antiguas, filtros, tabs, Atrás/Adelante, recarga HTTP | Mismo contexto, no solo MemoryRouter |
| V09 | Portada y lista, demo ficticia, estados vacíos/largos | Capacidades coherentes sin API real desde ejemplo |
| V10 | Teclado/lector/zoom/virtual keyboard/safe areas | Sin bloqueos ni controles ocultos, foco y nombre |

## Matriz de acceso y presentación

Pantallas: portada, acceso/callback, perfil, grupos/personas, Mis listas, detalles personal/compartido, catálogo/historial y gastos/reparto. Estados: carga, vacío, éxito, error general y códigos aplicables según contrato. Marcar «no aplica» solo con justificación, nunca omitir silenciosamente.

Anchos: 390 primero, 320/430/768/1280 y fronteras 767/768/1023/1024. Zoom 200%, nombres largos y teclado virtual. Chromium/Firefox/WebKit para V02/V05/V08/V10; lector NVDA+Chrome y VoiceOver+Safari con registro de versión/dispositivo; si faltan equipos/personas, registrar pendiente. Medir contraste 4.5:1 texto normal, 3:1 grande/controles y objetivos de 44 px. Axe no sustituye evaluación manual.

## Rendimiento SC-008

Aplicar perfil de [plan](plan.md) a rutas portada, Mis listas y detalles personal/compartido: 20 navegaciones frías/ruta y 20 muestras por añadir/marcar/abrir gastos. Medir LCP, latencia de interacción por Event Timing/INP de sesión y CLS; no confundir tiempos de API con respuesta a interacción. Registrar percentiles por ruta/acción, muestras crudas, fixture, CPU/red, navegador y hardware antes/después en `evidence/performance.md`. No crear este archivo con valores ficticios. Comparar carga diferida, bundle y fuentes; no afirmar métricas de campo a partir de laboratorio.

## Piloto SC-003/004/005/007

Mínimo 10 personas representativas, incluyendo primera visita y uso móvil; informar experiencia previa. Mismas instrucciones/tareas, sin pistas ni ayuda durante intento medido; si se ayuda, registrar fallo de independencia y luego observación cualitativa.

1. Acceder y llegar a tarea: cronómetro desde pulsar acceso hasta destino/acción disponible; incluir tiempo de identidad. Meta ≥9/10 <2 min.
2. Crear lista compartida, añadir producto nuevo, registrar gasto y consultar reparto, sin asistencia: ≥9/10.
3. Preguntar quién debe cuánto y a quién y si se ha transferido dinero: ≥9/10 respuestas completas correctas.
4. Evaluar 20 mensajes/casos de recuperación seleccionados con anticipación de la matriz, distribuidos entre participantes y pantallas: ≥19/20 con causa/siguiente acción correctas. Publicar selección, respuestas y denominadores; no escoger solo los mejores casos.

Registrar tiempos por tarea, errores/abandono, ayudas, aciertos, observaciones de atractivo/claridad y mejoras en `evidence/pilot.md` con datos anonimizados. No recoger credenciales ni grabar sin consentimiento. Una iteración que cambie resultados requiere nueva evidencia; no eliminar la anterior.

## Registro y cierre

Cada evidencia registra revisión de código, fecha, entorno, comando/protocolo, expected/actual, passed/failed/skipped, navegador/cuenta de prueba anonimizada y limitaciones. Publicar capturas/trazas sin secretos; separar harness, integración y piloto. Completar [aceptación](checklists/acceptance.md) y [trazabilidad](traceability.md). Un prerequisito ausente o fallo obligatorio mantiene su gate abierto. No crear informes de ejecución vacíos para simular resultados.
