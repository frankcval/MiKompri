# Feature Specification: MVP-4 Cliente Android con .NET MAUI

**Feature Branch**: `006-android-maui-client`

**Created**: 2026-09-30

**Status**: Draft

**Input**: User description: "Crear el primer cliente Android de MiKompri usando .NET MAUI, consumiendo las APIs existentes de Users, ShoppingList y ProductCatalog, con inicio de sesión mediante Microsoft Entra ID y cumplimiento obligatorio de TP11 (identidad canónica (tid, oid), UserId interno, audience lógica común), incluyendo la migración necesaria del backend desde `sub` hacia (tid, oid) antes de depender de esta identidad desde el cliente."

---

> ⚠️ **Naturaleza de esta spec**: Esta spec define el primer cliente de MiKompri (Android/.NET MAUI)
> y las adaptaciones mínimas de backend estrictamente necesarias para cumplir TP11 antes de que
> exista un cliente real dependiendo de la identidad externa. No introduce nuevos bounded contexts
> ni redefine los ya existentes, salvo el ajuste de correlación de identidad descrito aquí.

---

## Clarifications

### Session 2026-09-30

- Q: ¿Qué ocurre con los usuarios ya provisionados en `Users` que solo tienen `ExternalUserId` (`sub`) registrado? → A: Se requiere una migración de datos que resuelva `(tid, oid)` para los perfiles existentes en el primer login posterior al despliegue, sin duplicar perfiles y preservando `UserId` interno.
- Q: ¿El cliente Android debe soportar múltiples cuentas Microsoft Entra en el mismo dispositivo? → A: No en este MVP; una sesión activa por instalación de la app es suficiente. Cambiar de cuenta implica cerrar sesión y volver a autenticar.
- Q: ¿Cómo debe comportarse la app cuando el access token expira durante el uso? → A: Renovación silenciosa mediante refresh token/token silencioso de MSAL cuando sea posible; si falla, se solicita reautenticación interactiva antes de reintentar la operación pendiente.

### Session 2026-10-01

- Q: ¿Cómo se migran los usuarios existentes de `sub` a `(tid, oid)` sin crear duplicados ni perder el `UserId` interno? → A: Migración lazy (bajo demanda) ejecutada en el primer login posterior al despliegue: al recibir un token válido, `Users` busca primero por `(tid, oid)`; si no existe, busca por `sub` (compatibilidad transitoria) y, si encuentra coincidencia, actualiza ese mismo registro agregando `(tid, oid)` sin crear un perfil nuevo ni cambiar el `UserId`. Si no existe ninguna coincidencia, se crea un perfil nuevo correlacionado directamente por `(tid, oid)`. `sub` queda como dato heredado de solo lectura y no se usa para nuevas correlaciones una vez completada la migración del perfil.
- Q: ¿Quién determina el `OwnerId` de una lista personal y cómo se protege el acceso entre usuarios? → A: El `OwnerId` se deriva exclusivamente del `UserId` interno resuelto por el backend a partir del token autenticado; el cliente nunca envía ni puede sobreescribir el `OwnerId`. Toda operación de lectura, edición o eliminación sobre una lista personal exige que el `UserId` autenticado coincida con el `OwnerId` de la lista, devolviendo error de autorización en caso contrario.
- Q: ¿Qué librería y qué información gestiona la sesión/token en el cliente? → A: La app usa MSAL (Microsoft Authentication Library) para todo el ciclo de vida de tokens (adquisición interactiva, adquisición silenciosa y caché); la app no implementa lógica propia de almacenamiento o renovación de refresh tokens. En `SecureStorage` del dispositivo solo se conserva la información mínima de sesión gestionada por MSAL (su caché cifrada de cuenta/token); la app no guarda copias adicionales de tokens ni credenciales en otro almacenamiento.
- Q: ¿MVP-4 debe incluir gestión de grupos desde el cliente o depende de Swagger/Users.Api directamente? → A: MVP-4 incluye gestión mínima de grupos en el cliente: listar grupos propios, crear grupo, ver miembros de un grupo, añadir/eliminar miembros según el rol del usuario autenticado (Owner/Admin conforme a las reglas ya definidas en Spec 003), y mostrar el rol propio (Owner/Admin/Member) en cada grupo. Esto permite preparar listas compartidas sin depender de Swagger.
- Q: ¿MVP-4 permite gestionar (crear/editar) el catálogo de productos, mercados y precios, o solo consultarlos? → A: MVP-4 es de solo lectura sobre `ProductCatalog.Api`: consulta de catálogo, mercados e historial de precios. La creación y edición de productos, mercados y precios permanece fuera de alcance de este MVP y podrá incorporarse en una spec posterior si se justifica por valor de usuario.
- Q: ¿La arquitectura de autenticación implementada para Android debe reutilizarse literalmente en Web/iOS? → A: No. Se reutilizan la identidad canónica `(tid, oid)`, el `UserId` interno, los contratos de API y la arquitectura de acceso a backend (capa de servicios/clientes HTTP), pero la implementación concreta de autenticación por plataforma (por ejemplo, MSAL para Android/iOS vs. un flujo OIDC basado en navegador/redirección para Web) se resuelve mediante adaptadores específicos por plataforma detrás de una abstracción común.

