# Guía CORS — MiKompri Web Client

## Objetivo
Alinear la SPA `MiKompri.Web` con las APIs `Users`, `ShoppingList` y `ProductCatalog` para evitar bloqueos por CORS en local y cloud.

## Orígenes del frontend

### Local
- `http://localhost:5173`
- `http://127.0.0.1:5173`
- `https://localhost:5173` solo si se habilita TLS y se añade explícitamente a los orígenes; no es un default actual

### Cloud (Azure Static Web Apps)
- `https://<tu-swa>.azurestaticapps.net`
- Dominio custom final (si aplica)

## Headers y métodos requeridos
- Métodos: `GET`, `POST`, `PUT`, `PATCH`, `DELETE`, `OPTIONS`
- Headers permitidos:
  - `Authorization`
  - `Content-Type`
  - `Accept`
  - `X-Client-Utc-Offset-Minutes` para fechas civiles de compra/precio de 009
- Credenciales:
  - No usar cookies cross-site para auth de APIs.
  - Se usa `Bearer` token de Entra ID en header `Authorization`.

## Configuración recomendada en APIs .NET
1. Definir política CORS explícita por ambiente.
2. Permitir solo orígenes conocidos (no `AllowAnyOrigin` en producción).
3. Permitir los headers anteriores, incluido el contexto local de fecha de 009.
4. Aplicar `UseCors(...)` antes de `UseAuthentication()`/`UseAuthorization()`.

## Checklist de verificación
1. Preflight `OPTIONS` responde `200/204` con headers CORS esperados.
2. Requests autenticadas desde SPA retornan datos (sin error CORS).
3. Error real de auth/permiso (401/403) no debe confundirse con CORS.
4. En cloud, validar origen SWA real configurado en cada API.
5. Verificar una escritura de precio/compra con `X-Client-Utc-Offset-Minutes` y su preflight, no solo una consulta GET.

Las tres APIs actuales leen `Cors:AllowedOrigins` como lista separada por comas y usan `WithOrigins(...).AllowAnyMethod().AllowAnyHeader()`. Por defecto permiten los dos orígenes HTTP locales anteriores. Configurar `Cors__AllowedOrigins` por entorno; no dar por configurado un dominio cloud por aparecer en esta guía. El header de fecha no acredita identidad ni cambia permisos. [Contrato 009](../specs/009-shopping-experience-completion/contracts/api.md).

## Troubleshooting rápido
- **Error CORS en navegador**: revisar `Access-Control-Allow-Origin` y `Access-Control-Allow-Headers`.
- **401 en vez de CORS**: revisar token/scope delegado (`VITE_API_SCOPE`) y audience emitida en el access token.
- **Solo falla en cloud**: agregar dominio SWA exacto en política CORS backend.
