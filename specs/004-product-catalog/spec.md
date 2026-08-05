# Feature Specification: MVP-2 Catálogo de Productos y Precios

**Feature Branch**: `004-product-catalog`

**Created**: 2026-08-05

**Status**: Draft

**Input**: User description: "Crear la especificación 004-product-catalog para MVP-2, basada en 001-project-baseline, incluyendo catálogo reutilizable, mercados, precios por mercado, historial de precios y futura asociación con ítems de listas, con exclusiones explícitas."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Gestionar catálogo reutilizable (Priority: P1)

Como persona que prepara compras frecuentes, quiero mantener un catálogo único de productos reutilizables para no volver a escribir los mismos productos en cada lista.

**Why this priority**: Es la base funcional del MVP-2; sin catálogo reutilizable no existe valor acumulativo para precios ni para integración futura con listas.

**Independent Test**: Se puede validar creando, editando, consultando y desactivando productos del catálogo sin depender de mercados ni historial.

**Acceptance Scenarios**:

1. **Given** un usuario con permisos de gestión, **When** crea un producto con nombre y unidad de compra válidos, **Then** el producto queda disponible en el catálogo reutilizable.
2. **Given** un producto existente en el catálogo, **When** se actualizan sus datos descriptivos, **Then** los cambios quedan guardados y visibles en consultas posteriores.
3. **Given** un producto desactivado, **When** se consulta el catálogo activo, **Then** el producto no aparece como opción para nuevos registros de precio.

---

### User Story 2 - Gestionar mercados y registrar precios por mercado (Priority: P2)

Como persona que compra en distintos mercados, quiero registrar precios de un mismo producto por tienda para llevar control de cuánto cuesta en cada lugar.

**Why this priority**: Permite capturar el dato clave de negocio de MVP-2: precio por contexto de compra (mercado/tienda).

**Independent Test**: Se puede validar creando mercados y registrando precios de productos en fechas distintas, verificando que cada registro quede asociado al mercado correcto.

**Acceptance Scenarios**:

1. **Given** un mercado activo y un producto activo, **When** se registra un precio con fecha válida, **Then** el sistema guarda el registro con referencia al producto y al mercado.
2. **Given** múltiples precios del mismo producto en distintos mercados, **When** se consultan precios actuales, **Then** el sistema muestra una vista separada por mercado.
3. **Given** un intento de registrar un precio negativo o cero, **When** se envía la solicitud, **Then** el sistema rechaza el registro e informa el error de validación.

---

### User Story 3 - Consultar historial de precios y preparar asociación futura con listas (Priority: P3)

Como persona que quiere optimizar sus compras, quiero ver el historial de precios por producto y mercado, y dejar preparado el catálogo para asociarlo después con ítems de listas de compra.

**Why this priority**: Completa el valor analítico mínimo del MVP-2 y reduce retrabajo para MVPs posteriores de ShoppingList.

**Independent Test**: Se puede validar consultando el historial temporal de un producto en un mercado y verificando que cada ítem de historial conserve fecha y valor registrados.

**Acceptance Scenarios**:

1. **Given** un producto con múltiples precios históricos en un mercado, **When** se consulta su historial, **Then** se devuelve la secuencia ordenada por fecha.
2. **Given** un producto sin precios históricos, **When** se consulta su historial, **Then** el sistema devuelve resultado vacío sin error.
3. **Given** un producto de catálogo existente, **When** se consulta su identificador de referencia, **Then** puede utilizarse para futura vinculación con ítems de listas sin duplicar el producto.

---

### Edge Cases