---

## User Scenarios & Testing *(mandatory)*

Esta spec documenta las historias de usuario del primer cliente móvil de MiKompri, ordenadas por
prioridad para permitir una entrega incremental de un MVP funcional.

---

### User Story 1 - Inicio de sesión con Microsoft Entra ID (Priority: P1)

Como usuario de MiKompri, quiero iniciar sesión con mi cuenta Microsoft Entra ID desde la app Android para acceder a mis listas, catálogo y datos de grupo sin crear una identidad nueva.

**Why this priority**: Sin autenticación no existe ninguna otra funcionalidad posible del cliente. Es la puerta de entrada obligatoria y depende directamente de TP11.

**Independent Test**: Se puede validar instalando la app, iniciando sesión con una cuenta Entra de prueba y verificando que la app obtiene un access token válido y que el backend resuelve correctamente la identidad interna (`UserId`) asociada a `(tid, oid)`.

**Acceptance Scenarios**:

1. **Given** un usuario con cuenta Microsoft Entra ID válida, **When** inicia sesión desde la app Android, **Then** la app obtiene un token de acceso válido para el recurso lógico de backend de MiKompri.
2. **Given** un usuario que inicia sesión por primera vez desde cualquier cliente, **When** el backend recibe la primera solicitud autenticada, **Then** el bounded context `Users` resuelve `(tid, oid)` a un `UserId` interno, creando el perfil si no existe.
3. **Given** un usuario que ya tenía perfil local basado en `sub` antes de esta migración, **When** inicia sesión nuevamente después del despliegue, **Then** el sistema resuelve su perfil existente usando `(tid, oid)` sin crear un perfil duplicado.
4. **Given** credenciales inválidas o el usuario cancela el login, **When** el flujo de autenticación falla, **Then** la app muestra un mensaje de error claro y permite reintentar sin cerrar la aplicación.
5. **Given** un usuario ya autenticado con sesión activa, **When** vuelve a abrir la app, **Then** la app restaura la sesión sin solicitar login interactivo, salvo que el token no pueda renovarse silenciosamente.

---

### User Story 2 - Gestión segura del access token con MSAL (Priority: P1)

Como usuario autenticado, quiero que la app mantenga mi sesión de forma segura usando el ciclo de vida estándar de MSAL y la renueve automáticamente para no tener que iniciar sesión constantemente ni exponer mis credenciales.

**Why this priority**: Es prerequisito técnico para que cualquier otra historia de usuario funcione de forma continua; sin gestión de token no hay experiencia de uso viable.

**Independent Test**: Se puede validar forzando la expiración del access token y verificando que la app lo renueva de forma transparente mediante adquisición silenciosa de MSAL, o solicita reautenticación solo cuando es estrictamente necesario.

**Acceptance Scenarios**:

1. **Given** un access token próximo a expirar, **When** la app necesita llamar a una API protegida, **Then** intenta una adquisición silenciosa de token mediante MSAL antes de ejecutar la solicitud.
2. **Given** que la adquisición silenciosa falla, **When** la app detecta esta condición, **Then** solicita reautenticación interactiva mediante MSAL y reintenta la operación pendiente tras el éxito.
3. **Given** un usuario que cierra sesión explícitamente, **When** confirma la acción, **Then** la app invoca el `RemoveAccount` (o equivalente) de MSAL para limpiar la cuenta cacheada, junto con cualquier caché de sesión local relacionada con su identidad.
4. **Given** el ciclo de vida de tokens, **When** la app necesita adquirir, renovar o invalidar un token, **Then** toda esta gestión se delega exclusivamente a MSAL; la app no implementa lógica propia de almacenamiento, parsing o renovación manual de refresh tokens.
5. **Given** el almacenamiento seguro del dispositivo, **When** MSAL persiste su caché de cuenta/token, **Then** se utiliza el mecanismo de almacenamiento seguro nativo de la plataforma provisto por MSAL; la app no guarda copias adicionales de tokens, refresh tokens ni credenciales en otro almacenamiento propio (incluyendo `SecureStorage` u otro).

