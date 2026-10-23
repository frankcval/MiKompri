# Guía CORS — MiKompri Web Client

## Objetivo
Alinear la SPA `MiKompri.Web` con las APIs `Users`, `ShoppingList` y `ProductCatalog` para evitar bloqueos por CORS en local y cloud.

## Orígenes del frontend

### Local
- `http://localhost:5173`
- `https://localhost:5173` (si se habilita TLS local)

### Cloud (Azure Static Web Apps)
- `https://<tu-swa>.azurestaticapps.net`
- Dominio custom final (si aplica)

## Headers y métodos requeridos
- Métodos: `GET`, `POST`, `PUT`, `PATCH`, `DELETE`, `OPTIONS`
- Headers permitidos:
  - `Authorization`
  - `Content-Type`
  - `Accept`
- Credenciales:
  - No usar cookies cross-site para auth de APIs.
  - Se usa `Bearer` token de Entra ID en header `Authorization`.

## Configuración recomendada en APIs .NET
1. Definir política CORS explícita por ambiente.
2. Permitir solo orígenes conocidos (no `AllowAnyOrigin` en producción).
3. Permitir `Authorization` y `Content-Type`.
4. Aplicar `UseCors(...)` antes de `UseAuthentication()`/`UseAuthorization()`.

## Checklist de verificación
1. Preflight `OPTIONS` responde `200/204` con headers CORS esperados.
2. Requests autenticadas desde SPA retornan datos (sin error CORS).
3. Error real de auth/permiso (401/403) no debe confundirse con CORS.
4. En cloud, validar origen SWA real configurado en cada API.

## Troubleshooting rápido
- **Error CORS en navegador**: revisar `Access-Control-Allow-Origin` y `Access-Control-Allow-Headers`.
- **401 en vez de CORS**: revisar token/audience (`VITE_API_AUDIENCE`).
- **Solo falla en cloud**: agregar dominio SWA exacto en política CORS backend.
