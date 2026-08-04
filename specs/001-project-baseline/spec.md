# Feature Specification: Baseline del Proyecto MiKompri

**Feature Branch**: `001-project-baseline`

**Created**: 2026-06-30

**Status**: Aprobada con gate abierto (TP5) y baseline vigente

---

> ⚠️ **Naturaleza de esta spec**: Esta es una spec de documentación, no de implementación.
> Su objetivo es establecer el estado de partida del proyecto antes de iniciar el
> desarrollo guiado por Spec Kit. No genera código nuevo; genera un registro de
> referencia sobre lo que existe, lo que está pendiente y las restricciones vigentes.

---

## User Scenarios & Testing *(mandatory)*

Esta spec documenta tres flujos de usuario que representan el núcleo de valor de MiKompri,
reflejando el estado actual de implementación de cada uno.

---

### User Story 1 - Gestión Personal de Listas de Compra (Priority: P1)

Un usuario puede crear una lista de compra personal, agregar productos con nombre, precio
y cantidad, actualizar los detalles de un producto, marcar productos como comprados y
eliminar productos o la lista completa. Puede consultar el progreso de compra de la lista
(porcentaje de ítems comprados).

**Estado actual**: ✅ Implementado y desplegable como slice funcional.  
**Trabajo activo relacionado**: `002-shopping-list-core` NO redefine este núcleo como un MVP
nuevo; lo trata como una fase de endurecimiento, alineación funcional y cierre de huecos
de consistencia sobre la capacidad ya existente.

**Why this priority**: Es el núcleo irrenunciable del producto (PP3). Sin esta capacidad,
MiKompri no tiene propuesta de valor.

**Independent Test**: Se puede verificar de forma autónoma mediante la API en
`/api/v1/PurchaseLists`. Los tests de integración en `PurchaseListsApiTests` cubren
este flujo end-to-end.

**Acceptance Scenarios**:

1. **Given** un usuario autenticado (actualmente solo con `OwnerId` como Guid), **When** envía
   `POST /api/v1/PurchaseLists` con nombre válido, **Then** se crea la lista y se retorna `201 Created` con el Id generado.
2. **Given** una lista existente, **When** el usuario hace `POST /api/v1/PurchaseLists/{id}/items`
   con un producto nuevo, **Then** el ítem se agrega y la lista refleja el progreso actualizado.
3. **Given** un ítem en la lista, **When** el usuario hace `PATCH .../items/{itemId}/purchase`,
   **Then** el ítem se marca como comprado y el porcentaje de progreso de la lista aumenta.
4. **Given** un producto ya existente en la lista, **When** se intenta agregar el mismo
   `ProductId` nuevamente, **Then** la operación falla con error `400 Bad Request` (regla de dominio: sin duplicados).
5. **Given** una lista con ítems, **When** se elimina la lista, **Then** la lista y todos sus
   ítems son eliminados.

---

### User Story 2 - Listas Colaborativas por Grupo (Priority: P2)

Varios miembros de un grupo pueden compartir listas de compra. Una lista puede estar asociada
a un grupo, y los miembros del grupo pueden consultarla y gestionarla. Se puede filtrar las
listas por grupo.

**Estado actual**: ⚠️ Parcialmente implementado. El dominio (`PurchaseList.GroupId`),
la query `GetShoppingListByGroupId` y el filtro en el endpoint `GET /api/v1/PurchaseLists?groupId=`
están implementados. Sin embargo, no existe gestión de grupos desde la API (Users API sin
implementar), y no hay validación de pertenencia al grupo ni permisos.

**Why this priority**: Diferenciador clave del producto frente a apps de lista individual
(PP4 - Transparencia Colaborativa). Requiere el bounded context `Users` completo.

**Independent Test**: El filtro por `groupId` puede probarse manualmente creando listas con
`GroupId` asignado. No existe cobertura de test automatizado para el flujo colaborativo completo.

**Acceptance Scenarios**:

