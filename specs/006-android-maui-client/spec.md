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
- Q: ¿Cómo debe comportarse la app cuando el access token expira durante el uso? → A: (Superseded por Session 2026-10-02) La app intenta adquisición silenciosa mediante `AcquireTokenSilent` de MSAL; si falla, se solicita reautenticación interactiva antes de reintentar la operación pendiente. La app no gestiona refresh tokens directamente en ningún momento.

### Session 2026-10-01

- Q: ¿Cómo se migran los usuarios existentes de `sub` a `(tid, oid)` sin crear duplicados ni perder el `UserId` interno? → A: (Superseded por Session 2026-10-02) La migración se ejecuta en dos fases: Fase 1 captura `(tid, oid)` y los asocia al `UserId` existente sin cambiar aún la correlación canónica; Fase 2 adopta `(tid, oid)` como correlación canónica. Nunca se crean perfiles duplicados; `sub` queda como dato legacy de solo lectura tras la Fase 2.
- Q: ¿Quién determina el `OwnerId` de una lista personal y cómo se protege el acceso entre usuarios? → A: El `OwnerId` se deriva exclusivamente del `UserId` interno resuelto por el backend a partir del token autenticado; el cliente nunca envía ni puede sobreescribir el `OwnerId`. Toda operación de lectura, edición o eliminación sobre una lista personal exige que el `UserId` autenticado coincida con el `OwnerId` de la lista, devolviendo error de autorización en caso contrario.
- Q: ¿Qué librería y qué información gestiona la sesión/token en el cliente? → A: La app usa MSAL (Microsoft Authentication Library) para todo el ciclo de vida de tokens (adquisición interactiva, adquisición silenciosa y caché); la app no implementa lógica propia de almacenamiento o renovación de refresh tokens. En `SecureStorage` del dispositivo solo se conserva la información mínima de sesión gestionada por MSAL (su caché cifrada de cuenta/token); la app no guarda copias adicionales de tokens ni credenciales en otro almacenamiento.
- Q: ¿MVP-4 debe incluir gestión de grupos desde el cliente o depende de Swagger/Users.Api directamente? → A: MVP-4 incluye gestión mínima de grupos en el cliente: listar grupos propios, crear grupo, ver miembros de un grupo, añadir/eliminar miembros según el rol del usuario autenticado (Owner/Admin conforme a las reglas ya definidas en Spec 003), y mostrar el rol propio (Owner/Admin/Member) en cada grupo. Esto permite preparar listas compartidas sin depender de Swagger.
- Q: ¿MVP-4 permite gestionar (crear/editar) el catálogo de productos, mercados y precios, o solo consultarlos? → A: MVP-4 es de solo lectura sobre `ProductCatalog.Api`: consulta de catálogo, mercados e historial de precios. La creación y edición de productos, mercados y precios permanece fuera de alcance de este MVP y podrá incorporarse en una spec posterior si se justifica por valor de usuario.
- Q: ¿La arquitectura de autenticación implementada para Android debe reutilizarse literalmente en Web/iOS? → A: No. Se reutilizan la identidad canónica `(tid, oid)`, el `UserId` interno, los contratos de API y la arquitectura de acceso a backend (capa de servicios/clientes HTTP), pero la implementación concreta de autenticación por plataforma (por ejemplo, MSAL para Android/iOS vs. un flujo OIDC basado en navegador/redirección para Web) se resuelve mediante adaptadores específicos por plataforma detrás de una abstracción común.

### Session 2026-10-02

