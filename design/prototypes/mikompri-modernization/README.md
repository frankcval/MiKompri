# Composición 008

Propuesta móvil 390 px: encabezado compacto, nombre/contexto, progreso y filas con casilla estable; acciones secundarias de edición. Entrada con Todas/Personales/Compartidas y creación, sin un panel de estadísticas.

```text
MiKompri.                      Cuenta
Mis listas                     Crear lista
Todas | Personales | Compartidas
Compra de la semana
Casa · compra compartida
Listas           Grupos          Productos
```

Detalle: regreso → nombre/contexto → Añadir → Compra/Gastos/Personas → progreso → productos/cantidades. Barra móvil de tres destinos, cuenta arriba y Drawer alternativo; safe-area/padding inferior y ocultar barra durante foco en formulario. Escritorio lateral sin forzar dos columnas pequeñas.

Source Sans 3, #006B68/#172B3A/#526575/#FBFCF9/#FFFFFF/#D8E1E8. Revisión previa: conservar identidad, retirar paneles anidados y compartir filas reales con demo. Validación navegable sobre componentes reales/harness, sin segundo producto ni auth ficticia productiva. Esto no sustituye piloto humano.

## Revisar la composición navegable

El prototipo se valida sobre los componentes de la propia aplicación, con fixtures en Playwright. Desde `MiKompri.Web`: `npx playwright test tests/e2e/modernization/shopping.spec.ts --ui`. El [harness](../../../MiKompri.Web/tests/visual/main.tsx) y las [fixtures](../../../MiKompri.Web/tests/e2e/fixtures/modernization.ts) están separados de producción. La web real continúa en `/lists` y exige autenticación válida.

Refinamiento tras medir 50 productos: Más opciones despliega edición/eliminación al necesitarse, conservando compra directa y foco. Gastos se carga al abrir su pestaña. Las variantes web derivadas se llaman MiKompri Sans, con el diseño original y su licencia conservados; texto latino ligero y carga de otros alfabetos por cobertura.
