# Quickstart011 — revisar la implementación

## Contexto de diseño y Spec Kit

1. Confirmar rama011 y base main9597af5/PR59 integrado; leer constitution, spec, design, icon-map, ADR005 y acceptance.
2. Abrir design/mobile-lists.png, mobile-shopping.png y desktop-lists.png. Son referencias estáticas con datos ficticios, no pantallas instaladas ni prueba de accesibilidad.
3. Revisar manifest icon-audit.json (31 exports,5.7.0) y contrast.json (11 pares). Ese manifest registra la auditoría previa a instalar; bundle-after.json acredita la dependencia instalada y sus imports.
4. Spec Kit desde raíz:

```powershell
./.specify/scripts/powershell/check-prerequisites.ps1 -Json -RequireTasks -IncludeTasks
./.specify/scripts/powershell/check-prerequisites.ps1 -Json -PathsOnly
git diff --check
```

Debe resolver specs/011-web-iconography, mantener documentos/IDs y dejar intactas tareas de otras specs.

## Ejecutar y validar el candidato

Seguir tasks.md y las evidencias de implementación/validación. Usar npm ci con el lockfile, versión exacta auditada y licencia conservada. Tests funcionales primero para registry/acciones/foco/tooltip. Probar un corte de navegación/listas antes de adaptar otras pantallas. Capturar baseline y después con los mismos datos, fuente, viewport y motor.

Desde MiKompri.Web:

```powershell
npm ci
npm run dev:010a
npm run test:unit
npm run test:component
npm run test:integration
npm run lint
npm run typecheck
npm run build
$env:PLAYWRIGHT_ALL_BROWSERS='1'
npm run test:e2e:smoke -- --workers=2 --reporter=line,html
```

dev:010a reutiliza el archivo QA privado existente y APIs28082/28080/28083; comprobar salud si se hacen recorridos reales. No modificar/restaurar.env.local, cloud o BD para cambiar diseño. No publicar tokens/storageState ni usar una cuenta ajena. Las pruebas controladas no sustituyen login real ni auditoría humana.

Validar320/390/430/768/1280, teclado, tooltip hover/foco/Escape, coarse pointer, safe-area/teclado virtual, lector/zoom, forced-colors y reduced-motion. Ejercitar estados vacío/error/parcial/carga,100ítems y nombres/precios largos/desconocidos. Comparar navegación/historial/compra/Gastos/perfil y logoutGoogle↔Microsoft con comportamientos previos.

Aplicar protocolo de métricas008/009:20muestras por ruta, LCPp75<=2500ms, interacción<=200ms, CLS<=0.1; documentar baseline/post y chunks/gzip sin ampliar budgets/retries. No cerrar tareas anteriores al pasar011. Tres motores de CI no acreditan todos los navegadores/dispositivos: indicar exactamente los entornos usados.

Cierre requerido: CI Web del SHA final, iconos/imports/licencias, tablas de aceptación y capturas reales. Backend/EF no cambian; si aparecen cambios fuera de scope, detener esa ampliación y documentar causa. Reversión solo visual; no merge/deploy automático.
