# Prompt para trabajar en el repositorio

Adapta la ruta de referencia si has colocado esta carpeta en otro sitio. Después puedes copiar este texto en Codex abierto en tu repositorio MiKompri:

---

Implementa la portada de MiKompri en el cliente React tomando como referencia el prototipo situado en `design/prototypes/mikompri-home/`.

Primero lee las instrucciones del repositorio, la constitución del proyecto y los documentos vigentes de `specs/007-web-react-client/`. Examina el tema Fluent UI, el router, la autenticación y los componentes existentes antes de decidir la implementación. Actualiza el plan y las tareas si esta portada introduce trabajo que no está contemplado.

Conserva la dirección visual del prototipo: enfoque mobile-first, primario #006B68, Source Sans 3, superficies claras, jerarquía de contenidos, espaciado con base de 4 px y objetivos táctiles de al menos 44 × 44 px. Usa React, TypeScript y los componentes de Fluent UI v9 ya disponibles. Divide la portada en componentes y acota los estilos; no pegues `index.html` ni traslades los listeners globales de `app.js`.

Reutiliza los flujos reales para entrar, crear una lista y navegar. Verifica cómo debe comportarse la portada para visitantes y usuarios autenticados según la especificación existente. Conserva la protección de rutas y la jerarquía del menú compartido. No cambies la configuración de Microsoft Entra, los permisos ni los contratos del backend para encajar la demo.

Los datos de ejemplo deben quedar separados de los datos reales. El selector «Vista» es una herramienta de revisión del prototipo y no tiene que formar parte de la interfaz final. No presentes las simulaciones de autenticación, invitaciones o pagos como funcionalidades reales.

Valida la navegación, el menú móvil, teclado, estados de foco y formularios. Comprueba anchos de 320, 390, 430, 768 y 1280 px y ejecuta los controles existentes del proyecto. Registra la evidencia y cualquier límite de verificación en las tareas. No despliegues ni cambies la visibilidad de ningún sitio.

---
