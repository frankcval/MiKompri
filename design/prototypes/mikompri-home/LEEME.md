# MiKompri · Prototipo de portada

Esta carpeta contiene el código del prototipo que has aprobado como punto de partida: HTML, CSS, JavaScript, iconos y fuentes locales. Es una copia de la versión 1 desplegada el 26 de octubre de 2026.

## Verlo en tu ordenador

1. Extrae el ZIP en una carpeta.
2. Abre `index.html` en el navegador para una revisión rápida.
3. Si quieres servirlo por HTTP, abre una terminal en esta carpeta y ejecuta:

```powershell
py -m http.server 5500 --bind 127.0.0.1
```

Después abre `http://localhost:5500`. Detén el servidor con `Ctrl+C`.

## Qué contiene

- `index.html`: estructura, textos e iconos de la portada.
- `styles.css`: diseño responsive, estados, tipografía y colores.
- `app.js`: interacción con las listas, formularios, pestañas, reparto y vistas responsive.
- `fonts/`: Source Sans 3 y su licencia.
- `favicon.svg`: marca de prueba del prototipo.
- `GUIA_REACT.md`: propuesta para trasladarlo al cliente React con Fluent UI.
- `PROMPT_PARA_CODEX.md`: instrucciones para usar esta referencia al trabajar en tu repositorio.
- `QA.md`: comprobaciones realizadas en el prototipo.

Los cambios de las listas duran solamente mientras la página permanece abierta. La entrada con Microsoft, las invitaciones y el reparto son simulaciones. No hay integración con las API de MiKompri.

## Usarlo en tu proyecto

Puedes reutilizar los textos, estilos, colores, iconos y recursos, y adaptar el comportamiento. La portada todavía no es un componente React ni usa el paquete de componentes Fluent UI. `GUIA_REACT.md` explica cómo convertirla manteniendo la arquitectura del cliente.

Para empezar, copia esta carpeta a `design/prototypes/mikompri-home/` dentro de tu repositorio como referencia y utiliza el prompt incluido con Codex. Es una ubicación sugerida; ajústala si tu repositorio ya tiene una carpeta para diseños.

## Acceso y alojamiento

La versión alojada está en:
https://mikompri-portada-frank.frankcval.chatgpt.site

Ese alojamiento conserva el acceso privado a tu cuenta. Esta copia es código para uso local: no contiene el control de acceso de ChatGPT. No incluye credenciales, el repositorio Git ni la configuración de despliegue del sitio.

## Origen

- Versión: 1.
- Commit de origen: `be6f96d91b34f8836e17c76f9bcaaed134d1b504`.
- Fuente: la misma versión validada y desplegada en el prototipo privado.
