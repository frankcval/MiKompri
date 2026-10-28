# Integración en MiKompri.Web

El objetivo es conservar esta dirección visual usando la estructura existente de React, TypeScript y Fluent UI v9. El prototipo es una referencia visual y de interacción; las integraciones reales deben apoyarse en los servicios, contratos y reglas ya definidos en MiKompri.

## Orden de trabajo propuesto

1. Revisar las instrucciones del repositorio y los documentos de `specs/007-web-react-client/`. Verificar cómo encaja la portada en la navegación y actualizar las tareas correspondientes.
2. Crear la portada mediante componentes pequeños y estilos acotados. Conservar los textos y la jerarquía del prototipo.
3. Reutilizar el tema existente, Source Sans 3 y los tokens de marca con primario `#006B68`.
4. Definir las acciones de crear lista, entrar y acceder a listas según los flujos reales de la aplicación. Mantener las demos identificadas como ejemplos si se conservan.
5. Verificar navegación, accesibilidad y responsive en el navegador, y ejecutar los controles existentes del proyecto.

## Componentes orientativos

Estos nombres son sugerencias, no rutas ya existentes en tu repositorio.

| Sección | Componente sugerido |
| --- | --- |
| Página | HomePage |
| Cabecera y menú móvil | HomeHeader |
| Presentación y llamadas a la acción | HomeHero |
| Lista interactiva de ejemplo | ShoppingListPreview |
| Explicación en tres pasos | HowItWorks |
| Escenarios de uso | SharingScenarios |
| Preguntas frecuentes | HomeFaq |
| Cierre y pie | HomeFooter |

## Equivalencias de interfaz

| Prototipo | Fluent UI v9 |
| --- | --- |
| Botones | Button |
| Campos y validación | Field, Input |
| Ventanas y acceso | Dialog |
| Productos marcados | Checkbox |
| Progreso | ProgressBar |
| Participantes | Avatar, AvatarGroup |
| Pasos y vistas | TabList, Tab |
| Avisos | Toast / Toaster |
| Preguntas | Accordion |

## Estado e integración

- Sustituir las consultas al DOM, `innerHTML` y los listeners globales de `app.js` por estado y eventos de React.
- Llevar los ejemplos a fixtures separados de los datos reales. Los nombres de las listas introducidos por el usuario deben renderizarse como texto normal de React.
- Acotar el CSS a los componentes de portada. No trasladar sin revisión las reglas globales de `body`, `button`, `h1` o `dialog`.
- Reutilizar el `FluentProvider` y el tema existentes en el lugar que corresponda a la arquitectura actual.
- Mantener la jerarquía de las páginas protegidas y el menú compartido. Revisar por separado el comportamiento de la portada para visitantes y usuarios autenticados.
- Conectar las acciones a los flujos reales de autenticación y listas, sin inventar endpoints ni modificar audiencias o permisos.
- Tratar el selector flotante «Vista» y la demo de reparto como herramientas del prototipo. Decidir expresamente si se conservan en la versión real.

## Criterios de aceptación

- El propósito de MiKompri y la acción principal se entienden desde el primer bloque.
- La portada funciona a 320, 390, 430, 768 y 1280 px sin desbordamiento horizontal.
- Las acciones táctiles ofrecen áreas de al menos 44 × 44 px.
- El menú móvil abre, cierra y permite llegar a las secciones.
- Las pestañas, diálogos y formularios funcionan con teclado y muestran el foco.
- Se mantienen los contrastes, el movimiento reducido y el orden de lectura.
- Crear una lista y entrar llevan al flujo real acordado; no se confunden con las simulaciones.
- La navegación autenticada sigue visible y protegida.
- Pasan los controles existentes de TypeScript y calidad. Añadir pruebas para cambios de comportamiento que lo justifiquen.

La validación del prototipo no sustituye la validación de esta integración en React.
