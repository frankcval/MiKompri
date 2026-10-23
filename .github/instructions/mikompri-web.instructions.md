---
applyTo: "MiKompri.Web/**"
---

# MiKompri Web Frontend Instructions

## Stack y arquitectura

- Usa React + TypeScript strict + Vite.
- Usa Fluent UI v9 como sistema principal de componentes.
- Organiza el código por features (`features/*`) y evita estructuras globales tipo Controllers/Services/Models.
- Usa React Router para navegación.
- No introduzcas Redux salvo necesidad justificada y documentada.

## Autenticación, identidad y seguridad

- Usa MSAL React / MSAL Browser para autenticación.
- Usa Authorization Code Flow + PKCE.
- MSAL administra tokens; no implementar manejo manual de refresh token.
- No incluir client secret ni secretos en frontend.
- Cumplir TP11: identidad canónica `(tid, oid)`, `UserId` interno y audience backend común.

## Datos, APIs y bounded contexts

- Usa TanStack Query para server state.
- No llames `fetch` directamente desde páginas/componentes.
- Usa clientes API tipados.
- Separa clientes por bounded context:
  - Users
  - ShoppingList
  - ProductCatalog
- Mantén reglas de negocio en backend; no duplicarlas en React.

## Formularios y validación

- Usa React Hook Form + Zod para formularios.
- Evita `any`; solo en casos excepcionales y documentados localmente.

## UI/UX y accesibilidad

- Usa primero componentes/tokens de Fluent UI antes de crear equivalentes personalizados.
- Diseña para desktop, tablet y mobile web.
- Accesibilidad obligatoria: navegación por teclado, focus visible, labels correctos, HTML semántico y contraste adecuado.
- Maneja de forma explícita estados/errores HTTP: 400, 401, 403, 404, 409 y 503.

## Pruebas

- Usa Vitest + React Testing Library + MSW para pruebas unitarias/de componente/integración.
- Usa Playwright para E2E.

## Resolución de conflictos de guía

- Los skills `frontend-design` y `web-design-guidelines` son complementarios.
- Si existe conflicto, prioriza: Spec Kit (`spec.md`, `plan.md`, `tasks.md`), constitución y Fluent UI.