- Intento de crear dos productos activos con el mismo nombre normalizado y misma unidad de compra.
- Registro de precio para un producto desactivado o para un mercado desactivado.
- Carga de precio con fecha futura.
- Consulta de historial en un rango de fechas donde no existen registros.
- Eliminación o desactivación de un mercado que ya tiene precios históricos asociados.
- Intento de duplicar precios para el mismo producto, mercado y fecha exacta.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema MUST permitir crear productos de catálogo reutilizables con datos mínimos de identificación y unidad de compra.
- **FR-002**: El sistema MUST permitir editar los datos descriptivos de un producto de catálogo existente.
- **FR-003**: El sistema MUST permitir desactivar un producto sin perder su historial de precios previo.
- **FR-004**: El sistema MUST impedir el alta de productos duplicados según una regla de unicidad de negocio definida sobre nombre normalizado y unidad de compra.
- **FR-005**: El sistema MUST permitir crear y mantener mercados o tiendas como entidades reutilizables.
- **FR-006**: El sistema MUST permitir desactivar mercados sin eliminar registros históricos de precios existentes.
- **FR-007**: El sistema MUST permitir registrar precios de productos por mercado con valor monetario positivo y fecha efectiva.
- **FR-008**: El sistema MUST validar que no se registren precios con fecha futura.
- **FR-009**: El sistema MUST permitir múltiples registros históricos de precio para el mismo producto y mercado en diferentes fechas.
- **FR-010**: El sistema MUST impedir registros de precio duplicados para la misma combinación de producto, mercado y fecha efectiva.
- **FR-011**: El sistema MUST permitir consultar el historial de precios por producto, con filtro opcional por mercado y rango de fechas.
- **FR-012**: El sistema MUST devolver el historial de precios ordenado cronológicamente e incluyendo fecha, mercado y valor.
- **FR-013**: El sistema MUST exponer para cada producto un identificador estable reutilizable para futura asociación con ítems de listas de compra.
- **FR-014**: El sistema MUST mantener independencia de alcance respecto a listas compartidas, reparto de gastos y recomendaciones inteligentes en este MVP.
- **FR-015**: El sistema MUST registrar auditoría mínima en productos, mercados y precios, incluyendo como mínimo `CreatedAt` y `UpdatedAt`; y cuando exista contexto de identidad, también `CreatedBy` y `UpdatedBy`.

### Alcance

**Incluye (MVP-2):**
- Catálogo reutilizable de productos.
- Gestión de mercados/tiendas.
- Registro y consulta histórica de precios por mercado.
- Preparación contractual para asociación futura producto-catálogo con ítem de lista.

**Fuera de alcance (explícito):**
- Listas compartidas y reparto de gastos.
- OCR y lectura de tickets.
- Alertas de promociones.
- Comparación automática o scraping de precios.
- Recomendaciones inteligentes.
- Cliente móvil .NET MAUI.
- Integración completa entre ShoppingList y Users.

### Key Entities *(include if feature involves data)*

- **ProductoCatalogo**: Representa un producto reutilizable. Atributos clave: identificador estable, nombre mostrado, nombre normalizado, unidad de compra, estado (activo/inactivo), fechas de creación y actualización.
- **Mercado**: Representa una tienda o punto de compra. Atributos clave: identificador, nombre, ubicación descriptiva opcional, estado (activo/inactivo), fechas de creación y actualización.
- **RegistroPrecioProducto**: Representa un precio observado para un producto en un mercado y fecha concreta. Atributos clave: identificador, producto, mercado, fecha efectiva, valor monetario, marca temporal de registro.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Al menos el 95% de los productos nuevos se pueden registrar completamente en menos de 2 minutos por usuarios internos de operación.
- **SC-002**: Al menos el 95% de los registros de precio válidos se completan en menos de 30 segundos desde el inicio de captura.
- **SC-003**: El 100% de las consultas de historial devuelven resultados cronológicos correctos para rangos de fechas válidos en pruebas de aceptación.
- **SC-004**: Al menos el 90% de usuarios en una prueba moderada (mínimo 10 participantes) reporta que puede identificar variaciones de precio entre mercados sin apoyo adicional.
- **SC-005**: El 100% de productos del catálogo queda disponible con identificador estable para su futura asociación con ítems de listas.

## Assumptions

- La gestión de catálogo y precios se realiza inicialmente por usuarios autenticados con permisos operativos ya existentes en el sistema.
- El MVP-2 se centra en captura y consulta manual de precios; no incluye ingestión automática desde fuentes externas.
- El valor monetario se registra en una moneda operativa única definida por el negocio para este MVP.
- La asociación directa entre productos de catálogo e ítems de ShoppingList se implementará en una spec posterior; en MVP-2 solo se garantiza identificador estable y reutilizable.
- La colaboración avanzada entre usuarios y reglas de pertenencia por grupo no se amplía en este MVP, manteniendo el alcance acotado.