---

### User Story 3 - Consulta y gestión de perfil propio (Priority: P2)

Como usuario autenticado, quiero ver y actualizar mi nombre visible para que mis colaboradores me reconozcan en listas y grupos compartidos.

**Why this priority**: Es la funcionalidad básica de identidad visible para el usuario, ya soportada por el backend de `Users` (Spec 003), y de bajo esfuerzo relativo de UI.

**Independent Test**: Se puede validar consultando el perfil propio tras el login y actualizando el nombre visible, verificando que el cambio persiste al recargar la pantalla.

**Acceptance Scenarios**:

1. **Given** un usuario autenticado, **When** abre la pantalla de perfil, **Then** la app muestra nombre visible, email y datos básicos obtenidos de `Users.Api`.
2. **Given** un usuario que edita su nombre visible con un valor válido, **When** confirma el cambio, **Then** la app actualiza el perfil vía `Users.Api` y refleja el nuevo valor.
3. **Given** un intento de guardar un nombre visible vacío, **When** el usuario confirma, **Then** la app muestra un error de validación sin llamar a la API.

---

### User Story 4 - Listas personales (Priority: P1)

Como usuario autenticado, quiero crear, ver, editar y eliminar mis listas de compra personales desde el móvil para gestionar mis compras del día a día, con la certeza de que nadie más puede acceder a ellas.

**Why this priority**: Es el núcleo irrenunciable del producto (PP3) y debe estar disponible desde el primer cliente.

**Independent Test**: Se puede validar creando una lista personal, agregando un ítem, marcándolo como comprado y eliminando la lista, verificando persistencia contra `ShoppingList.Api`. Adicionalmente, se valida que un segundo usuario autenticado no puede ver ni modificar la lista del primero.

**Acceptance Scenarios**:

1. **Given** un usuario autenticado, **When** crea una lista personal con nombre válido, **Then** la lista se crea con `OwnerId` igual al `UserId` interno derivado del usuario autenticado (nunca de un valor enviado por el cliente) y aparece en su listado principal.
2. **Given** una lista personal existente, **When** el usuario la edita (nombre/descripción), **Then** los cambios se reflejan en la app y en el backend.
3. **Given** una lista personal existente, **When** el usuario la elimina, **Then** la lista desaparece del listado y de sus ítems asociados.
4. **Given** un error de red o backend al operar sobre una lista, **When** la operación falla, **Then** la app muestra un estado de error claro sin perder datos ya ingresados en el formulario.
5. **Given** una lista personal propiedad de otro usuario, **When** un usuario autenticado distinto intenta consultarla, editarla o eliminarla (por ejemplo, mediante un identificador conocido), **Then** el backend rechaza la operación con error de autorización y la app no expone ningún dato de esa lista.
6. **Given** listas personales creadas antes de esta spec con `OwnerId` ya asignado, **When** se despliega la nueva validación de autorización, **Then** dichas listas siguen siendo accesibles normalmente por su propietario original sin requerir migración de datos adicional.

---

### User Story 5 - Ítems de listas (Priority: P1)

Como usuario autenticado, quiero agregar, editar, marcar como comprado y eliminar ítems dentro de una lista para reflejar el progreso real de mi compra.

**Why this priority**: Es la unidad mínima de valor dentro de una lista; sin gestión de ítems, la lista no tiene utilidad práctica.

**Independent Test**: Se puede validar agregando varios ítems a una lista, marcando algunos como comprados y verificando que el progreso de la lista se actualiza correctamente en la UI.

**Acceptance Scenarios**:

1. **Given** una lista existente, **When** el usuario agrega un ítem con nombre, cantidad y precio, **Then** el ítem aparece en la lista y el progreso se recalcula.
2. **Given** un ítem existente, **When** el usuario lo marca como comprado, **Then** el estado visual cambia y el progreso de la lista aumenta.
3. **Given** un ítem existente, **When** el usuario lo elimina, **Then** desaparece de la lista y el progreso se recalcula.
4. **Given** un intento de agregar un ítem con `ProductId` duplicado en la misma lista, **When** se envía la solicitud, **Then** la app muestra el error de negocio devuelto por el backend sin duplicar el ítem localmente.

