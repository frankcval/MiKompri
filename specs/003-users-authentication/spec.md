# Feature Specification: Users Authentication & Groups

**Feature Branch**: `003-users-authentication`

**Created**: 2026-07-12

**Status**: Draft

**Input**: User description: "Formalizar el bounded context de Users/Auth como la siguiente funcionalidad después de 002-shopping-list-core: perfiles de usuario locales, sincronización de identidad, actualización de perfil, grupos colaborativos con roles Owner/Admin/Member y autorización basada en pertenencia."

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Reconocimiento y sincronización de perfil local (Priority: P1)

Como usuario autenticado, quiero que MiKompri me reconozca como usuario local para poder ser propietario de datos dentro de la plataforma.

**Why this priority**: Sin un perfil local, ninguna operación colaborativa futura —propiedad de listas, pertenencia a grupos, trazabilidad de acciones— es posible. Es la base de toda la identidad dentro del sistema.

**Independent Test**: Se valida enviando una solicitud autenticada al endpoint de sincronización de perfil. El sistema crea el perfil local si no existe y devuelve los datos del perfil resultante. Una segunda llamada con el mismo token no genera duplicados.

**Acceptance Scenarios**:

1. **Given** que un usuario válido se autentica por primera vez, **When** realiza una solicitud al endpoint de sincronización de perfil, **Then** el sistema crea un perfil local asociado a su identidad y devuelve los datos del perfil creado.
2. **Given** que un usuario ya tiene perfil local, **When** vuelve a sincronizar su perfil, **Then** el sistema actualiza los datos que hayan cambiado en los claims de identidad y devuelve el perfil actualizado sin duplicar el registro.
3. **Given** que una solicitud llega sin token de autenticación válido, **When** el sistema intenta procesarla, **Then** la solicitud es rechazada con un error de acceso no autorizado.

---

### User Story 2 - Consulta de perfil del usuario autenticado (Priority: P1)

Como usuario autenticado, quiero consultar mi perfil para que la aplicación pueda mostrar la información de mi cuenta.

**Why this priority**: Es el acceso de lectura fundamental al perfil propio, necesario para cualquier flujo de interfaz de usuario y para verificar que la sincronización funcionó correctamente.

**Independent Test**: Se valida creando un perfil mediante sincronización y consultando el endpoint de perfil propio. La respuesta contiene el nombre visible, email y timestamps de trazabilidad.

**Acceptance Scenarios**:

1. **Given** que el usuario autenticado tiene perfil local, **When** consulta su propio perfil, **Then** recibe nombre visible, email y marcas de creación y última modificación.
2. **Given** que el usuario autenticado no tiene perfil local, **When** consulta su perfil, **Then** el sistema devuelve un error claro indicando que el perfil aún no ha sido sincronizado.
3. **Given** que la solicitud no incluye autenticación válida, **When** se intenta consultar el perfil, **Then** la solicitud es rechazada con error de acceso no autorizado.

---

### User Story 3 - Actualización de perfil del usuario autenticado (Priority: P2)

Como usuario autenticado, quiero actualizar mi nombre visible para que mi perfil sea legible para futuros colaboradores.

**Why this priority**: Permite al usuario personalizar cómo aparece ante otros miembros de sus grupos. Depende de que exista un perfil local (P1 primero).

**Independent Test**: Se valida sincronizando un perfil y luego enviando una solicitud de actualización con un nuevo nombre visible. Al consultar el perfil después, el nombre actualizado es el que aparece.

**Acceptance Scenarios**:

1. **Given** que el usuario autenticado tiene perfil local, **When** envía una solicitud de actualización con un nombre visible válido, **Then** el sistema actualiza el nombre y devuelve el perfil actualizado con la nueva marca de modificación.
2. **Given** que el usuario autenticado envía un nombre visible vacío o solo con espacios, **When** intenta actualizar su perfil, **Then** el sistema rechaza la solicitud con un error de validación claro.
3. **Given** que la solicitud no incluye autenticación válida, **When** se intenta actualizar el perfil, **Then** la solicitud es rechazada con error de acceso no autorizado.

---

### User Story 4 - Creación de grupo colaborativo (Priority: P1)

Como usuario autenticado, quiero crear un grupo para poder compartir listas de compra con mi familia o mi hogar en el futuro.

**Why this priority**: La creación de grupos es el punto de entrada a la colaboración. Sin grupos no hay pertenencia ni roles, que son la base de autorización de esta feature.

**Independent Test**: Se valida enviando una solicitud de creación de grupo con nombre válido. El sistema crea el grupo y automáticamente registra al creador como miembro con rol Owner. Al consultar los miembros del grupo recién creado, aparece únicamente el creador.

**Acceptance Scenarios**:

1. **Given** que un usuario autenticado con perfil local envía una solicitud de creación de grupo con nombre válido, **When** el sistema procesa la solicitud, **Then** se crea el grupo y se registra automáticamente al creador como miembro con rol Owner.
2. **Given** que el grupo ha sido creado, **When** se consultan sus miembros, **Then** el creador aparece en la lista con rol Owner.
3. **Given** que un usuario envía una solicitud de creación de grupo con nombre vacío, **When** el sistema lo procesa, **Then** la solicitud es rechazada con un error de validación claro.
4. **Given** que la solicitud no incluye autenticación válida, **When** se intenta crear un grupo, **Then** la solicitud es rechazada con error de acceso no autorizado.

---

### User Story 5 - Gestión de miembros de un grupo (Priority: P2)

Como propietario o administrador de un grupo, quiero agregar y eliminar miembros para gestionar quién pertenece al grupo.

**Why this priority**: Depende de que existan grupos y perfiles locales (P1). Es la operación principal de administración colaborativa dentro del bounded context.

**Independent Test**: Se valida con un Owner que agrega un segundo usuario al grupo, verifica que aparece en la lista de miembros, luego lo elimina y verifica que ya no aparece. Se valida también que un Member no puede realizar estas operaciones.

**Acceptance Scenarios**:

1. **Given** que un usuario autenticado con rol Owner o Admin en un grupo envía una solicitud para agregar un nuevo miembro existente, **When** el sistema procesa la solicitud, **Then** el nuevo miembro queda registrado con el rol indicado y aparece en la lista de miembros del grupo.
2. **Given** que un usuario ya es miembro de un grupo, **When** se intenta agregarlo de nuevo al mismo grupo, **Then** el sistema rechaza la solicitud con un error claro indicando que ya es miembro.
3. **Given** que un usuario autenticado con rol Owner o Admin en un grupo elimina a un miembro existente, **When** el sistema procesa la solicitud, **Then** el miembro es retirado y ya no aparece en la lista de miembros.
4. **Given** que un usuario autenticado con rol Member en un grupo intenta agregar o eliminar miembros, **When** el sistema procesa la solicitud, **Then** la operación es rechazada con un error de acceso no autorizado.
5. **Given** que se intenta agregar como miembro a un identificador que no existe como perfil de usuario local, **When** el sistema procesa la solicitud, **Then** devuelve un error claro indicando que el usuario no existe.

---

### User Story 6 - Consulta de miembros de un grupo (Priority: P2)

Como miembro de un grupo, quiero consultar la lista de miembros del grupo para saber quién pertenece a él.

**Why this priority**: Operación de lectura necesaria para la coordinación entre miembros. Depende de que existan grupos y membresías.

**Independent Test**: Se valida con un miembro del grupo que consulta la lista de miembros y obtiene la relación completa con nombres visibles y roles.

**Acceptance Scenarios**:

1. **Given** que el usuario autenticado es miembro de un grupo, **When** consulta la lista de miembros, **Then** recibe la lista de todos los miembros con sus nombres visibles y roles.
2. **Given** que el usuario autenticado no es miembro del grupo cuya lista consulta, **When** realiza la solicitud, **Then** la solicitud es rechazada con un error de acceso no autorizado.

---

### Edge Cases

- ¿Qué ocurre si el proveedor de identidad no incluye email en los claims al sincronizar? El email se trata como campo opcional; el sistema sincroniza con los claims disponibles y registra el perfil sin email.
- ¿Puede un Owner eliminarse a sí mismo del grupo si es el único Owner? No se puede eliminar la última membresía Owner del grupo; el sistema devuelve un error claro indicando que debe haber al menos un Owner.
- ¿Qué ocurre si se intenta crear un grupo con nombre duplicado para el mismo usuario? Se permite, ya que el nombre es un dato legible, no un identificador único. El identificador del grupo es un valor generado por el sistema.
- ¿Qué sucede cuando un usuario con rol Member intenta listar miembros de un grupo al que no pertenece? La solicitud es rechazada con error de acceso no autorizado.
- ¿Puede un Admin eliminar a un Owner del grupo? No; los Admins no pueden gestionar miembros con rol Owner; solo el Owner puede hacerlo.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema DEBE rechazar todas las solicitudes a endpoints protegidos que no incluyan un token de autenticación válido emitido por el proveedor de identidad configurado.
- **FR-002**: El sistema DEBE permitir a un usuario autenticado sincronizar su perfil local a partir de los claims de identidad (sub, nombre, email). Si el perfil no existe, debe crearlo; si ya existe, debe actualizarlo con los claims actuales.
- **FR-003**: El sistema DEBE permitir a un usuario autenticado consultar su propio perfil local, incluyendo nombre visible, email y timestamps de trazabilidad.
- **FR-004**: El sistema DEBE permitir a un usuario autenticado actualizar su nombre visible en el perfil local. El nombre visible no puede estar vacío ni contener solo espacios.
- **FR-005**: El sistema DEBE permitir a un usuario autenticado con perfil local crear un grupo con un nombre no vacío.
- **FR-006**: Al crear un grupo, el sistema DEBE registrar automáticamente al creador como miembro del grupo con rol Owner en la misma operación atómica.
- **FR-007**: El sistema DEBE permitir a un usuario con rol Owner o Admin en un grupo agregar un nuevo miembro existente como perfil local, especificando el rol a asignar (Admin o Member).
- **FR-008**: El sistema DEBE impedir agregar a un usuario que ya es miembro del mismo grupo. La respuesta debe incluir un mensaje de error claro.
- **FR-009**: El sistema DEBE permitir a un usuario con rol Owner en un grupo eliminar cualquier miembro. Un Admin puede eliminar miembros con rol Member o Admin, pero no con rol Owner.
- **FR-010**: El sistema DEBE impedir la eliminación de la última membresía Owner de un grupo.
- **FR-011**: El sistema DEBE permitir a cualquier miembro de un grupo consultar la lista de miembros del grupo con sus nombres visibles y roles.
- **FR-012**: El sistema DEBE impedir que usuarios con rol Member realicen operaciones de gestión de miembros (agregar o eliminar).
- **FR-013**: El sistema DEBE devolver errores de validación con mensajes claros que identifiquen el campo y la razón del error.
- **FR-014**: Las respuestas de la API DEBEN seguir el mismo estilo de respuesta, estructura de errores y convenciones de trazabilidad ya establecidas en el bounded context ShoppingList.
- **FR-015**: El bounded context Users DEBEN incluir tests automatizados de dominio, de aplicación y de integración de API para los escenarios críticos de éxito y de error descritos en esta especificación.