1. **Given** una lista asociada a `groupId=X`, **When** un miembro del grupo consulta
   `GET /api/v1/PurchaseLists?groupId=X`, **Then** recibe todas las listas del grupo.
2. **Given** una lista compartida, **When** un miembro agrega un ítem, **Then** el ítem registra
   quién lo añadió (`AddedBy`) y todos los miembros del grupo pueden verlo.

   > ⚠️ El campo `AddedBy` en `ListItem` **no existe aún** en el dominio (deuda técnica DT-004).

---

### User Story 3 - Gestión de Usuarios y Grupos (Priority: P3)

Un usuario puede registrarse en MiKompri usando un proveedor de identidad externo
(OAuth/OIDC). Puede crear un grupo, invitar a otros miembros y asignarles roles
(Owner, Admin, Member). Los miembros del grupo pueden colaborar en listas compartidas.

**Estado actual**: 🔶 Dominio definido, aplicación y API sin implementar.
El bounded context `Users` tiene entidades (`User`, `Group`, `GroupMembership`, `GroupRole`)
y repositorios de infraestructura configurados. La capa Application está vacía.
La API expone únicamente el endpoint scaffolded `WeatherForecastController`.

**Why this priority**: Prerequisito para la colaboración real. Sin identidad de usuario
no hay transparencia colaborativa (PP4).

**Independent Test**: No se puede probar de forma independiente aún. Requiere que la capa
Application de Users esté implementada.

**Acceptance Scenarios**:

1. **Given** un token OAuth/OIDC válido de un proveedor soportado, **When** el usuario accede
   por primera vez, **Then** se crea automáticamente un perfil de usuario en MiKompri.
2. **Given** un usuario registrado, **When** crea un grupo con un nombre, **Then** el grupo
   se crea y el usuario queda como Owner automáticamente.
3. **Given** un grupo existente donde soy Owner, **When** invito a otro usuario con rol Member,
   **Then** el usuario pasa a ser miembro del grupo y puede acceder a las listas compartidas.

---

### Edge Cases

- ¿Qué ocurre si un usuario intenta acceder a una lista de un grupo al que no pertenece?
  (actualmente no hay validación de pertenencia).
- ¿Qué sucede si se elimina un grupo con listas asociadas? (relación `GroupId` en `PurchaseList`
  es nullable; no existe lógica de cascada o desvinculación definida).
- ¿Cómo se gestiona la revocación de membresía de un usuario que tiene ítems en listas
  compartidas?

---

## Requirements *(mandatory)*

### Functional Requirements

#### Bounded Context: ShoppingList — Estado Actual ✅

- **FR-001**: El sistema DEBE permitir crear listas de compra con nombre, propietario y grupo
  opcional.
- **FR-002**: El sistema DEBE permitir agregar ítems a una lista con `ProductId`, nombre, precio
  y cantidad.
- **FR-003**: El sistema DEBE impedir agregar un producto duplicado (mismo `ProductId`) a la
  misma lista.
- **FR-004**: El sistema DEBE permitir actualizar nombre, precio y/o cantidad de un ítem existente.
- **FR-005**: El sistema DEBE permitir marcar ítems como comprados.
- **FR-006**: El sistema DEBE calcular y exponer el progreso de compra de una lista
  (porcentaje de ítems comprados).
- **FR-007**: El sistema DEBE permitir filtrar listas por `ownerId` o `groupId`.
- **FR-008**: El sistema DEBE retornar errores estructurados con código HTTP apropiado y
  `traceId` para depuración.
- **FR-009**: El sistema DEBE exponer un endpoint de health check (`/health`).
- **FR-010**: El sistema DEBE documentar su API mediante OpenAPI/Swagger.

#### Bounded Context: Users — Dominio Definido, Aplicación Pendiente ⚠️

- **FR-011**: El sistema DEBE soportar usuarios vinculados a proveedores de identidad externos
  mediante `IdentityProvider` + `ExternalUserId` (OAuth/OIDC).