- Q: ¿Las operaciones sobre listas personales deben exigir autenticación y cómo se garantiza que el `OwnerId` no pueda ser indicado arbitrariamente por el cliente? → A: Confirmado y reforzado: toda operación (crear, consultar, editar, eliminar) sobre listas personales requiere un token válido; el backend deriva el `OwnerId` efectivo exclusivamente de la identidad autenticada (`UserId` interno resuelto del token), rechaza cualquier valor de propietario enviado por el cliente en el cuerpo/parámetros de la solicitud, y devuelve error de autorización si el `UserId` autenticado no coincide con el `OwnerId` de la lista solicitada.
- Q: ¿Cómo debe interactuar exactamente la app con MSAL en cada llamada a una API protegida? → A: Confirmado: antes de cada llamada a una API protegida, la app DEBE invocar `AcquireTokenSilent` de MSAL contra la cuenta activa en caché; solo si `AcquireTokenSilent` falla (por ejemplo, por `MsalUiRequiredException` o equivalente) la app recurre a autenticación interactiva. La app no gestiona ni manipula refresh tokens directamente en ningún punto; toda la caché y ciclo de vida de tokens es responsabilidad exclusiva de MSAL. Al cerrar sesión, la app DEBE eliminar la cuenta de la caché de MSAL (`RemoveAccount` o equivalente).
- Q: ¿Cómo debe estructurarse exactamente la migración de identidad `sub -> (tid, oid)`? → A: Confirmado como transición en dos fases explícitas: **Fase 1 (convivencia)** — mientras la configuración de autenticación actual siga activa, el backend captura y persiste `tid` y `oid` del token entrante, asociándolos al `UserId` existente (correlacionado previamente por `sub`), sin cambiar aún la clave de correlación canónica ni la audience. **Fase 2 (corte)** — una vez que todos los clientes activos emiten tokens con `tid`/`oid` resueltos, el backend cambia la correlación canónica a `(tid, oid)` y adopta la audience de backend común (TP11) como mecanismo de validación principal. Durante ambas fases, `sub`/`ExternalUserId` se conserva únicamente como dato legacy de solo lectura, y en ningún caso se crea un `UserId` nuevo si el usuario ya puede correlacionarse con uno existente (por `(tid, oid)` o, transitoriamente, por `sub`).
- Q: ¿Qué alcance exacto de gestión de grupos debe implementar el cliente en MVP-4? → A: Confirmado el alcance mínimo ya definido: listar mis grupos (con rol propio), crear grupo, ver miembros, y añadir/eliminar miembros según los permisos ya soportados por el backend (Owner/Admin conforme a Spec 003). Queda explícitamente fuera de alcance cualquier administración avanzada de grupos (por ejemplo, renombrar grupo, transferir propiedad, configuración de grupo) y el cambio de rol de un miembro existente, dado que esta última capacidad no está soportada por el backend actual.
- Q: ¿Cuál es el alcance final de ProductCatalog en MVP-4? → A: Confirmado como solo lectura desde Android: consultar productos, consultar mercados y consultar historial de precios. No se implementa creación, edición ni eliminación de productos, mercados o precios desde el cliente en este MVP.

### Session 2026-10-03

- Q: ¿La Fase 1 de migración debe forzar la resolución de `(tid, oid)` para el 100% de los perfiles existentes, incluyendo aquellos que nunca vuelven a autenticar? → A: No. La Fase 1 es una migración lazy: solo se resuelven `tid`/`oid` para perfiles que efectivamente autentican durante esa fase, preservando siempre su `UserId` y sin duplicar perfiles. Los perfiles que nunca autentican permanecen legítimamente sin `(tid, oid)` resuelto y esto no es un defecto. La Fase 2 (corte de correlación canónica y adopción de audience común) solo puede iniciarse cuando se cumple explícitamente un criterio operativo de cobertura de perfiles activos o se ejecuta un backfill administrativo para los perfiles pendientes; no se define una fecha fija ni un backfill obligatorio de alcance ilimitado dentro de este MVP.
- Q: ¿`ProductCatalog.Api` debe incorporar autenticación JWT real y protección `[Authorize]` en sus endpoints de consulta? → A: Sí. `ProductCatalog.Api` debe adoptar el mismo esquema `AddAuthentication().AddJwtBearer(...)` ya usado por `Users.Api`/`ShoppingList.Api`, con la misma Authority y el mismo mecanismo `ValidAudiences` para la transición de audience común (TP11); los endpoints de consulta consumidos por el cliente pasan a requerir `[Authorize]`, y se añaden tests de `401 Unauthorized` sin token y de éxito con token válido.

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
3. **Given** un usuario que ya tenía perfil local basado en `sub` antes de esta migración, **When** inicia sesión nuevamente después del despliegue, **Then** el sistema captura y asocia `(tid, oid)` a su `UserId` existente (Fase 1) sin crear un perfil duplicado; una vez completada la Fase 2, ese mismo perfil queda correlacionado canónicamente por `(tid, oid)`.
4. **Given** credenciales inválidas o el usuario cancela el login, **When** el flujo de autenticación falla, **Then** la app muestra un mensaje de error claro y permite reintentar sin cerrar la aplicación.
5. **Given** un usuario ya autenticado con sesión activa, **When** vuelve a abrir la app, **Then** la app restaura la sesión sin solicitar login interactivo, salvo que el token no pueda renovarse silenciosamente.

