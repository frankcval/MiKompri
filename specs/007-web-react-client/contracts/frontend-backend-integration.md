# Contract — Frontend/Backend Integration (007-web-react-client)

## Scope
Contrato de integración del cliente Web SPA con APIs existentes de MiKompri (`Users`, `ShoppingList`, `ProductCatalog`) manteniendo separación de bounded contexts.

## 1. Authentication Contract
- **Provider**: Microsoft Entra ID.
- **Flow**: Authorization Code + PKCE.
- **Client**: SPA registrada de forma independiente.
- **Token Use**: Bearer token por request a backend.
- **Non-Negotiable Rules**:
  - Sin client secret en frontend.
  - `(tid, oid)` y audience común de backend son decisiones cerradas.

## 2. Request Contract
- Header requerido: `Authorization: Bearer <access_token>`
- Header recomendado: `Content-Type: application/json`
- CORS backend debe permitir origen SPA por ambiente (local/cloud) y método/headers necesarios.

## 3. Response/Error Contract
- **200/201/204**: operación exitosa
- **400**: validación/negocio (mostrar mensaje accionable y errores de campo si aplica)
- **401**: sesión inválida/expirada (forzar reauth)
- **403**: autenticado sin permisos (mostrar estado forbidden)
- **404**: recurso no encontrado (estado not found)
- **409**: conflicto de negocio (mostrar mensaje y preservar contexto de formulario)
- **503**: servicio no disponible (mostrar estado y opción de reintento)

## 4. Bounded Context Separation
- `Users`: identidad/perfil/grupos/membresías.
- `ShoppingList`: listas personales/compartidas, ítems, gastos, balances, settlement.
- `ProductCatalog`: consulta read-only de productos/mercados/historial.
- El frontend no accede a bases de datos ni lógica interna cross-context.

## 5. Backend Prerequisites (Blocking)
1. Users migra correlación `sub -> (tid, oid)`.
2. Users mantiene `ExternalUserId` nullable en transición.
3. Backend mantiene audience común y validaciones homogéneas JWT.
4. ShoppingList autoriza listas personales por identidad autenticada y deriva `OwnerId`.
5. ProductCatalog aplica autenticación JWT real.

## 6. Deployment Compatibility (Future Azure SWA)
- Configuración por variables de entorno de frontend.
- Redirect URIs separados para local y cloud.
- Sin secretos en bundle.
- Compatible con despliegue en Azure Static Web Apps (preferente) cuando se habilite la fase cloud.