---

### User Story 5b - Gestión mínima de grupos (Priority: P2)

Como usuario autenticado, quiero listar mis grupos, crear un grupo nuevo, ver sus miembros y gestionar membresías según mi rol, para poder preparar listas compartidas sin depender de Swagger.

**Why this priority**: Es prerequisito funcional directo de las listas compartidas (User Story 6); sin un mínimo de gestión de grupos en el cliente, la colaboración por grupo no es utilizable de forma autónoma desde la app.

**Independent Test**: Se puede validar creando un grupo desde la app, verificando que el creador aparece como Owner, añadiendo un segundo miembro y comprobando que aparece en la lista de miembros con el rol asignado.

**Acceptance Scenarios**:

1. **Given** un usuario autenticado, **When** consulta la sección de grupos, **Then** ve el listado de grupos donde tiene membresía activa, con su rol (Owner/Admin/Member) en cada uno.
2. **Given** un usuario autenticado, **When** crea un grupo con nombre válido, **Then** el grupo se crea y el usuario queda registrado automáticamente como Owner.
3. **Given** un grupo existente, **When** el usuario consulta sus miembros, **Then** la app muestra la lista de miembros junto con el rol de cada uno.
4. **Given** un usuario con rol Owner o Admin en un grupo, **When** añade un nuevo miembro respetando las reglas de asignación de rol de Spec 003 (Admin solo puede asignar Member; Owner puede asignar Admin o Member), **Then** el miembro queda registrado y visible en la lista de miembros.
5. **Given** un usuario con rol Owner o Admin, **When** elimina a un miembro conforme a las reglas de autorización de Spec 003, **Then** el miembro deja de aparecer en la lista de miembros del grupo.
6. **Given** un usuario con rol Member, **When** intenta añadir o eliminar miembros, **Then** la app no ofrece esa acción o el backend la rechaza con error de autorización.

---

### User Story 6 - Listas compartidas y gastos colaborativos (Priority: P2)

Como miembro de un grupo, quiero ver y operar listas compartidas del grupo, incluyendo el registro de quién compró y pagó cada ítem, para colaborar de forma transparente con mi familia o grupo.

**Why this priority**: Es el diferenciador colaborativo del producto (PP4) y ya está soportado por el backend (Spec 005); el cliente debe exponerlo, pero puede entregarse después del núcleo de listas personales.

**Independent Test**: Se puede validar con un usuario miembro de un grupo, consultando las listas compartidas del grupo, registrando la compra de un ítem con participantes y verificando que la trazabilidad (`AddedBy`, `PaidBy`) se refleja en la UI.

**Acceptance Scenarios**:

1. **Given** un usuario con membresía activa en un grupo, **When** consulta sus listas, **Then** ve tanto sus listas personales como las listas compartidas de sus grupos, diferenciadas visualmente.
2. **Given** una lista compartida, **When** el usuario agrega un ítem, **Then** la app registra automáticamente su identidad como `AddedBy` sin pedirlo explícitamente.
3. **Given** un ítem comprado en una lista compartida, **When** el usuario registra el gasto (comprador, pagador, precio real, participantes), **Then** la app envía estos datos a `ShoppingList.Api` y refleja el registro en la UI.
4. **Given** un usuario sin membresía activa en el grupo de una lista compartida, **When** intenta acceder a ella (p. ej., por un enlace o estado obsoleto en caché), **Then** la app muestra un error de autorización sin exponer datos de la lista.

---

### User Story 7 - Balances y settlement del grupo (Priority: P2)

Como miembro de un grupo, quiero ver los balances de gasto y la propuesta de liquidación para saber cuánto debo o me deben dentro del grupo.

**Why this priority**: Cierra el ciclo de valor de las listas compartidas (Spec 005); sin esta vista, el registro de gastos no tiene utilidad práctica visible para el usuario.

**Independent Test**: Se puede validar con una lista compartida que tenga varios gastos registrados, consultando el resumen de balances y la propuesta de liquidación desde la app.

**Acceptance Scenarios**:

1. **Given** una lista compartida con gastos registrados, **When** el usuario abre la vista de balances, **Then** la app muestra el total pagado y el total correspondiente por participante.
2. **Given** balances calculados, **When** existen deudas pendientes, **Then** la app muestra la propuesta de liquidación (quién paga a quién y cuánto).
3. **Given** una lista compartida sin gastos válidos, **When** el usuario consulta balances, **Then** la app muestra un estado vacío indicando que no hay deudas pendientes, sin error.