### Key Entities *(include if feature involves data)*

- **UserProfile**: Representa la identidad del usuario dentro de MiKompri. Atributos clave: identificador único interno, identificador externo derivado del claim `sub` del proveedor de identidad, nombre visible, email (opcional), timestamp de creación, timestamp de última modificación.
- **Group**: Representa un grupo colaborativo. Atributos clave: identificador único, nombre legible, timestamp de creación, timestamp de última modificación.
- **GroupMembership**: Representa la pertenencia de un UserProfile a un Group. Atributos clave: referencia al UserProfile, referencia al Group, rol (Owner / Admin / Member), timestamp de incorporación. La combinación UserProfile + Group es única.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El 100 % de las solicitudes a endpoints protegidos sin token de autenticación válido son rechazadas con respuesta de acceso no autorizado.
- **SC-002**: Un usuario autenticado puede sincronizar su perfil local y consultarlo en menos de 2 segundos desde el momento en que su token es validado.
- **SC-003**: Al crear un grupo, la membresía del creador con rol Owner se registra en la misma operación; no se permite que un grupo quede sin Owner en ningún momento.
- **SC-004**: El 100 % de los intentos de duplicar la membresía de un usuario en el mismo grupo son rechazados con un error claro y descriptivo.
- **SC-005**: El 100 % de las operaciones de gestión de miembros realizadas por usuarios con rol Member son rechazadas con un error de acceso no autorizado.
- **SC-006**: Un Owner puede completar el flujo de creación de grupo, adición de un miembro y consulta de la lista de miembros en menos de 3 minutos de interacción.
- **SC-007**: Los tests automatizados cubren, como mínimo, los escenarios de éxito y de error de cada historia de usuario de prioridad P1 y P2 en las capas de dominio, aplicación e integración de API.
- **SC-008**: Las respuestas de error de validación son coherentes con el estilo del bounded context ShoppingList existente, incluyendo códigos de error claros e información de trazabilidad.

---

## Assumptions

- La autenticación es gestionada íntegramente por un proveedor de identidad externo compatible con OAuth2/OIDC (por ejemplo, Azure Entra ID o Auth0). El sistema MiKompri solo valida el token JWT emitido por ese proveedor y no emite tokens propios.
- El claim `sub` del token JWT es el identificador único del usuario en el proveedor de identidad y se usa como clave de correlación para el perfil local de MiKompri.
- El email es un campo opcional en los claims; el sistema crea el perfil local incluso si no está presente.
- No se implementará inicio de sesión con contraseña, ni se crearán flujos de registro de usuario propios, ni se emitirán refresh tokens.
- La sincronización de perfil puede realizarse mediante un endpoint explícito; no se implementa sincronización transparente automática en cada solicitud para simplificar el diseño inicial.
- Los grupos no tienen límite máximo de miembros en esta iteración.
- El nombre de un grupo no necesita ser único a nivel global; es un dato legible, no un identificador.
- La eliminación de un miembro no elimina ningún dato histórico asociado a ese miembro dentro del grupo; es una baja lógica de membresía.
- El bounded context ShoppingList no se modifica en esta iteración. La integración entre usuarios/grupos y listas de compra se realizará en una feature posterior, siguiendo el principio PP3 de la constitución.
- El bounded context Users ya existe parcialmente en la solución como proyecto scaffolded; esta feature formaliza su dominio, aplicación e infraestructura siguiendo el patrón de ShoppingList como referencia.
- No se implementará API Gateway ni se realizará despliegue productivo en Azure como parte de esta iteración.
