# Implementation Plan: 007-web-react-client

**Branch**: `007-web-react-client` | **Date**: 2026-10-23 | **Spec**: [`spec.md`](./spec.md)

**Input**: Feature specification from `/specs/007-web-react-client/spec.md`

## Summary

Construir el plan técnico de un cliente Web SPA de MiKompri como MVP-4 (siguiente iniciativa activa), priorizando validación full-stack de identidad, autorización y contratos de los bounded contexts `Users`, `ShoppingList` y `ProductCatalog` sin acoplarse a decisiones específicas de MAUI. La implementación se organizará por features con React + TypeScript strict + Vite + Fluent UI v9, autenticación MSAL (Authorization Code + PKCE), estado servidor con TanStack Query y estrategia de despliegue futura en Azure Static Web Apps.

## Technical Context

**Language/Version**: TypeScript (strict) + React 18 (SPA)

**Primary Dependencies**: React, Vite, Fluent UI v9, React Router, MSAL React/MSAL Browser, TanStack Query, React Hook Form, Zod

**Storage**: N/A (cliente web stateless; persistencia de negocio en APIs backend)

**Testing**: Vitest, React Testing Library, MSW, Playwright

**Target Platform**: Navegadores modernos en desktop/tablet/mobile

**Project Type**: Web SPA frontend en monorepo

**Performance Goals**: Primera navegación autenticada <2 min para usuarios piloto; UX fluida en flujos P1 con caché y refetch controlado

**Constraints**:
- Cumplimiento estricto de TP11 (Entra ID, `(tid, oid)`, `UserId` interno, audience común)
- Sin client secret en frontend
- Sin SSR/Next.js
- Sin Redux salvo necesidad demostrada
- Separación estricta frontend vs bounded contexts backend

**Scale/Scope**:
- Flujos P1: auth/sesión, listas personales/compartidas
- Flujos P2: settlement y ProductCatalog read-only
- Flujos transversales: responsive, accesibilidad básica, estados HTTP, CI frontend

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- **TP8 (tests obligatorios)**: PASS. Estrategia definida con unit/component (Vitest/RTL), integración de red (MSW) y E2E (Playwright) para flujos críticos.
- **TP11 (identidad/auth transversal)**: PASS. Se mantiene Entra ID, MSAL + PKCE, `(tid, oid)` y audience backend común como decisiones cerradas; `UserId` interno como identidad de dominio.
- **Bounded-context boundaries**: PASS. El frontend consume contratos HTTP públicos; no hay acceso directo a BDs ni lógica interna de `Users`.

**Resultado gate pre-Phase 0**: PASS.

## Project Structure

### Documentation (this feature)

```text
specs/007-web-react-client/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   └── frontend-backend-integration.md
└── tasks.md
```

### Source Code (repository root)

```text
MiKompri.Web/
├── src/
│   ├── app/
│   │   ├── providers/
│   │   ├── router/
│   │   └── layout/
│   ├── auth/
│   │   ├── msal/
│   │   ├── guards/
│   │   └── session/
│   ├── api/
│   │   ├── client/
│   │   ├── interceptors/
│   │   └── errors/
│   ├── features/
│   │   ├── profile/
│   │   ├── groups/
│   │   ├── shopping-lists/
│   │   ├── shared-lists/
│   │   ├── settlements/
│   │   └── product-catalog/
│   └── shared/
│       ├── ui/
│       ├── forms/
│       ├── hooks/
│       ├── config/
│       └── accessibility/
├── tests/
│   ├── unit/
│   ├── component/
│   ├── integration/
│   └── e2e/
└── .env.*
```

**Structure Decision**:
Se adopta arquitectura por features para minimizar acoplamiento entre dominios UI, facilitar escalado por módulos funcionales y mantener trazabilidad con bounded contexts backend (`Users`, `ShoppingList`, `ProductCatalog`). Se evita una estructura global tipo Controllers/Services/Models y se preserva reutilización conceptual futura para MAUI/iOS.

## Phase 0: Research

1. Confirmar estrategia de autenticación SPA con MSAL React/MSAL Browser + Authorization Code + PKCE y session restore segura.
2. Definir patrón de API client común con propagación Bearer token, manejo de expiración y estrategia de retries para 503.
3. Definir patrón de TanStack Query (query keys por feature, invalidación, staleTime, refetchOnWindowFocus).
4. Definir patrón de formularios con React Hook Form + Zod (errores de validación 400/409 y mensajes UX).
5. Definir arquitectura de errores HTTP centralizados (400/401/403/404/409/503) y estados de pantalla.
6. Definir estrategia de configuración por ambientes (`.env.local`, `.env.test`, `.env.production`) sin secretos en bundle.
7. Definir guía responsive (mobile-first) y criterios de accesibilidad verificables.
8. Definir estrategia CI frontend y despliegue futuro SWA con separación de responsabilidades backend.

## Phase 1: Design & Contracts

### Diseño técnico

- **Auth + Protected Routes**: guardas de ruta y bootstrap de sesión con MSAL; fallback para 401/403.
- **API client común**: capa única para HTTP con inyección de token, correlación de errores y normalización de respuestas.
- **Server state**: TanStack Query como fuente de verdad para datos remotos; sin Redux inicial.
- **Formularios**: React Hook Form + Zod por feature; mapeo de errores backend a errores de campo/globales.
- **UI/UX transversal**: Fluent UI theme central, componentes compartidos de loading/empty/error/unauthorized, responsive y accesibilidad.
- **Separación BCs**: módulos de features mapeados a contratos de `Users`, `ShoppingList`, `ProductCatalog` sin mezclar reglas internas backend.

### Cambios backend prerequisite (bloqueantes)

1. `Users`: migración de correlación de identidad `sub -> (tid, oid)` + soporte transicional `ExternalUserId` nullable.
2. `ShoppingList`: autorización efectiva de listas personales por identidad autenticada; `OwnerId` derivado del usuario autenticado.
3. `ProductCatalog`: autenticación JWT real alineada a audience común y validaciones TP11.
4. Plataforma: configuración CORS para origen SPA local/cloud y headers Authorization.

### CI/CD frontend

- Workflow dedicado de frontend: install, lint/typecheck, unit/component/integration tests, build, `docker build` (cumplimiento TP4) y publish artifacts.
- E2E Playwright en stage de validación (smoke en PR y suite completa en main/release).

### Estrategia Azure Static Web Apps (futuro)

- Objetivo preferente: Azure Static Web Apps si se mantiene compatibilidad técnica final.
- Despliegue recomendado: `azd` (IaC + reproducibilidad), no obligatorio para MVP local.
- Configuración prevista: `navigationFallback` SPA, variables por ambiente, redirect URIs local/cloud y cero secretos en frontend.

### Re-check Constitución (post-design)

- TP8: PASS (matriz de pruebas definida por niveles).
- TP11: PASS (decisiones de identidad cerradas y no reabiertas).
- Boundaries BC: PASS (contratos HTTP, sin acceso directo a datos entre contextos).

**Resultado gate post-Phase 1**: PASS.

## Complexity Tracking

Sin violaciones constitucionales previstas.