---

### User Story 8 - Consulta de catálogo de productos, mercados e historial de precios (solo lectura) (Priority: P3)

Como usuario que planifica compras, quiero consultar el catálogo de productos, los mercados disponibles y el historial de precios para decidir mejor dónde y qué comprar.

**Why this priority**: Aporta valor analítico adicional ya soportado por el backend (Spec 004), pero no es bloqueante para el núcleo de listas y colaboración; puede entregarse en una fase posterior del MVP.

**Alcance explícito**: Esta historia es exclusivamente de **solo lectura** sobre `ProductCatalog.Api`. La creación y edición de productos, mercados y registro de precios desde el cliente Android queda fuera de alcance de MVP-4 (ver sección "Alcance").

**Independent Test**: Se puede validar consultando el catálogo de productos, filtrando por mercado y revisando el historial de precios de un producto concreto desde la app.

**Acceptance Scenarios**:

1. **Given** un usuario autenticado, **When** abre la sección de catálogo, **Then** ve la lista de productos activos disponibles desde `ProductCatalog.Api`.
2. **Given** un producto del catálogo, **When** el usuario consulta su historial de precios, **Then** la app muestra los precios ordenados cronológicamente, indicando el mercado de cada registro.
3. **Given** la sección de mercados, **When** el usuario la consulta, **Then** ve el listado de mercados activos disponibles para referencia.

---

### User Story 9 - Navegación principal y estados de la aplicación (Priority: P1)

Como usuario de la app, quiero una navegación clara entre las secciones principales (listas, catálogo, perfil, grupos) y saber cuándo la app está cargando, sin conexión o en error, para entender qué está pasando en todo momento.

**Why this priority**: Es la base de experiencia de usuario mínima viable; sin navegación clara y manejo de estados, ninguna otra historia es utilizable de forma confiable.

**Independent Test**: Se puede validar navegando entre las secciones principales de la app y forzando escenarios de pérdida de conectividad o error de backend, verificando que cada pantalla comunica su estado correctamente.

**Acceptance Scenarios**:

1. **Given** un usuario autenticado, **When** abre la app, **Then** accede a una navegación principal con acceso directo a Listas, Catálogo, Perfil y Grupos.
2. **Given** una operación de red en curso, **When** la app espera respuesta del backend, **Then** muestra un estado de carga claro en la pantalla correspondiente.
3. **Given** ausencia de conectividad de red, **When** el usuario intenta realizar una operación que requiere backend, **Then** la app muestra un estado de "sin conexión" explícito, sin bloquear el resto de la navegación.
4. **Given** un error no controlado del backend (p. ej., 5xx), **When** ocurre durante una operación, **Then** la app muestra un mensaje de error genérico y permite reintentar la operación.

---

### User Story 10 - Configuración de URLs de backend por entorno (Priority: P3)

Como responsable técnico del proyecto, quiero poder configurar las URLs base de Users, ShoppingList y ProductCatalog por entorno (desarrollo, staging, producción) para poder probar y desplegar la app sin recompilar lógica de negocio.

**Why this priority**: Es una necesidad operativa transversal, pero de menor prioridad que la funcionalidad orientada al usuario final; puede resolverse con un mecanismo simple de configuración.

**Independent Test**: Se puede validar cambiando la configuración de entorno de la app (por build configuration o archivo de configuración) y verificando que la app apunta a las URLs correctas sin cambios de código.

**Acceptance Scenarios**:

1. **Given** una build de la app configurada para un entorno específico, **When** la app se ejecuta, **Then** todas las llamadas a Users, ShoppingList y ProductCatalog usan las URLs base configuradas para ese entorno.
2. **Given** un cambio de configuración de entorno, **When** se genera una nueva build con la configuración actualizada, **Then** no es necesario modificar el código de las pantallas ni de los clientes HTTP.

---

### Edge Cases

