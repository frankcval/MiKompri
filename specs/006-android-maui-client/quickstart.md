# Quickstart: Validación MVP-4 Cliente Android con .NET MAUI

Esta guía describe cómo validar end-to-end el cumplimiento de esta feature, una vez implementada. No contiene código de implementación; referencia los contratos en [contracts/api-contracts.md](./contracts/api-contracts.md) y el modelo en [data-model.md](./data-model.md).

## Prerrequisitos

- Backend (`Users.Api`, `ShoppingList.Api`, `ProductCatalog.Api`) corriendo localmente vía Docker Compose, con PostgreSQL disponible para cada bounded context (per TP4).
- Registro de aplicación cliente Android en Microsoft Entra ID (tenant de desarrollo), con los scopes necesarios para la audience de backend común.
- Emulador o dispositivo Android con el cliente `MiKompri.Mobile` instalado, apuntando a las URLs base de entorno "desarrollo" (FR-014).
- Al menos un usuario de prueba en el tenant de Entra ID.

## Escenario 1 — Login y resolución de identidad (Fase 1 de migración)

1. Levantar el backend con la configuración de Fase 1 activa (captura de `tid`/`oid` sin cambiar correlación canónica).
2. Iniciar sesión en el cliente con un usuario de prueba nuevo (sin perfil previo en `Users`).
3. Verificar en `Users.Api` que se creó un `User` con `TenantId`/`ObjectId` resueltos y `ExternalUserId` vacío o no aplicable (usuario nuevo, sin paso por `sub`).
4. Cerrar sesión (`RemoveAccount` vía MSAL) y volver a iniciar sesión con el mismo usuario.
5. Verificar que **no** se creó un segundo `User` (mismo `UserId` interno que en el paso 3).
6. **Resultado esperado**: SC-002 de la spec — ningún usuario existente pierde acceso ni se duplica.

## Escenario 2 — Migración de usuario legacy (`sub` → `(tid, oid)`), migración lazy

1. Provisionar manualmente (o mediante datos de prueba) un `User` existente correlacionado solo por `ExternalUserId` (`sub`), simulando un perfil pre-migración.
2. Iniciar sesión en el cliente con la cuenta Entra ID correspondiente a ese `sub`.
3. Verificar que el backend asocia `tid`/`oid` al `UserId` existente (mismo `Id`), sin crear un perfil nuevo.
4. Provisionar un segundo `User` legacy que **no** inicia sesión durante la ventana de prueba.
5. Verificar que este segundo perfil permanece con `TenantId`/`ObjectId` en `null` y `ExternalUserId` no nulo, sin que esto se trate como un error ni bloquee ninguna operación existente (migración lazy).
6. Verificar que la Fase 2 (cambio de correlación canónica a `(tid, oid)` y adopción de audience común) solo se activa en el entorno de prueba tras confirmar manualmente el cumplimiento de un criterio de cobertura de perfiles activos, o tras ejecutar un backfill simulado para perfiles pendientes.
7. **Resultado esperado**: FR-020 cumplido — convivencia sin duplicación, sin exigir migración de perfiles inactivos, y con un criterio explícito de entrada a Fase 2.

## Escenario 3 — Endurecimiento de `OwnerId` en listas personales

1. Autenticarse en el cliente como Usuario A.
2. Crear una lista personal desde el cliente.
3. Verificar en la base de datos de `ShoppingList` que `OwnerId` de la lista creada coincide con el `UserId` interno de Usuario A (nunca un valor arbitrario).
4. Intentar (por ejemplo, mediante una llamada directa a la API con un token de Usuario B) consultar/editar/eliminar la lista creada por Usuario A.
5. **Resultado esperado**: la API responde con error de autorización (403/404); el cliente MAUI nunca expone ni permite enviar un `OwnerId` propio al crear la lista (FR-026, FR-027).

## Escenario 4 — Gestión básica de grupos

1. Autenticarse como Usuario A.
2. Crear un grupo desde el cliente.
3. Añadir a Usuario B como miembro (`Member`).
4. Verificar que Usuario A ve su rol como `Owner` y Usuario B ve su rol como `Member` al listar grupos.
5. Intentar desde la cuenta de Usuario B (rol `Member`) añadir un nuevo miembro.
6. **Resultado esperado**: la operación es rechazada por el backend según la matriz de roles de Spec 003 (FR-016c); la UI del cliente refleja el rol correspondiente.

## Escenario 5 — Catálogo de productos (solo lectura, con autenticación real)

1. Sin autenticarse (sin token), intentar consultar directamente un endpoint de `ProductCatalog.Api` (por ejemplo, productos activos).
2. **Resultado esperado**: la API responde `401 Unauthorized` (FR-023a).
3. Autenticarse en el cliente y repetir la consulta con un token válido.
4. **Resultado esperado**: la API responde `200 OK` con el catálogo esperado.
5. Navegar a la sección de Catálogo en el cliente y consultar productos activos, mercados activos, e historial de precios de un producto.
6. Verificar que no existe ninguna acción de creación/edición disponible en la UI para catálogo, mercados o precios (FR-016d).

## Escenario 6 — Estados de UI (loading/error/offline)

1. Con el dispositivo sin conectividad, intentar crear un ítem en una lista.
2. **Resultado esperado**: la UI muestra un estado "sin conexión" distinguible de un error de servidor, sin intentar la operación de escritura (FR-013, FR-015).
3. Restaurar conectividad y reintentar; verificar transición a estado "cargando" y luego "éxito" o "error" según corresponda.

## Escenario 7 — Ciclo de vida de token (MSAL)

1. Forzar la expiración del access token (o esperar su expiración natural en un entorno de prueba configurado con vida corta).
2. Realizar una operación protegida desde el cliente.
3. **Resultado esperado**: el cliente invoca `AcquireTokenSilent` primero; si el refresco silencioso tiene éxito, la operación se completa sin interacción del usuario. Si falla, se solicita reautenticación interactiva antes de reintentar la operación pendiente (FR-003).
4. Cerrar sesión y verificar que la cuenta se elimina de la caché de MSAL (`RemoveAccount`), sin dejar tokens accesibles para la app (FR-004, FR-016b).

## Criterios de cierre

Esta feature se considera validada cuando los 7 escenarios anteriores pasan de forma reproducible en un entorno de desarrollo, y los tests automatizados descritos en `plan.md` § Estrategia de Tests pasan en CI (backend) y localmente (cliente).