- **FR-012**: El sistema DEBE permitir crear grupos con nombre y asignar automáticamente al
  creador como Owner.
- **FR-013**: El sistema DEBE soportar tres roles de membresía: `Owner`, `Admin`, `Member`.
- **FR-014**: El sistema DEBE permitir añadir y remover miembros de un grupo.
- **FR-015**: El sistema DEBE impedir agregar al mismo usuario dos veces al mismo grupo.
- **FR-016** *(pendiente de implementar)*: La API de Users DEBE exponer endpoints REST para
  registro de usuario, gestión de grupos y membresías.

#### Restricciones de Constitución Vigentes

- **RC-001**: Ninguna feature nueva DEBE comenzar sin `spec.md`, `plan.md` y `tasks.md`
  (TP10).
- **RC-002**: Toda decisión técnica significativa DEBE documentarse como ADR o en `plan.md`
  (TP9).
- **RC-003**: La comunicación entre bounded contexts NO DEBE hacerse mediante acceso directo
  a base de datos ni referencias de dominio internas (TP2).
- **RC-004**: Todo PR de feature DEBE referenciar los tres artefactos de spec (TP10).

### Key Entities *(include if feature involves data)*

#### Bounded Context: ShoppingList

- **PurchaseList**: Aggregate root. Representa una lista de compra. Atributos: `Id`, `Name`,
  `Description`, `OwnerId` (Guid), `GroupId` (Guid?, nullable), `CreatedAt`, `UpdatedAt`.
  Contiene una colección de `ListItem`. Expone métodos de dominio: `AddItem`, `UpdateItem`,
  `DeleteItem`, `MarkItemAsPurchased`, `Rename`, `ChangeGroup`, `UpdateDescription`.
- **ListItem**: Entidad hija de `PurchaseList`. Atributos: `Id`, `ProductId` (Guid, referencia
  externa), `Name` (copia para visualización), `Price`, `Quantity`, `IsPurchased`,
  `PurchaseListId`. ⚠️ Falta campo `AddedBy` (deuda DT-004).
- **ListProgress**: Value Object. Calcula el porcentaje de ítems comprados sobre el total.

#### Bounded Context: Users

- **User**: Entidad. Atributos: `Id`, `DisplayName`, `Email`, `IdentityProvider`,
  `ExternalUserId`. Soporte multi-IdP (Keycloak, Auth0, Entra, MiKompri-Auth).
- **Group**: Entidad. Atributos: `Id`, `Name`, `OwnerId`. Contiene colección de
  `GroupMembership`. El owner se agrega automáticamente al crear el grupo.
- **GroupMembership**: Entidad. Atributos: `Id`, `GroupId`, `UserId`, `Role` (enum:
  `Owner`, `Admin`, `Member`).

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Esta spec está aprobada y los tres artefactos (`spec.md`, `plan.md`, `tasks.md`)
  existen bajo `specs/001-project-baseline/` antes de continuar con cualquier nueva feature.
- **SC-002**: El equipo puede crear una nueva spec de feature en menos de 10 minutos usando
  los comandos de Spec Kit (`/speckit.specify`, `/speckit.plan`, `/speckit.tasks`).
- **SC-003**: Todos los tests existentes de ShoppingList pasan en cada ejecución del pipeline CI
  para `push` y `pull_request`, y la rama `main` mantiene estado verde en el último build exitoso.
- **SC-004**: El directorio `specs/` existe y contiene al menos esta spec, estableciendo el
  patrón para futuras specs de features.
- **SC-005**: Las deudas técnicas documentadas (DT-001 a DT-008) tienen un issue de seguimiento
  creado en GitHub o están referenciadas en el backlog.
- **SC-006**: El equipo puede identificar la siguiente feature activa a ejecutar:
  `002-shopping-list-core` como hardening/completado del core individual de ShoppingList,
  dejando autenticación de usuarios y evolución de `Users` como trabajo posterior.