- ¿Qué ocurre si el usuario revoca el consentimiento de la app en Microsoft Entra ID mientras tiene sesión activa en el dispositivo? La siguiente renovación de token debe fallar y forzar reautenticación.
- ¿Qué ocurre si el dispositivo pierde conectividad justo después de iniciar una operación de escritura (crear ítem, registrar gasto)? La app debe informar el fallo sin asumir éxito ni duplicar el envío automáticamente.
- ¿Qué ocurre si dos dispositivos del mismo usuario modifican la misma lista compartida casi simultáneamente? La app debe reflejar el último estado consistente devuelto por el backend al refrescar, sin intentar resolver conflictos de forma local.
- ¿Qué ocurre si el backend deniega el acceso por audience o scope incorrecto tras un cambio de configuración? La app debe mostrar un error de autorización distinguible de un error de conectividad.
- ¿Qué ocurre si un usuario con perfil migrado desde `sub` tiene datos parcialmente inconsistentes (por ejemplo, `tid`/`oid` no resueltos aún)? El login debe completar la migración de forma transparente o, si no es posible, informar un error claro sin bloquear permanentemente la cuenta.
- ¿Qué ocurre si el usuario minimiza la app durante el flujo interactivo de login y vuelve después? El flujo debe poder reanudarse o reiniciarse limpiamente sin dejar la app en un estado inconsistente.

---

## Requirements *(mandatory)*

### Functional Requirements

#### Cliente Android / .NET MAUI

- **FR-001**: La app DEBE permitir iniciar sesión mediante Microsoft Entra ID usando MSAL con un flujo interactivo estándar (OIDC/OAuth2 con PKCE) apropiado para aplicaciones móviles públicas.
- **FR-002**: La app DEBE delegar en MSAL el almacenamiento del access token y del refresh token (o equivalente), usando el caché cifrado nativo que MSAL gestiona sobre el mecanismo de almacenamiento seguro de la plataforma Android; la app no implementa un almacenamiento propio adicional para estos datos.
- **FR-003**: La app DEBE intentar renovar el access token mediante adquisición silenciosa de MSAL antes de solicitar reautenticación interactiva.
- **FR-004**: La app DEBE permitir cerrar sesión explícitamente, invocando la eliminación de cuenta de MSAL y limpiando cualquier dato de sesión en memoria/caché local adicional asociado al usuario.
- **FR-005**: La app DEBE permitir consultar y actualizar el perfil propio (nombre visible) contra `Users.Api`.
- **FR-006**: La app DEBE permitir crear, consultar, editar y eliminar listas personales contra `ShoppingList.Api`.
- **FR-007**: La app DEBE permitir agregar, editar, marcar como comprado y eliminar ítems dentro de una lista contra `ShoppingList.Api`.
- **FR-008**: La app DEBE permitir consultar las listas compartidas de los grupos a los que pertenece el usuario, diferenciándolas de las listas personales.
- **FR-009**: La app DEBE permitir registrar en ítems de listas compartidas el comprador, pagador, precio real y participantes del gasto, conforme al contrato de `ShoppingList.Api` (Spec 005).
- **FR-010**: La app DEBE permitir consultar balances por participante y la propuesta de liquidación de una lista compartida.
- **FR-011**: La app DEBE permitir consultar el catálogo de productos activos, los mercados activos y el historial de precios de un producto contra `ProductCatalog.Api`.
- **FR-012**: La app DEBE ofrecer una navegación principal con acceso a Listas, Catálogo, Perfil y Grupos.
- **FR-013**: La app DEBE comunicar explícitamente, para cada operación relevante contra el backend, al menos los estados: cargando, éxito, error y sin conexión.
- **FR-014**: La app DEBE permitir configurar las URLs base de `Users.Api`, `ShoppingList.Api` y `ProductCatalog.Api` por entorno (desarrollo, staging, producción) sin requerir cambios en el código de las pantallas.
- **FR-015**: La app NO DEBE implementar sincronización offline bidireccional; las operaciones de escritura requieren conectividad activa con el backend correspondiente.
- **FR-016**: La arquitectura del cliente DEBE estructurarse en capas que separen (a) la identidad canónica del usuario `(tid, oid)` y el `UserId` interno, (b) los contratos de acceso a `Users.Api`, `ShoppingList.Api` y `ProductCatalog.Api`, y (c) el mecanismo concreto de autenticación por plataforma. Los futuros clientes (Web, iOS) DEBEN poder reutilizar (a) y (b) sin cambios, implementando un adaptador de autenticación propio de su plataforma para (c) detrás de una abstracción común, en lugar de reutilizar literalmente la implementación de autenticación móvil basada en MSAL para Android.
- **FR-016a**: La app DEBE usar MSAL (Microsoft Authentication Library) como único mecanismo de adquisición, caché, renovación silenciosa e invalidación de tokens; la app NO DEBE implementar almacenamiento, parsing ni renovación manual de refresh tokens fuera de MSAL.
- **FR-016b**: La app NO DEBE persistir tokens, refresh tokens ni credenciales en `SecureStorage` u otro almacenamiento propio adicional al caché cifrado gestionado internamente por MSAL.
- **FR-016c**: La app DEBE ofrecer gestión mínima de grupos: listar grupos propios con el rol del usuario en cada uno, crear grupo, ver miembros de un grupo, y añadir/eliminar miembros respetando la matriz de autorización Owner/Admin/Member ya definida en Spec 003.
- **FR-016d**: La app DEBE limitarse a operaciones de lectura sobre `ProductCatalog.Api` (catálogo de productos, mercados e historial de precios); la creación y edición de productos, mercados y precios desde el cliente queda fuera de alcance de MVP-4.

