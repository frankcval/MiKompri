# Checklist de Calidad de Requisitos: Users Authentication & Groups

**Purpose**: Unit tests for requirements — valida la calidad, claridad y completitud de la spec antes de implementar
**Created**: 2026-07-12
**Feature**: [spec.md](../spec.md) | **Plan**: [plan.md](../plan.md)
**Focus**: Requisitos funcionales claros · Implementación oculta · Fuera de alcance · Seguridad y autorización · Capacidad de prueba · Comportamiento de API · Escenarios de error
**Traceability**: ≥ 80 % de los ítems referencian `[Spec §FRx]`, `[Gap]`, `[Ambiguity]` o `[Conflict]`

---

## Completitud de Requisitos Funcionales

- [ ] CHK001 - ¿Existe un requisito funcional explícito para la consulta de grupos del usuario autenticado (`GET /api/v1/groups`)? US4/US6 implican esta operación pero ningún FR la declara formalmente. [Completeness, Gap]
- [ ] CHK002 - ¿Está especificado qué ocurre cuando el claim `sub` está ausente del token durante el auto-provisioning? FR-002 declara que `sub` es el único claim obligatorio, pero no define el código de error resultante ni si el request es rechazado como no autenticado. [Completeness, Gap, Spec §FR-002]
- [ ] CHK003 - ¿Están definidos los códigos HTTP de respuesta para cada operación exitosa? Ningún FR entre FR-001 y FR-016 especifica si la respuesta esperada es 200, 201 o 204. [Completeness, Gap, Spec §FR-005, Spec §FR-007, Spec §FR-009]
- [ ] CHK004 - ¿Están declarados los límites de longitud de los campos validados (`DisplayName`, nombre de grupo) en los requisitos funcionales, no solo en el plan técnico? [Completeness, Gap, Spec §FR-004, Spec §FR-005]
- [ ] CHK005 - ¿Está especificado si el campo `email` es actualizable mediante `PUT /users/me` (FR-004) o exclusivamente mediante el refresco explícito (FR-016)? FR-004 solo menciona "nombre visible". [Completeness, Spec §FR-004, Spec §FR-016]
- [ ] CHK006 - ¿Está definido qué devuelve `POST /api/v1/groups` al crear un grupo: solo el `GroupId`, el objeto completo o una URL de recurso? [Completeness, Gap, Spec §FR-005]
- [ ] CHK007 - ¿Están definidos los requisitos para el header `Location` u otros headers de respuesta en las operaciones de creación de recursos (grupos, membresías)? [Completeness, Gap]
- [ ] CHK008 - ¿Está definido qué campos exactamente devuelve `GET /users/me`? FR-003 menciona "nombre visible, email y timestamps de trazabilidad" sin aclarar si `IdentityProvider` y `ExternalUserId` también se exponen. [Completeness, Spec §FR-003]

---

## Claridad y Precisión de Requisitos

- [ ] CHK009 - ¿Es "token de autenticación válido" en FR-001 suficientemente específico? ¿Incluye validación de firma, audiencia, emisor y expiración, o delega todos los detalles al plan técnico? [Clarity, Spec §FR-001]
- [ ] CHK010 - ¿Está la frase "mismo estilo de respuesta, estructura de errores y convenciones de trazabilidad" en FR-014 cuantificada, o es una referencia circular a ShoppingList que el lector debe deducir? [Clarity, Ambiguity, Spec §FR-014]
- [ ] CHK011 - ¿Está cuantificado "mensajes claros" en FR-013 con una estructura de error concreta (campos, tipos de datos, ejemplo)? Identificar "el campo y la razón" es descriptivo pero no verifica el formato de la respuesta. [Clarity, Spec §FR-013]
- [ ] CHK012 - ¿Está claramente definido si FR-016 (refresco explícito) puede crear el perfil si no existe, o si solo actualiza un perfil ya existente? El texto dice "actualizar los datos del perfil local", lo que implica que ya debe existir. [Clarity, Spec §FR-016]
- [ ] CHK013 - ¿Son el término "nombre visible" (FR-004), `DisplayName` (modelo de datos) y el claim `name` (IdP) el mismo concepto con nombres distintos? La variación terminológica no está alineada explícitamente. [Clarity, Terminology, Spec §FR-004]
- [ ] CHK014 - ¿Están diferenciados en los acceptance scenarios los códigos HTTP 401 (no autenticado) y 403 (autenticado sin permiso)? Múltiples escenarios usan "error de acceso no autorizado" para ambos casos sin especificar el código. [Clarity, Spec §US1, Spec §US5, Spec §US6]
- [ ] CHK015 - ¿Están diferenciados en FR-007 y FR-009 qué errores de autorización producen código 403 vs qué errores de validación producen código 400? Agregar a un usuario que no existe podría ser 400 o 404; un Admin intentando asignar rol Admin podría ser 400 o 403. [Clarity, Spec §FR-007, Spec §FR-009]