---

## Assumptions

- Los bounded contexts `ShoppingList` y `Users` no se comunican directamente en el estado
  actual. `OwnerId` y `GroupId` en `PurchaseList` son Guids sin validación cruzada con el
  contexto `Users` (asumido intencional como deuda técnica DT-001).
- El entorno de producción actual es GitHub Container Registry (GHCR). El despliegue en
  Azure está planificado pero no implementado (ver DT-007).
- No existe autenticación/autorización en ningún endpoint. Los endpoints son públicos
  (asumido intencional para MVP-0).
- `ProductId` en `ListItem` es una referencia externa (Guid). No existe un bounded context
  de Productos actualmente; se asume que el producto es identificado externamente o
  manualmente por el cliente.
- La deuda técnica documentada en esta spec no bloquea el uso del sistema actual, pero
  DEBE ser abordada antes de lanzar la funcionalidad colaborativa a usuarios reales.

---

## Estado Actual del Proyecto por Bounded Context

### ShoppingList — Operacional ✅

| Capa           | Estado       | Observaciones                                              |
|----------------|--------------|------------------------------------------------------------|
| Domain         | ✅ Completo   | PurchaseList, ListItem, ListProgress implementados y con tests |
| Application    | ✅ Completo   | 7 Commands, 5 Queries, validadores, pipeline behaviors     |
| Infrastructure | ⚠️ Parcial    | EF Core + PostgreSQL operativos, Unit of Work implementado; migraciones explícitas pendientes (ADR-002) |
| API            | ✅ Completo   | REST `/api/v1/PurchaseLists`, Swagger, health check        |
| Tests          | ✅ Completo   | Domain, Application y API con cobertura automatizada en CI |
| CI/CD          | ✅ Activo     | CI: build+test+SonarCloud. CD: push imagen a GHCR          |

### Users — Dominio Definido, Sin Operacional ⚠️

| Capa           | Estado           | Observaciones                                              |
|----------------|------------------|------------------------------------------------------------|
| Domain         | ✅ Completo       | User, Group, GroupMembership, GroupRole definidos          |
| Application    | 🔴 Sin implementar | Sin commands, queries ni handlers                        |
| Infrastructure | ⚠️ Parcial        | DbContext + repos creados, sin migraciones conocidas       |
| API            | 🔴 Solo scaffolding | WeatherForecastController (placeholder), sin endpoints reales |
| Tests          | 🔴 Sin cobertura  | No existen tests para el bounded context Users             |
| CI/CD          | ⚠️ Parcial        | El pipeline CI no incluye tests de Users                   |

---

## Deuda Técnica Identificada

| ID     | Descripción                                                                          | Prioridad | Tipo              |
|--------|--------------------------------------------------------------------------------------|-----------|-------------------|
| DT-001 | `OwnerId` y `GroupId` en `PurchaseList` son Guids sin validación cruzada con `Users` | Alta      | Integridad        |
| DT-002 | `Class1.cs` placeholder en `MiKompri.ShoppingList.Domain` (archivo vacío/residual)   | Baja      | Limpieza          |
| DT-003 | `ProgramTest.cs` en `MiKompri.ShoppingList.Api` con propósito incierto               | Baja      | Limpieza          |
| DT-004 | `ListItem` no tiene campo `AddedBy`/`CreatedBy` (viola PP4 - Transparencia Colaborativa) | Alta   | Funcionalidad     |
| DT-005 | Nombres de carpetas/archivos con typos: `AddAplicaction`, `DeleteShoppinList`, `UpdateItemShoopingListCommand` | Baja | Nombrado |
| DT-006 | Métodos de `ListItem` en minúscula (`updateName`, `updatePrice`, `updateQuantity`) — viola convenciones C# | Media | Convenciones |
| DT-007 | CD pipeline solo publica en GHCR; no despliega en Azure (TP5 pendiente de cumplir)   | Alta      | Infraestructura   |
| DT-008 | Users API sin implementar (WeatherForecastController como placeholder)               | Alta      | Funcionalidad     |
| DT-009 | Sin autenticación ni autorización en ningún endpoint (aceptable en MVP-0, bloqueante para producción colaborativa) | Alta | Seguridad |
| DT-010 | Docker Compose no incluye Users API ni define configuración para entorno completo     | Media     | Infraestructura   |

