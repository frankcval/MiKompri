---
applyTo: "MiKompri.Web/tests/**,MiKompri.Web/**/*.spec.ts,MiKompri.Web/**/*.spec.tsx,MiKompri.Web/**/*.test.ts,MiKompri.Web/**/*.test.tsx"
---

# MiKompri Web Tests Instructions

## Enfoque

- Escribe tests orientados a comportamiento observable del usuario.
- Prioriza Given/When/Then implícito en nombres y escenarios.
- Evita tests acoplados a implementación interna.

## Cobertura mínima obligatoria por flujo relevante

Cada flujo principal debe cubrir como mínimo:

1. loading
2. empty state
3. error
4. 401
5. 403
6. flujo exitoso

## Herramientas

- Unit/component/integration: Vitest + React Testing Library + MSW.
- E2E: Playwright.

## Reglas de calidad

- Usa dobles de red con MSW para respuestas controladas.
- Verifica accesibilidad básica en UI crítica (roles/labels/focus visible).
- Mantén independencia entre tests; sin dependencias de orden.
- Evita snapshots masivos sin aserciones de comportamiento.

## Seguridad y contratos

- No hardcodear secretos, tokens reales ni credenciales en tests.
- Respetar contratos backend por bounded context (Users, ShoppingList, ProductCatalog).
- Validar manejo explícito de HTTP 400, 401, 403, 404, 409, 503 cuando el flujo lo requiera.