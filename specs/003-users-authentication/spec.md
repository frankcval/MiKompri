# Feature Specification: Users Authentication & Groups

**Feature Branch**: `003-users-authentication`

**Created**: 2026-07-12

**Status**: Draft

**Input**: User description: "Formalizar el bounded context de Users/Auth como la siguiente funcionalidad después de 002-shopping-list-core: perfiles de usuario locales, sincronización de identidad, actualización de perfil, grupos colaborativos con roles Owner/Admin/Member y autorización basada en pertenencia."

---

## Clarifications

### Session 2026-07-12

- Q: ¿Cuál es la estrategia de activación de la sincronización del perfil local? → A: Híbrido — el perfil se crea automáticamente en el primer request autenticado (auto-provisioning en middleware) y existe un endpoint explícito de refresco para actualizar claims cuando el usuario haya cambiado datos en el IdP.
- Q: ¿Qué roles puede asignar un Admin al agregar miembros? → A: Admin solo puede asignar rol Member; la asignación del rol Admin (y la eliminación de Admins u Owners) es exclusiva del Owner.
- Q: ¿Cuál es el claim JWT mínimo obligatorio para el auto-provisioning del perfil? → A: Solo `sub` es obligatorio; nombre y email son siempre opcionales. Si el IdP no los incluye, el perfil se crea igualmente y el usuario puede completar su nombre visible después.
- Q: ¿Los grupos pueden eliminarse en esta iteración? → A: No — la eliminación de grupos queda explícitamente fuera de alcance. Se diseñará en una feature posterior cuando el vínculo entre grupos y listas de compra esté definido.
- Q: ¿Qué artefacto del bounded context Users actuará como referencia canónica cuando ShoppingList implemente listas compartidas? → A: `GroupId` (UUID) — una futura lista compartida se vinculará a un grupo, no a un usuario individual. `GroupId` es la referencia cruzada canónica entre Users y ShoppingList.

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Reconocimiento y sincronización de perfil local (Priority: P1)

Como usuario autenticado, quiero que MiKompri me reconozca como usuario local para poder ser propietario de datos dentro de la plataforma.

**Why this priority**: Sin un perfil local, ninguna operación colaborativa futura —propiedad de listas, pertenencia a grupos, trazabilidad de acciones— es posible. Es la base de toda la identidad dentro del sistema.

**Independent Test**: Se valida enviando cualquier solicitud autenticada a la API (p.ej., consultar el propio perfil). El sistema auto-provisiona el perfil antes de responder. Una segunda llamada con el mismo token no crea un perfil duplicado. Se valida también el endpoint explícito de refresco actualizando los claims e inspeccionando que el perfil refleja los datos del nuevo token.

**Acceptance Scenarios**:

1. **Given** que un usuario válido se autentica por primera vez, **When** realiza cualquier solicitud autenticada a un endpoint protegido, **Then** el sistema auto-provisiona un perfil local usando los claims del token antes de procesar la solicitud, sin requerir una llamada explícita previa.
2. **Given** que un usuario ya tiene perfil local, **When** realiza una nueva solicitud autenticada, **Then** el sistema procesa la solicitud normalmente sin alterar ni duplicar el perfil existente.
3. **Given** que un usuario ha actualizado sus datos en el proveedor de identidad, **When** llama al endpoint explícito de refresco de perfil con su token actualizado, **Then** el sistema actualiza el nombre visible y el email del perfil local con los claims del nuevo token y devuelve el perfil actualizado.
4. **Given** que una solicitud llega sin token de autenticación válido, **When** el sistema intenta procesarla, **Then** la solicitud es rechazada con un error de acceso no autorizado.

---

### User Story 2 - Consulta de perfil del usuario autenticado (Priority: P1)

Como usuario autenticado, quiero consultar mi perfil para que la aplicación pueda mostrar la información de mi cuenta.

**Why this priority**: Es el acceso de lectura fundamental al perfil propio, necesario para cualquier flujo de interfaz de usuario y para verificar que la sincronización funcionó correctamente.

**Independent Test**: Se valida creando un perfil mediante sincronización y consultando el endpoint de perfil propio. La respuesta contiene el nombre visible, email y timestamps de trazabilidad.

**Acceptance Scenarios**:

1. **Given** que el usuario autenticado tiene perfil local, **When** consulta su propio perfil, **Then** recibe nombre visible, email y marcas de creación y última modificación.
2. **Given** que el usuario autenticado accede a su perfil por primera vez (sin perfil local previo), **When** realiza la solicitud con token válido, **Then** el sistema auto-provisiona el perfil y devuelve los datos del perfil recién creado.
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

**Why this priority**: La creación de grupos es el punto de entrada a la colaboración. Sin grupos no hay pertenencia ni roles, que son la base de autorización de esta feature. El `GroupId` generado al crear cada grupo actuará como referencia cruzada canónica con el futuro bounded context de ShoppingList colaborativo.

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