---

## MVPs Definidos

> **Nota de trazabilidad**: La numeración de specs (`001-`, `002-`, `003-`…) no equivale
> directamente a la numeración de MVPs. Varias specs pueden contribuir a un mismo MVP
> (como ocurre con MVP-0), y una spec puede ser de tipo documental/baseline sin representar
> un MVP propio. La tabla siguiente establece la trazabilidad explícita entre specs y MVPs.

---

### Tabla de trazabilidad MVP ↔ Specs

| MVP    | Nombre                                             | Estado           | Specs relacionadas                                      |
|--------|----------------------------------------------------|------------------|---------------------------------------------------------|
| MVP-0  | Listas de compra personales / Shopping List Core   | ✅ Completado     | `001-project-baseline`, `002-shopping-list-core`        |
| MVP-1  | Usuarios, autenticación e identidad                | 🟡 En curso (Draft) | `003-users-authentication`                           |
| MVP-2  | Catálogo de productos, mercados e historial de precios | ⬜ Pendiente   | `004-product-catalog` *(spec futura)*                   |
| MVP-3  | Listas compartidas y reparto de gastos             | ⬜ Pendiente     | `005-shared-lists-settlement` *(spec futura)*           |
| MVP-4  | Cliente Android con .NET MAUI                      | ⬜ Pendiente     | `006-android-maui-client` *(spec futura)*               |
| MVP-5  | Presupuesto y control de gasto                     | ⬜ Pendiente     | *(spec futura)*                                         |
| MVP-6  | Sugerencias inteligentes                           | ⬜ Pendiente     | *(spec futura)*                                         |
| MVP-7  | Tickets, fotos y OCR                               | ⬜ Pendiente     | *(spec futura)*                                         |
| MVP-8  | Promociones y alertas                              | ⬜ Pendiente     | *(spec futura)*                                         |
| MVP-9  | Comunidad e integración con comercios              | ⬜ Pendiente     | *(spec futura)*                                         |

---

### MVP-0 — Listas de Compra Personales / Shopping List Core (Estado: ✅ Completado)

**Alcance**: Un usuario puede crear y gestionar listas de compra propias con sus ítems.
Sin autenticación. Sin colaboración. Sin cliente móvil.

**Criterio de done**: ShoppingList API funcional y publicada en GHCR, con tests en CI verde.

**Specs que cubren este MVP**:
- `001-project-baseline`: Documentación del estado de partida del proyecto antes de adoptar
  GitHub Spec Kit. Establece la baseline documental del MVP-0.
- `002-shopping-list-core`: Hardening y cierre funcional del core de listas de compra ya
  existente. No define un MVP nuevo; completa y consolida el MVP-0.

---

### MVP-1 — Usuarios, Autenticación e Identidad (Estado: 🟡 En curso / Draft)

**Alcance**: Los usuarios se autentican con un IdP externo (OAuth/OIDC). La API valida
tokens. `OwnerId` en `PurchaseList` se vincula al usuario autenticado. Bounded context
`Users` operacional con registro de perfil automático, consulta y actualización de perfil,
y gestión de grupos colaborativos con roles Owner/Admin/Member.

**Prerequisito de PP4**: Sin identidad real no hay transparencia colaborativa.

**Spec relacionada**: `003-users-authentication`

**Decisiones pendientes (requieren ADR)**:
- Elección del IdP: Keycloak, Auth0, Entra B2C u otro.
- Estrategia de autorización en API: JWT Bearer, middleware, políticas.

---

### MVP-2 — Catálogo de Productos, Mercados e Historial de Precios (Estado: ⬜ Pendiente)