---

### User Story 2 - Gestión segura del access token con MSAL (Priority: P1)

Como usuario autenticado, quiero que la app mantenga mi sesión de forma segura usando el ciclo de vida estándar de MSAL y la renueve automáticamente para no tener que iniciar sesión constantemente ni exponer mis credenciales.

**Why this priority**: Es prerequisito técnico para que cualquier otra historia de usuario funcione de forma continua; sin gestión de token no hay experiencia de uso viable.

**Independent Test**: Se puede validar forzando la expiración del access token y verificando que la app lo renueva de forma transparente mediante adquisición silenciosa de MSAL, o solicita reautenticación solo cuando es estrictamente necesario.

**Acceptance Scenarios**:

1. **Given** que la app necesita llamar a una API protegida, **When** prepara la solicitud, **Then** invoca `AcquireTokenSilent` de MSAL contra la cuenta activa en caché antes de ejecutar la solicitud.
2. **Given** que `AcquireTokenSilent` falla (por ejemplo, `MsalUiRequiredException` o equivalente), **When** la app detecta esta condición, **Then** solicita reautenticación interactiva mediante MSAL y reintenta la operación pendiente tras el éxito.
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
7. **Given** una solicitud sin token de acceso válido (no autenticada), **When** intenta crear, consultar, editar o eliminar una lista personal, **Then** el backend rechaza la operación (error de autenticación) sin ejecutar ninguna acción sobre datos de listas.

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
- **FR-003**: Antes de cada llamada a una API protegida, la app DEBE invocar `AcquireTokenSilent` de MSAL contra la cuenta activa en caché; solo si esta llamada falla (por ejemplo, `MsalUiRequiredException` o equivalente) la app DEBE recurrir a autenticación interactiva.
- **FR-004**: La app DEBE permitir cerrar sesión explícitamente, invocando `RemoveAccount` (o equivalente) de MSAL para eliminar la cuenta de la caché, y limpiando cualquier dato de sesión en memoria/caché local adicional asociado al usuario.
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
- **FR-016c**: La app DEBE ofrecer gestión mínima de grupos: listar grupos propios con el rol del usuario en cada uno, crear grupo, ver miembros de un grupo, y añadir/eliminar miembros respetando la matriz de autorización Owner/Admin/Member ya definida en Spec 003. La app NO DEBE implementar administración avanzada de grupos (renombrar, transferir propiedad, configuración) ni cambio de rol de un miembro existente, dado que esta última capacidad no está soportada por el backend actual.
- **FR-016d**: La app DEBE limitarse a operaciones de lectura sobre `ProductCatalog.Api` (catálogo de productos, mercados e historial de precios); la creación y edición de productos, mercados y precios desde el cliente queda fuera de alcance de MVP-4.

#### Identidad y Backend (cumplimiento de TP11)