1. **Given** que un usuario con rol Owner en un grupo agrega un nuevo miembro especificando rol Admin o Member, **When** el sistema procesa la solicitud, **Then** el miembro queda registrado con el rol indicado y aparece en la lista de miembros del grupo.
2. **Given** que un usuario con rol Admin en un grupo agrega un nuevo miembro especificando rol Member, **When** el sistema procesa la solicitud, **Then** el miembro queda registrado con rol Member y aparece en la lista del grupo.
3. **Given** que un usuario con rol Admin en un grupo intenta agregar un miembro especificando rol Admin, **When** el sistema procesa la solicitud, **Then** la operación es rechazada con error de autorización indicando que solo el Owner puede asignar rol Admin.
4. **Given** que un usuario ya es miembro de un grupo, **When** se intenta agregarlo de nuevo al mismo grupo, **Then** el sistema rechaza la solicitud con un error claro indicando que ya es miembro.
5. **Given** que un usuario con rol Owner en un grupo elimina a un miembro existente (cualquier rol), **When** el sistema procesa la solicitud, **Then** el miembro es retirado y ya no aparece en la lista de miembros.
6. **Given** que un usuario con rol Admin en un grupo elimina a un miembro con rol Member, **When** el sistema procesa la solicitud, **Then** el miembro es retirado y ya no aparece en la lista de miembros.
7. **Given** que un usuario con rol Admin en un grupo intenta eliminar a un miembro con rol Admin u Owner, **When** el sistema procesa la solicitud, **Then** la operación es rechazada con error de autorización.
8. **Given** que un usuario con rol Member en un grupo intenta agregar o eliminar miembros, **When** el sistema procesa la solicitud, **Then** la operación es rechazada con error de acceso no autorizado.
9. **Given** que se intenta agregar como miembro a un identificador que no existe como perfil local, **When** el sistema procesa la solicitud, **Then** devuelve un error claro indicando que el usuario no existe.

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

- ¿Qué ocurre si el proveedor de identidad no incluye `name` o `email` en los claims? Ambos son opcionales; el único claim obligatorio es `sub`. El sistema crea el perfil con los claims disponibles y deja vacíos los campos ausentes. El usuario puede actualizar su nombre visible en cualquier momento mediante la operación de actualización de perfil.
- ¿Puede un Owner eliminarse a sí mismo del grupo si es el único Owner? No se puede eliminar la última membresía Owner del grupo; el sistema devuelve un error claro indicando que debe haber al menos un Owner.
- ¿Qué ocurre si se intenta crear un grupo con nombre duplicado para el mismo usuario? Se permite, ya que el nombre es un dato legible, no un identificador único. El identificador del grupo es un valor generado por el sistema.
- ¿Qué sucede cuando un usuario con rol Member intenta listar miembros de un grupo al que no pertenece? La solicitud es rechazada con error de acceso no autorizado.
- ¿Puede un Admin eliminar a un Owner o a otro Admin del grupo? No; Admin solo puede eliminar miembros con rol Member. Los intentos de eliminar a un Admin u Owner son rechazados con error de autorización.
- ¿Puede un Admin asignar el rol Admin a un nuevo miembro? No; la asignación del rol Admin es exclusiva del Owner. El sistema rechaza con error de autorización cualquier intento de Admin de asignar un rol superior a Member.
- ¿Puede un usuario eliminar un grupo? No; la eliminación de grupos no está soportada en esta iteración. El sistema no expone ningún endpoint para esa operación; el diseño de esta funcionalidad queda diferido a una feature posterior.