**Alcance**: Gestión de un catálogo de productos reutilizables, asociación a mercados
o tiendas y registro histórico de precios para facilitar comparativas de compra.

**Prerequisito**: MVP-1 completado.

**Spec futura sugerida**: `004-product-catalog`

---

### MVP-3 — Listas Compartidas y Reparto de Gastos (Estado: ⬜ Pendiente)

**Alcance**: Un usuario puede crear grupos, invitar miembros y compartir listas con el
grupo. Cada ítem registra quién lo añadió (`AddedBy`). El acceso a listas de grupo está
restringido a sus miembros. Incluye reparto de gastos entre participantes.

**Prerequisito**: MVP-1 completado.

**Spec futura sugerida**: `005-shared-lists-settlement`

---

### MVP-4 — Cliente Android con .NET MAUI (Estado: ⬜ Pendiente)

**Alcance**: Aplicación Android que permite gestionar listas de compra personales y
grupales con UX móvil primera (PP5). Consume las APIs REST de MiKompri.

**Prerequisito**: MVP-1 o MVP-3 completado.

**Spec futura sugerida**: `006-android-maui-client`

---

### MVP-5 — Presupuesto y Control de Gasto (Estado: ⬜ Pendiente)

**Alcance**: Herramientas para establecer presupuestos de compra y hacer seguimiento
del gasto real frente al planificado.

**Prerequisito**: MVP-2 o MVP-3 completado.

---

### MVP-6 — Sugerencias Inteligentes (Estado: ⬜ Pendiente)

**Alcance**: Recomendaciones automáticas de productos basadas en historial de compra,
patrones de uso y preferencias del usuario o grupo.

**Prerequisito**: MVP-2 o MVP-5 completado.

---

### MVP-7 — Tickets, Fotos y OCR (Estado: ⬜ Pendiente)

**Alcance**: Captura de tickets de compra mediante foto con extracción automática de
productos y precios por OCR, integrada con el historial de compras.

**Prerequisito**: MVP-2 completado.

---

### MVP-8 — Promociones y Alertas (Estado: ⬜ Pendiente)

**Alcance**: Notificaciones de ofertas y bajadas de precio sobre productos del catálogo
o de listas activas del usuario.

**Prerequisito**: MVP-2 completado.

---

### MVP-9 — Comunidad e Integración con Comercios (Estado: ⬜ Pendiente)

**Alcance**: Funcionalidades sociales (valoraciones, listas públicas, tendencias) e
integración directa con comercios para catálogos y precios en tiempo real.

**Prerequisito**: MVP-6, MVP-7 o MVP-8 completado.

---

## Cómo Continuar el Desarrollo con Spec Kit

El flujo de trabajo estándar para cualquier nueva feature a partir de esta baseline es:

```
1. /speckit.specify  → Crea spec.md con requisitos y criterios de aceptación
2. /speckit.clarify  → Resuelve ambigüedades antes de planificar (si aplica)
3. /speckit.plan     → Crea plan.md con diseño técnico y ADRs
4. /speckit.tasks    → Crea tasks.md con ítems ejecutables
5. /speckit.implement → Implementa guiado por tasks.md
6. /speckit.checklist → Valida cumplimiento antes de PR
```

**Próximo paso activo**: `003-users-authentication` — MVP-1 (Usuarios, autenticación e identidad).
La spec `002-shopping-list-core` ya está implementada y representa el cierre del MVP-0.
El flujo recomendado para futuras specs sigue siendo:
```
/speckit.specify  → Crea spec.md con requisitos y criterios de aceptación
/speckit.clarify  → Resuelve ambigüedades antes de planificar (si aplica)
/speckit.plan     → Crea plan.md con diseño técnico y ADRs
/speckit.tasks    → Crea tasks.md con ítems ejecutables
/speckit.implement → Implementa guiado por tasks.md
/speckit.checklist → Valida cumplimiento antes de PR
```