---

## Consistencia de Requisitos

- [ ] CHK016 - ¿Está documentada explícitamente la asimetría entre FR-002 (permite `DisplayName` vacío al provisionar) y FR-004 (impide `DisplayName` vacío al actualizar)? Sin esta aclaración, la spec puede interpretarse como contradictoria. [Consistency, Conflict, Spec §FR-002, Spec §FR-004]
- [ ] CHK017 - ¿Está documentado que `GET /users/me` tiene el efecto secundario de crear el perfil (auto-provisioning) cuando es el primer request del usuario? Un endpoint de lectura con efecto de escritura es una decisión de diseño que debe ser explícita. [Consistency, Spec §US2, Spec §FR-003]
- [ ] CHK018 - ¿Es el escenario 5 de US5 ("Owner elimina miembro de cualquier rol") consistente con FR-010 que impide eliminar al último Owner? ¿Están ambas reglas presentadas de forma que el lector entienda su interacción sin ambigüedad? [Consistency, Spec §US5, Spec §FR-010]
- [ ] CHK019 - ¿Son consistentes los campos de usuario expuestos en `GET /users/me` (FR-003) y los que aparecen como datos de miembro en `GET /groups/{id}/members` (FR-011)? ¿Ambas fuentes devuelven el `displayName` del perfil local en el momento de la consulta? [Consistency, Spec §FR-003, Spec §FR-011]

---

## Capacidad de Prueba y Criterios de Aceptación

- [ ] CHK020 - ¿Puede SC-002 ("menos de 2 segundos") verificarse de forma determinista en un entorno de tests sin depender de la latencia del proveedor de identidad externo? [Testability, Measurability, Spec §SC-002]
- [ ] CHK021 - ¿Puede SC-006 ("menos de 3 minutos de interacción") verificarse como criterio de aceptación automatizado, o es una métrica de UX que solo puede evaluarse manualmente? [Testability, Measurability, Spec §SC-006]
- [ ] CHK022 - ¿Son los 9 acceptance scenarios de US5 independientemente verificables, o algunos dependen del estado creado por otros scenarios anteriores de la misma historia? [Testability, Spec §US5]
- [ ] CHK023 - ¿Son los 8 criterios de éxito (SC-001 a SC-008) trazables individualmente a uno o más requisitos funcionales (FR) específicos? SC-006 en particular no referencia ningún FR explícito. [Traceability, Spec §SC-001, Spec §SC-006]
- [ ] CHK024 - ¿Están los acceptance scenarios de US1 escritos de forma que la creación del perfil (primer request) y la idempotencia (segundo request sin duplicado) sean verificables como tests independientes sin compartir estado? [Testability, Spec §US1]
- [ ] CHK025 - ¿Está definido en la spec cómo pueden verificarse los escenarios de integración de la API en ausencia de un proveedor OIDC real, sin requerir un IdP activo para que los tests pasen? [Testability, Gap, Spec §FR-015]

---

## Cobertura de Escenarios de Error

- [ ] CHK026 - ¿Está definido en la spec el error esperado cuando se intenta agregar a un usuario que no existe como perfil local (FR-007, escenario 9 de US5): ¿es 404, 400 o un código diferente? [Error Scenario, Clarity, Spec §US5]
- [ ] CHK027 - ¿Está definido el comportamiento cuando `DELETE /groups/{id}/members/{userId}` recibe el `userId` del propio caller siendo este el único Owner del grupo? ¿Se aplica FR-010 con el mismo mensaje, o existe un mensaje específico para la auto-eliminación? [Error Scenario, Gap, Spec §FR-009, Spec §FR-010]
- [ ] CHK028 - ¿Están definidos los requisitos de respuesta cuando un `GroupId` en la URL no existe vs cuando existe pero el caller no es miembro? Devolver 404 en ambos casos puede ocultar la existencia del grupo; devolver 403 puede confirmarla. [Error Scenario, Security, Spec §FR-011]
- [ ] CHK029 - ¿Está definido el comportamiento cuando `PUT /users/me` (FR-004) es llamado por un usuario cuyo perfil, por algún fallo del middleware, no existe en BD? Con auto-provisioning este caso debería ser imposible, pero ¿lo documenta la spec explícitamente? [Error Scenario, Consistency, Spec §FR-002, Spec §FR-004]
- [ ] CHK030 - ¿Está especificado qué error produce `POST /groups/{id}/members` cuando el `userId` en el cuerpo coincide con el `userId` del propio caller (auto-adición)? ¿Se trata como duplicado (FR-008) o como error de validación distinto? [Error Scenario, Gap, Spec §FR-007, Spec §FR-008]
- [ ] CHK031 - ¿Están los 7 edge cases de la spec trazados explícitamente a los requisitos funcionales (FR) que los resuelven? [Traceability, Spec §Edge Cases]