- **FR-017**: El sistema DEBE autenticar a los usuarios del cliente Android mediante Microsoft Entra ID como proveedor OIDC externo, conforme a TP11.
- **FR-018**: El bounded context `Users` DEBE resolver la identidad externa canónica `(tid, oid)` de cada usuario y mantenerla como clave de correlación hacia el `UserId` interno de MiKompri.
- **FR-019**: El sistema NO DEBE utilizar el claim `sub` como identificador global persistente de correlación entre aplicaciones cliente a partir de esta spec.
- **FR-020**: La migración de identidad DEBE ejecutarse en dos fases explícitas. **Fase 1 (convivencia, migración lazy)**: mientras la configuración de autenticación actual siga activa, el backend captura `tid` y `oid` únicamente cuando un usuario autentica, y los asocia (persiste) contra el `UserId` existente, correlacionado hasta ese momento por `sub`, sin crear un perfil nuevo ni cambiar aún la clave de correlación canónica. Los perfiles que nunca vuelven a autenticar durante la Fase 1 permanecen legítimamente sin `tid`/`oid` resuelto; esto NO es un defecto ni bloquea el avance de la migración. **Fase 2 (corte)**: el sistema NO DEBE iniciar la Fase 2 hasta que se cumpla explícitamente al menos uno de los siguientes criterios de entrada: (a) un umbral de cobertura definido operativamente (por ejemplo, un porcentaje mínimo de perfiles **activos** —con autenticación reciente dentro de una ventana definida— ya correlacionados por `(tid, oid)`), verificable mediante consulta/reporte; o (b) la ejecución de un job de backfill que resuelva `(tid, oid)` para los perfiles pendientes que aún no hayan autenticado (por ejemplo, mediante Microsoft Graph u otro mecanismo administrativo), dejando constancia de los perfiles que no pudieron resolverse. Una vez cumplido el criterio elegido, el backend cambia la correlación canónica a `(tid, oid)` y adopta la audience de backend común (TP11) como mecanismo principal de validación. En ambas fases, el sistema NUNCA DEBE crear un `UserId` nuevo si el usuario ya puede correlacionarse con uno existente (por `(tid, oid)` o, transitoriamente, por `sub`).
- **FR-021**: Durante la Fase 1 y la Fase 2, el sistema DEBE seguir permitiendo la operación normal del backend existente (Users, ShoppingList, ProductCatalog) usando el `UserId` interno ya asignado; `sub`/`ExternalUserId` se conserva únicamente como dato legacy de solo lectura durante la transición y no se usa para nuevas correlaciones una vez completada la Fase 2.
- **FR-022**: Las APIs de MiKompri (`Users.Api`, `ShoppingList.Api`, `ProductCatalog.Api`) DEBEN tratarse como un único recurso lógico OAuth con una audience de backend común para el cliente Android, conforme a TP11.
- **FR-023**: Cada API DEBE continuar validando issuer, audience, firma, expiración y scopes aplicables de los tokens emitidos para el cliente Android, sin relajar controles existentes.
- **FR-023a**: `ProductCatalog.Api` DEBE incorporar autenticación JWT real equivalente a la ya existente en `Users.Api`/`ShoppingList.Api` (`AddAuthentication().AddJwtBearer(...)`, misma Authority, y soporte de `ValidAudiences` para la transición de audience común de TP11), y proteger con `[Authorize]` los endpoints de consulta (productos, mercados, historial de precios) consumidos por el cliente Android; el sistema DEBE rechazar con `401 Unauthorized` cualquier solicitud no autenticada a estos endpoints.
- **FR-024**: El registro de aplicación cliente en Microsoft Entra ID para Android DEBE ser independiente del de otros clientes futuros (Web, iOS), pero DEBE representar siempre al mismo usuario de MiKompri mediante `(tid, oid)`.
- **FR-025**: La autorización de negocio (Owner/Admin/Member, pertenencia a grupos, propiedad de listas) DEBE seguir siendo responsabilidad exclusiva de los bounded contexts correspondientes; el cliente Android no DEBE implementar lógica de autorización propia más allá de reflejar los estados/errores devueltos por el backend.
- **FR-026**: Para operaciones sobre listas personales, el backend (`ShoppingList`) DEBE derivar el `OwnerId` efectivo exclusivamente del `UserId` interno resuelto a partir del token autenticado; el backend NO DEBE aceptar ni confiar en un `OwnerId` u otro identificador de propietario enviado explícitamente por el cliente.
- **FR-027**: Toda operación (crear, consultar, editar, eliminar) sobre listas personales DEBE requerir un token de acceso válido; el backend DEBE rechazar con error de autenticación cualquier solicitud no autenticada sobre este recurso.

