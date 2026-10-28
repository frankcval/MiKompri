# Diseño de experiencia

Dirección implementada localmente, con capturas de 320/390/430/768/1280 px y comprobaciones de teclado/foco. Se reutilizan [diseño vigente](../007-web-react-client/design.md) y [frontend-design](../../.agents/skills/frontend-design/SKILL.md). La evaluación humana y la revisión manual ampliada siguen pendientes; ver [evidencia](evidence/validation.md).

## Sistema

Elemento característico: la compra, con productos/cantidades/marca de estado. Source Sans 3 local 400/600/700, cuerpo 16 px e importes tabulares alineados. Marca #006B68, texto #172B3A, secundario #526575, lienzo #FBFCF9, superficie #FFFFFF, borde #D8E1E8. Medir contraste en cada estado; la paleta no lo acredita.

Tokens Fluent/espaciado 4 px, objetivos 44 px. Superficies solo para grupos reales, menos paneles anidados y títulos repetidos. Identidad mediante contenido y lenguaje cotidiano, sin animación decorativa dispersa.

## Recorridos

```text
Portada → Crear lista → Microsoft → creación → detalle real
Enlace protegido → Microsoft → misma lista/grupo/pestaña autorizados
Mis listas → Todas / Personales / Compartidas → abrir o crear
Grupo → Personas/roles + Listas → detalle
Detalle → buscar → seleccionar o Crear y añadir → continuar
Compartida → Compra / Gastos / Personas
```

```text
Móvil                         Escritorio
MiKompri          Cuenta      MiKompri                    Cuenta
Mis listas       + Crear      Mis listas | Grupos | Productos
[Todas][Personales][Grupo]    Índice filtrado | Detalle si cabe
Nombre / grupo / progreso    Nombre              + Añadir
productos y cantidades       Compra | Gastos | Personas
[Listas][Grupos][Productos]   productos / cantidades / acciones
```

Probar barra móvil de tres destinos y cuenta arriba, safe areas y teclado. Si empeora éxito/ocluye, mantener Drawer registrando evidencia, con acceso directo a Mis listas. No forzar lateral en tablet. URLs antiguas siguen como entradas; filtros en URL.

## Compra

Cabecera compacta con nombre/contexto, menú y progreso. Añadir junto a productos: búsqueda, creación contextual, cantidad/unidad y precio opcional; desconocido explícito. Casilla accesible cuando contrato habilitado; producto prominente, cantidad secundaria, menú nominal de editar/eliminar. Conservar fila/foco al marcar; agrupación estable o filtros explícitos para pendientes/comprados. Deshacer usa contrato real, nunca solo animación local.

Errores cerca de fila/formulario, éxitos breves anunciados sin desplazar lista y pending independiente. Portada reutiliza ShoppingListView con datos ficticios y capacidades entregadas, sin consultas ni mutaciones reales.

## Gastos/personas

«Debes pagar…» / «Te deben…», moneda/destinatario; pagado/parte/saldo como detalle. Propuesta explica que no transfiere dinero. Registro desde producto conserva selección y pagador permitido al repetir; participantes requieren revisión explícita. Comprador/pagador/estado de compra son conceptos distintos. Personas muestra roles e historia; no invita sin contrato.

## Estados/calidad

Vacío conduce a crear/añadir; 401 reauth+borrador; 403 pérdida de permiso; 404 volver; 409 revisar; 503/red reintentar lectura. Sin claves de configuración como instrucciones de producto. Enlaces navegan, botones actúan; etiquetas/errores por campo, anuncios, foco visible/retorno, zoom 200%, lector, movimiento reducido y controles no tapados por teclado/barras.

Antes de construir, prototipo con datos de prueba y tareas representativas; criticar móvil/escritorio y retirar información duplicada. Se conserva identidad existente y se mejora continuidad, no una estética genérica añadida. Iconos opcionales coherentes sin sustituir etiquetas necesarias.

## Estado implementado

Ver [validación](evidence/validation.md) y [tareas con estado individual](tasks.md). Los contratos aditivos ya están implementados localmente. Se conservan los límites históricos, la separación de pruebas controladas/Entra y los gates de piloto, accesibilidad ampliada y rendimiento. La navegación móvil incluye barra inferior con safe areas, Drawer y menú de cuenta; se oculta al enfocar campos. Las acciones de gestión se compactaron para dar prioridad a productos, con nombres accesibles completos y confirmación de eliminación.