---

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema DEBE rechazar todas las solicitudes a endpoints protegidos que no incluyan un token de autenticación válido emitido por el proveedor de identidad configurado.
- **FR-002**: El sistema DEBE auto-aprovisionar el perfil local del usuario autenticado en el primer request a cualquier endpoint protegido: si no existe un perfil local vinculado al claim `sub` del token, el sistema lo crea automáticamente. El claim `sub` es el único requisito obligatorio; los claims `name` y `email` se mapean al perfil si están presentes en el token, y se dejan vacíos si no lo están. Las solicitudes siguientes del mismo usuario no vuelven a crear ni actualizar el perfil automáticamente.
- **FR-003**: El sistema DEBE permitir a un usuario autenticado consultar su propio perfil local, incluyendo nombre visible, email y timestamps de trazabilidad.
- **FR-004**: El sistema DEBE permitir a un usuario autenticado actualizar su nombre visible en el perfil local. El nombre visible no puede estar vacío ni contener solo espacios.
- **FR-005**: El sistema DEBE permitir a un usuario autenticado con perfil local crear un grupo con un nombre no vacío.
- **FR-006**: Al crear un grupo, el sistema DEBE registrar automáticamente al creador como miembro del grupo con rol Owner en la misma operación atómica.
- **FR-007**: El sistema DEBE permitir a un usuario con rol Owner agregar nuevos miembros especificando rol Admin o Member. Un usuario con rol Admin solo puede agregar miembros con rol Member; la asignación del rol Admin es exclusiva del Owner. Los intentos de un Admin de asignar rol Admin deben ser rechazados con error de autorización.
- **FR-008**: El sistema DEBE impedir agregar a un usuario que ya es miembro del mismo grupo. La respuesta debe incluir un mensaje de error claro.
- **FR-009**: El sistema DEBE permitir a un usuario con rol Owner eliminar cualquier miembro del grupo. Un usuario con rol Admin solo puede eliminar miembros con rol Member; los intentos de un Admin de eliminar a un miembro con rol Admin u Owner deben ser rechazados con error de autorización.
- **FR-010**: El sistema DEBE impedir la eliminación de la última membresía Owner de un grupo.
- **FR-011**: El sistema DEBE permitir a cualquier miembro de un grupo consultar la lista de miembros del grupo con sus nombres visibles y roles.
- **FR-012**: El sistema DEBE impedir que usuarios con rol Member realicen operaciones de gestión de miembros (agregar o eliminar).
- **FR-013**: El sistema DEBE devolver errores de validación con mensajes claros que identifiquen el campo y la razón del error.
- **FR-014**: Las respuestas de la API DEBEN seguir el mismo estilo de respuesta, estructura de errores y convenciones de trazabilidad ya establecidas en el bounded context ShoppingList.
- **FR-015**: El bounded context Users DEBEN incluir tests automatizados de dominio, de aplicación y de integración de API para los escenarios críticos de éxito y de error descritos en esta especificación.
- **FR-016**: El sistema DEBE exponer un endpoint explícito de refresco de perfil que actualice los datos del perfil local (nombre visible, email) con los claims del token en uso, permitiendo al usuario sincronizar manualmente cuando sus datos hayan cambiado en el proveedor de identidad.

### Key Entities *(include if feature involves data)*

- **UserProfile**: Representa la identidad del usuario dentro de MiKompri. Atributos clave: identificador único interno, identificador externo derivado del claim `sub` del proveedor de identidad (el único obligatorio), nombre visible (opcional en creación, mapeado desde claim `name` si está presente), email (opcional, mapeado desde claim `email` si está presente), timestamp de creación, timestamp de última modificación.
- **Group**: Representa un grupo colaborativo. Atributos clave: identificador único (`GroupId`, UUID generado por el sistema), nombre legible, timestamp de creación, timestamp de última modificación. `GroupId` es el identificador que actuará como referencia cruzada canónica cuando ShoppingList implemente listas compartidas: una lista compartida se asociará a un grupo, no a un usuario.
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
- Solo el claim `sub` es obligatorio para el auto-provisioning y el refresco de perfil. Los claims `name` y `email` son opcionales; si están presentes en el token se mapean al perfil, y si están ausentes los campos correspondientes quedan vacíos. El usuario puede actualizar su nombre visible en cualquier momento mediante la operación de actualización de perfil (FR-004).
- No se implementará inicio de sesión con contraseña, ni se crearán flujos de registro de usuario propios, ni se emitirán refresh tokens.
- El sistema implementa una estrategia híbrida de aprovisionamiento de perfil: el perfil se crea automáticamente (auto-provisioning) la primera vez que el usuario realiza cualquier request autenticado, a través de un mecanismo de middleware ejecutado antes de la lógica del endpoint; y existe adicionalmente un endpoint explícito de refresco para que el usuario actualice su perfil cuando sus datos hayan cambiado en el proveedor de identidad. No se sincroniza en cada request para evitar escrituras innecesarias.
- Los grupos no tienen límite máximo de miembros en esta iteración.
- El nombre de un grupo no necesita ser único a nivel global; es un dato legible, no un identificador.
- La eliminación de un miembro no elimina ningún dato histórico asociado a ese miembro dentro del grupo; es una baja lógica de membresía.
- El bounded context ShoppingList no se modifica en esta iteración. La integración entre usuarios/grupos y listas de compra se realizará en una feature posterior; `GroupId` (UUID generado por el sistema) actuará como referencia cruzada canónica: una futura lista compartida se vinculará a un `Group`, no a un usuario individual, siguiendo el principio PP3 de la constitución.
- Los grupos no son eliminables en esta iteración. La operación de eliminación de grupo queda explícitamente fuera de alcance hasta que el vínculo entre grupos y listas de compra esté definido, para evitar decisiones de diseño prematuras sobre el destino de los recursos vinculados.
- El bounded context Users ya existe parcialmente en la solución como proyecto scaffolded; esta feature formaliza su dominio, aplicación e infraestructura siguiendo el patrón de ShoppingList como referencia.
- No se implementará API Gateway ni se realizará despliegue productivo en Azure como parte de esta iteración.
