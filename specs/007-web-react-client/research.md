# Research — 007-web-react-client

## Decision 1: SPA con React + TypeScript strict + Vite + Fluent UI v9
- **Decision**: Adoptar SPA en React con TypeScript strict y Vite; UI con Fluent UI v9.
- **Rationale**: Alinea la spec aclarada, acelera ciclo de desarrollo, facilita modularidad por features y mantiene consistencia UX corporativa.
- **Alternatives considered**:
  - Next.js/SSR: descartado por fuera de alcance del MVP.
  - Stack UI alternativo: descartado por no estar en decisiones confirmadas.

## Decision 2: Autenticación con MSAL React/MSAL Browser + Authorization Code + PKCE
- **Decision**: Usar MSAL para login/logout/session restore y adquisición de tokens para APIs.
- **Rationale**: Cumple TP11, evita secretos en frontend y mantiene compatibilidad con Entra ID.
- **Alternatives considered**:
  - Flujos implícitos legacy: descartados por seguridad.
  - Gestión manual de OAuth: descartada por mayor riesgo y complejidad.

## Decision 3: Estado remoto con TanStack Query, sin Redux inicial
- **Decision**: Gestionar server state con TanStack Query y evitar Redux salvo necesidad demostrada.
- **Rationale**: Reduce boilerplate para CRUD API-driven y mejora caché/invalidation por feature.
- **Alternatives considered**:
  - Redux Toolkit global: descartado por complejidad temprana no justificada.

## Decision 4: API client común con propagación Bearer y manejo centralizado de errores HTTP
- **Decision**: Definir una capa única de cliente HTTP para token propagation, normalización de errores y mapping UI.
- **Rationale**: Asegura comportamiento uniforme para 400/401/403/404/409/503 y simplifica observabilidad funcional.
- **Alternatives considered**:
  - Clientes por feature sin base común: descartado por inconsistencia operativa.

## Decision 5: Formularios con React Hook Form + Zod
- **Decision**: Formularios validados por esquema con mapeo de errores backend.
- **Rationale**: Mejora DX y UX, valida datos antes de red y reduce errores de integración.
- **Alternatives considered**:
  - Validación ad-hoc por componente: descartada por duplicación y baja mantenibilidad.

## Decision 6: Arquitectura por features con separación explícita de bounded contexts backend
- **Decision**: Estructura `features/profile`, `groups`, `shopping-lists`, `shared-lists`, `settlements`, `product-catalog`.
- **Rationale**: Mantiene trazabilidad con `Users`, `ShoppingList`, `ProductCatalog` sin mezclar reglas de dominio backend en frontend.
- **Alternatives considered**:
  - Organización global Controllers/Services/Models: descartada por acoplamiento transversal.

## Decision 7: Estrategia local/cloud y Azure Static Web Apps futura
- **Decision**: Configuración por `.env.*`, redirect URIs local/cloud, CORS backend por ambiente; objetivo de despliegue futuro en Azure Static Web Apps con azd preferente.
- **Rationale**: Permite MVP local sin bloquear camino cloud y mantiene cero secretos en bundle.
- **Alternatives considered**:
  - Despliegue cloud inmediato obligatorio: descartado, no requerido para este MVP de planificación.