#### Identidad y Backend (cumplimiento de TP11)

- **FR-017**: El sistema DEBE autenticar a los usuarios del cliente Android mediante Microsoft Entra ID como proveedor OIDC externo, conforme a TP11.
- **FR-018**: El bounded context `Users` DEBE resolver la identidad externa canónica `(tid, oid)` de cada usuario y mantenerla como clave de correlación hacia el `UserId` interno de MiKompri.
- **FR-019**: El sistema NO DEBE utilizar el claim `sub` como identificador global persistente de correlación entre aplicaciones cliente a partir de esta spec.
- **FR-020**: El backend DEBE ejecutar una migración de datos *lazy* (bajo demanda): en el primer login posterior al despliegue, `Users` busca el perfil primero por `(tid, oid)`; si no existe, busca por `sub` como mecanismo transitorio y, de encontrar coincidencia, actualiza ese registro agregando `(tid, oid)` sin crear un perfil nuevo ni modificar el `UserId` interno existente. Si tampoco existe coincidencia por `sub`, se crea un perfil nuevo correlacionado directamente por `(tid, oid)`.
- **FR-021**: Mientras un perfil no tenga `(tid, oid)` resuelto, el sistema DEBE seguir permitiendo la operación normal del backend existente (Users, ShoppingList, ProductCatalog) usando el `UserId` interno ya asignado, sin interrumpir funcionalidad ya operativa.
- **FR-022**: Las APIs de MiKompri (`Users.Api`, `ShoppingList.Api`, `ProductCatalog.Api`) DEBEN tratarse como un único recurso lógico OAuth con una audience de backend común para el cliente Android, conforme a TP11.
- **FR-023**: Cada API DEBE continuar validando issuer, audience, firma, expiración y scopes aplicables de los tokens emitidos para el cliente Android, sin relajar controles existentes.
- **FR-024**: El registro de aplicación cliente en Microsoft Entra ID para Android DEBE ser independiente del de otros clientes futuros (Web, iOS), pero DEBE representar siempre al mismo usuario de MiKompri mediante `(tid, oid)`.
- **FR-025**: La autorización de negocio (Owner/Admin/Member, pertenencia a grupos, propiedad de listas) DEBE seguir siendo responsabilidad exclusiva de los bounded contexts correspondientes; el cliente Android no DEBE implementar lógica de autorización propia más allá de reflejar los estados/errores devueltos por el backend.
- **FR-026**: Para operaciones sobre listas personales, el backend (`ShoppingList`) DEBE derivar el `OwnerId` efectivo exclusivamente del `UserId` interno resuelto a partir del token autenticado; el backend NO DEBE aceptar ni confiar en un `OwnerId` u otro identificador de propietario enviado explícitamente por el cliente.

### Key Entities *(include if feature involves data)*

