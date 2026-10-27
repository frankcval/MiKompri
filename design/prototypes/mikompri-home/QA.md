# Validación del prototipo

Validado el 26 de octubre de 2026 en Chrome mediante el navegador de pruebas de Sites.

## Comprobaciones completadas

- Escritorio: portada completa visible y sin desbordamiento horizontal.
- Móvil: marco de 390 px, documento y viewport con el mismo ancho útil; menú abre y se cierra tras elegir un ancla.
- Tablet: marco de 768 px, documento y viewport con el mismo ancho útil; disposición de dos columnas.
- Marcar Pan integral en la portada cambia el progreso de 2/5 a 3/5.
- Abrir la demo conserva el estado marcado de la lista de ejemplo.
- Añadir Arroz integral muestra un nuevo producto y actualiza el contador a 3/6.
- Un formulario de lista vacío muestra «Escribe un nombre para tu lista».
- Crear «Compra de prueba» abre la lista vacía con total de 0,00 €.
- El ejemplo de 24,80 € se reparte en 8,27 €, 8,27 € y 8,26 €.
- La simulación de compra saldada cambia el texto y el estado de los participantes.
- La invitación abre el panel y permite seleccionar un texto ficticio, sin conceder acceso.
- La entrada simulada abre Compra de la semana mediante teclado.
- Seleccionar el escenario de piso abre Compra del piso.
- Los pasos de explicación responden al clic y a las flechas del teclado.
- El acordeón de preguntas se abre con Enter.
- Los controles del selector permiten abrir y cerrar las vistas móvil/tablet.
- Auditoría estática: IDs únicos, anclas internas existentes, recursos y fuentes locales presentes.
- `node --check dist/app.js` pasa.

## Alcance y límites

La comprobación responsive se realizó con marcos de navegador del ancho indicado; no es una prueba en dispositivos físicos. Se observaron errores intermitentes de transporte en el navegador automatizado; se retomaron los recorridos y se verificaron sus estados resultantes. Los mensajes de error observados de una extensión de Chrome no pertenecen al código de la página.

No se ha probado una sesión real de Microsoft ni una integración de API: esos recorridos se simulan expresamente. La demo no crea usuarios, envía invitaciones ni ejecuta pagos.