### Key Entities *(include if feature involves data)*

- **SesiónCliente**: Representa el estado de autenticación local en el dispositivo Android. Atributos conceptuales: token de acceso vigente, expiración, estado de sesión (activa/expirada/cerrada). No es una entidad de dominio de backend; vive únicamente en el cliente.
- **PerfilUsuario (vista de cliente)**: Proyección del perfil expuesto por `Users.Api` consumida por la app: nombre visible, email, `UserId` interno (no expuesto directamente como dato editable).
- **CorrelaciónIdentidadExterna** *(backend, bounded context Users)*: Relación `(tid, oid) -> UserId`. Extiende el modelo existente de `User` para incorporar la clave canónica de Microsoft Entra, conforme a TP11, sin eliminar la compatibilidad con perfiles migrados desde `sub`.

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Un usuario nuevo puede completar el inicio de sesión con Microsoft Entra ID y ver su perfil en menos de 1 minuto en condiciones normales de red.
- **SC-002**: El 100% de los perfiles de `Users` existentes antes de esta spec que autentican al menos una vez durante la Fase 1 obtienen `tid`/`oid` capturados y asociados a su `UserId` existente en esa misma autenticación, sin generar perfiles duplicados. La Fase 1 no exige ni espera que perfiles que nunca autentican sean migrados (migración lazy); la Fase 2 solo se inicia cuando se cumple el criterio explícito de cobertura o backfill definido en FR-020, momento en el cual los perfiles correlacionables quedan bajo `(tid, oid)` como clave canónica, verificado en pruebas de migración.
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
- La migración de `sub` a `(tid, oid)` se implementa en dos fases (convivencia y corte) como un cambio incremental y compatible hacia atrás: los perfiles existentes no pierden su `UserId` interno ni su historial, y nunca se crea un usuario nuevo si ya puede correlacionarse con uno existente.
- El cliente Android es la única superficie de usuario cubierta por esta spec; Web e iOS quedan fuera de alcance, pero la arquitectura de identidad y acceso a APIs debe quedar preparada para reutilizarse por esos clientes futuros.
- No se requiere soporte multi-cuenta simultáneo en el mismo dispositivo para este MVP.
- El modo offline se limita a mostrar el último estado conocido en pantalla cuando sea razonable; no se implementa sincronización bidireccional ni cola de operaciones pendientes en esta spec.
- La publicación en Google Play, y el soporte a otros sistemas operativos móviles, quedan fuera de alcance de esta spec.

---

## Alcance

**Incluye (MVP-4):**

- Cliente Android con .NET MAUI consumiendo `Users.Api`, `ShoppingList.Api` y `ProductCatalog.Api`.
- Inicio de sesión con Microsoft Entra ID y gestión segura de sesión/token en el dispositivo.
- Migración de correlación de identidad en `Users` de `sub` a `(tid, oid)` en dos fases (convivencia y corte), manteniendo el `UserId` interno existente.
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
- Administración avanzada de grupos (renombrar, transferir propiedad, configuración) y cambio de rol de un miembro existente, al no estar soportado por el backend actual.
- Creación, edición o eliminación de productos, mercados o precios desde el cliente Android (ProductCatalog es solo lectura en este MVP).
- Rediseño de los bounded contexts existentes (`Users`, `ShoppingList`, `ProductCatalog`) más allá de los cambios estrictamente necesarios para cumplir TP11 descritos en esta spec.