- **SesiónCliente**: Representa el estado de autenticación local en el dispositivo Android. Atributos conceptuales: token de acceso vigente, expiración, estado de sesión (activa/expirada/cerrada). No es una entidad de dominio de backend; vive únicamente en el cliente.
- **PerfilUsuario (vista de cliente)**: Proyección del perfil expuesto por `Users.Api` consumida por la app: nombre visible, email, `UserId` interno (no expuesto directamente como dato editable).
- **CorrelaciónIdentidadExterna** *(backend, bounded context Users)*: Relación `(tid, oid) -> UserId`. Extiende el modelo existente de `User` para incorporar la clave canónica de Microsoft Entra, conforme a TP11, sin eliminar la compatibilidad con perfiles migrados desde `sub`.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Un usuario nuevo puede completar el inicio de sesión con Microsoft Entra ID y ver su perfil en menos de 1 minuto en condiciones normales de red.
- **SC-002**: El 100% de los perfiles de `Users` existentes antes de esta spec se resuelven correctamente a `(tid, oid)` en su primer login posterior al despliegue, sin generar perfiles duplicados, verificado en pruebas de migración.
- **SC-003**: Un usuario puede crear una lista personal, agregar un ítem y marcarlo como comprado en menos de 2 minutos desde la app, sin errores de sincronización con el backend.
- **SC-004**: El 100% de las operaciones de escritura sobre listas, ítems y gastos compartidos realizadas desde la app quedan reflejadas correctamente en `ShoppingList.Api` al consultar el estado actualizado.
- **SC-005**: Los usuarios pueden identificar en menos de 10 segundos si una operación está cargando, falló o no tiene conexión, medido en pruebas de usabilidad moderadas (mínimo 5 participantes).
- **SC-006**: El equipo puede cambiar el entorno de URLs de backend de la app (desarrollo/staging/producción) generando una nueva build, sin modificar código de pantallas, en menos de 15 minutos.
- **SC-007**: Ningún flujo de la app depende del claim `sub` como identificador de correlación de usuario; la verificación técnica confirma que toda correlación de identidad persistente usa `(tid, oid)` o el `UserId` interno.

---

## Assumptions

- Microsoft Entra ID ya es (o pasa a ser, mediante esta spec) el proveedor OIDC configurado para las APIs de MiKompri, conforme a TP11 de la constitución.
- El cliente Android se implementa con .NET MAUI, conforme a TP6 de la constitución, reutilizando C# como lenguaje principal del stack.
- Las APIs `Users.Api`, `ShoppingList.Api` y `ProductCatalog.Api` ya exponen las capacidades funcionales necesarias (Specs 003, 004 y 005); esta spec no redefine sus contratos salvo el cambio de correlación de identidad descrito en TP11.
- La migración de `sub` a `(tid, oid)` se implementa como un cambio incremental y compatible hacia atrás: los perfiles existentes no pierden su `UserId` interno ni su historial.
- El cliente Android es la única superficie de usuario cubierta por esta spec; Web e iOS quedan fuera de alcance, pero la arquitectura de identidad y acceso a APIs debe quedar preparada para reutilizarse por esos clientes futuros.
- No se requiere soporte multi-cuenta simultáneo en el mismo dispositivo para este MVP.
- El modo offline se limita a mostrar el último estado conocido en pantalla cuando sea razonable; no se implementa sincronización bidireccional ni cola de operaciones pendientes en esta spec.
- La publicación en Google Play, y el soporte a otros sistemas operativos móviles, quedan fuera de alcance de esta spec.

---

## Alcance

**Incluye (MVP-4):**

- Cliente Android con .NET MAUI consumiendo `Users.Api`, `ShoppingList.Api` y `ProductCatalog.Api`.
- Inicio de sesión con Microsoft Entra ID y gestión segura de sesión/token en el dispositivo.
- Migración de correlación de identidad en `Users` de `sub` a `(tid, oid)`, manteniendo el `UserId` interno existente.
- Perfil de usuario, listas personales (con autorización estricta por `OwnerId` derivado del usuario autenticado), listas compartidas, ítems, gastos compartidos, balances y liquidación, todo desde la app.
- Gestión mínima de grupos desde el cliente: listar grupos propios con rol, crear grupo, ver miembros, añadir/eliminar miembros según rol.
- Consulta de solo lectura del catálogo de productos, mercados e historial de precios (sin creación/edición desde el cliente).
- Navegación principal y manejo explícito de estados de carga, error y sin conexión.
- Configuración de URLs de backend por entorno.
- Arquitectura de identidad y acceso a API preparada para ser reutilizada por futuros clientes Web e iOS.

**Fuera de alcance (explícito):**

- Publicación en Google Play u otras tiendas de aplicaciones.
- Cliente iOS.
- Cliente Web.
- OCR y lectura de tickets.
- Notificaciones push.
- Scraping o comparación automática de precios.
- Recomendaciones inteligentes.
- Pagos bancarios o integración con pasarelas de pago.
- Modo offline completo con sincronización bidireccional.
- Rediseño de los bounded contexts existentes (`Users`, `ShoppingList`, `ProductCatalog`) más allá de los cambios estrictamente necesarios para cumplir TP11 descritos en esta spec.