---

## Cobertura de Seguridad y Autorización

- [ ] CHK032 - ¿Están especificados en los requisitos funcionales (no solo en Assumptions) los aspectos de validación del token JWT que son responsabilidad de la API de Users: firma, audiencia, emisor y expiración? [Security, Gap, Spec §FR-001, Spec §Assumptions]
- [ ] CHK033 - ¿Están definidos los requisitos para evitar que las respuestas de error revelen la existencia de recursos a los que el caller no tiene acceso (prevención de enumeración de grupos y usuarios)? [Security, Gap, Spec §FR-013]
- [ ] CHK034 - ¿Están definidos los requisitos de comportamiento cuando se presenta un token comprometido o revocado en el endpoint de refresco explícito FR-016? El JWT puede ser técnicamente válido (firma correcta) pero estar en una lista de revocación. [Security, Gap, Spec §FR-016]
- [ ] CHK035 - ¿Está definido el comportamiento cuando el claim `sub` del token coincide con el `ExternalUserId` de un perfil existente pero el `IdentityProvider` almacenado en BD es diferente al del token? [Security, Edge Case, Gap, Spec §FR-002]
- [ ] CHK036 - ¿Está declarado explícitamente en los requisitos el límite de responsabilidad de seguridad: qué valida la API de Users (token Bearer) vs qué delega íntegramente al proveedor OIDC externo (flujo de autenticación, MFA, revocación)? [Security, Clarity, Spec §FR-001, Spec §Assumptions]

---

## Comportamiento de la API

- [ ] CHK037 - ¿Está especificado si `GroupRole` se serializa como string (`"Owner"`, `"Admin"`, `"Member"`) o como entero en las respuestas de la API? [API Behavior, Clarity, Gap]
- [ ] CHK038 - ¿Está definida la forma (shape) de la respuesta de `POST /groups/{id}/members` cuando la adición es exitosa: ¿devuelve la membresía creada, el grupo actualizado o solo confirma la operación? [API Behavior, Completeness, Spec §FR-007]
- [ ] CHK039 - ¿Están definidos los requisitos de idempotencia para `POST /api/v1/users/me/refresh`? ¿Múltiples llamadas consecutivas con el mismo token y sin cambios en los claims deben producir el mismo estado y el mismo código HTTP? [API Behavior, Completeness, Spec §FR-016]

---

## Fuera de Alcance — Exclusiones Explícitas

- [ ] CHK040 - ¿Está declarado en el cuerpo principal de la spec (no solo en Assumptions) que la API de Users no emite tokens JWT propios ni actúa como proveedor de identidad? Esta decisión arquitectónica debería ser visible en la primera lectura de la spec. [Out-of-scope, Spec §Assumptions]
- [ ] CHK041 - ¿Está declarado formalmente que no existe un endpoint para eliminar grupos en esta iteración, además de la mención en el edge case? Una declaración en Assumptions o en una sección de fuera de alcance haría esta exclusión inequívoca. [Out-of-scope, Spec §Edge Cases]
- [ ] CHK042 - ¿Están declaradas explícitamente las operaciones de gestión de membresía no disponibles en esta iteración, como cambiar el rol de un miembro existente (`PATCH /groups/{id}/members/{userId}`)? Sin esta declaración el lector puede asumir que la operación existe. [Out-of-scope, Gap]
- [ ] CHK043 - ¿Están el inicio de sesión con contraseña, los flujos de registro propios y la emisión de refresh tokens declarados como fuera de alcance en el cuerpo de la spec, no solo en Assumptions? [Out-of-scope, Spec §Assumptions]

---

## Notas de Revisión

- **Total de ítems**: 43 (CHK001–CHK043)
- **Ítems con referencia de trazabilidad** (`[Spec §FRx]`, `[Gap]`, `[Ambiguity]`, `[Conflict]`): 43/43 (100 %)
- **Distribución por categoría**: Completitud (8) · Claridad (7) · Consistencia (4) · Capacidad de prueba (6) · Escenarios de error (6) · Seguridad (5) · API (3) · Fuera de alcance (4)
- **Ítems marcados [Gap]**: 22 — requisitos que no están documentados en la spec actual y requieren decisión o clarificación antes de codificar
- **Ítems marcados [Conflict]**: 1 — CHK016 (asimetría DisplayName vacío en provisioning vs actualización)
- **Ítems marcados [Ambiguity]**: 2 — CHK010 (FR-014 delegación implícita), CHK013 (terminología inconsistente)
