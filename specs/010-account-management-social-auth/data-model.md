# Modelo mínimo — 010-A y extensiones010-B

Modelo 010-A implementado en esta rama; sin aplicación a bases originales/productivas. [Auditoría inicial](current-state-audit.md), [migración](migration.md), [ADR](../../docs/adr/ADR-004-ciam-identity.md). La aceptación de traslado real sigue abierta.

Migración aditiva `20261030212332_AccountExperience010A`: nueve columnas Users (Username, GivenName, FamilyName, PreferredLanguage, DefaultListSort, ProfileVersion, IsSuspended, SecurityVersion, TokensValidFromExclusive) y journal IdentityMigrations con FK restrictiva e índices únicos origen/destino. No reescribe UserId/ownership. DefaultListSort admite newest/oldest/name/updated; PreferredLanguage admite es o null. ProfileVersion controla concurrencia; solo el alta inicial importa claims, por lo que no hace falta almacenar snapshots del IdP ni flags de autoridad por campo.

## Invariantes

1. User.Id Guid actual no cambia al entrar, personalizar o trasladar la identidad Microsoft.
2. Una tupla de emisor confiable/namespace+tid+oid resuelve como máximo un UserId; unicidad también bajo carreras.
3. Un User tiene **una canónica activa**. Google/Microsoft son mecanismos del IdP, no un catálogo de subjects en Users.
4. Registro de traslado Microsoft no es una segunda identidad gestionable permanente. Corte/rollback solo por procedimiento aprobado.
5. No correlación por email/perfil/sub global ni transferencia de listas/gastos para arreglar duplicados.
6. Personalización previa prevalece sobre claims; borrado voluntario tampoco se rellena automáticamente.
7. No cascada de User sobre historia compartida; no rediseño de grupos/ownership.

## 010-A: reutilizar primero

| Elemento | Diseño mínimo |
|---|---|
| User existente | Id, IdentityProvider/namespace, TenantId, ObjectId, ExternalUserId legado, DisplayName/Email. Mantener índices canónicos; separar namespace workforce/CIAM desde emisor validado, no Google ID |
| Estado D1 | AccountStatus Active/Suspended, SecurityVersion y TokensValidFromExclusive/corte aplicable a canónica actual. No tabla genérica de bindings solo para suspender una cuenta de una identidad |
| Perfil básico | Username opcional único normalizado, DisplayName requerido, GivenName/FamilyName opcionales, PreferredLanguage y preferencias tipadas aprobadas. ProfileVersion/autoridad de campos para concurrencia y preservación |
| Traslado Microsoft | Registro acotado UserId, source namespace/tid/oid, target namespace/tid/oid, cohorte, estado/versión, referencia de aprobación y digest de equivalencia/inventario. Reserva única de destino y origen retirado; no códigos/tokens/OTP |
| Auditoría | Actor/proceso autorizado, evento/resultado, referencia de cohorte y timestamps. Sin atributos del proveedor ni PII innecesaria |

Las columnas nuevas se justifican con tests, no se añaden automáticamente todas las entidades del diseño anterior. El fallback ExternalUserId/sub solo sirve a registros legacy incompletos en el mismo issuer/tenant/cliente autorizado; jamás adopta un User de otro namespace ni revierte una fuente retirada.

El método actual AssignCanonicalIdentity/SyncClaims debe dejar de poder cambiar libremente una canónica o sobrescribir personalizaciones en cada login. Resolución normal es lectura/alta nueva; traslado es operación administrativa específica y perfil tiene autoridad propia.

## Perfil y autoridad

| Campo | Regla A |
|---|---|
| Username | Opcional, público opt-in, 3–30 caracteres `[a-z][a-z0-9_]{2,29}`, trim/minúsculas/índice único, reservados admin/support/system/mikompri. No login ni email |
| DisplayName | 1–100, fallback «Usuario», sin publicar email como fallback. Inicialización permitida y después edición propia |
| GivenName / FamilyName | Opcionales hasta100 cada uno; no inferir apellido dividiendo DisplayName |
| PreferredLanguage | Opcional, solo idiomas ya soportados; default actual. No proyecto nuevo de internacionalización |
| Preferencias | Solo claves tipadas con funcionalidad actual acordada; defaults existentes. Sin bolsa JSON arbitraria ni claves de seguridad. No inventar preferencias para justificar persistencia |
| Email existente/contacto | Mantener compatibilidad y procedencia mínima; separado del identificador de login. No inferir verified ni usarlo para cambiar identidad |
| Avatar | Iniciales/fallback derivadas del nombre propio. Importar foto, upload/storage/URL/SSRF/EXIF y avatar personalizado diferidos |
| Nacimiento/teléfono/dirección | Fuera de persistencia/recogida A |

Claims permitidos pueden inicializar campos al crear User. En usuarios anteriores asumir perfil personalizado. Login/sync no sobreescribe campos User, incluso null/clear; una futura reimportación explícita es B. La representación de autoridad puede ser mínima por campo, no obliga a copiar todo el perfil externo ni crear snapshots extensos.

## D1 y transición

D1 consulta estado vigente y canónica del token por request antes de negocio, con resultado reutilizable solo dentro de la petición. Al suspender/reactivar/cambiar fuente, cortes y versiones impiden revivir tokens anteriores; iat aquí es corte de emisión, no prueba G3. Sin caché entre requests ni Graph.

Durante coexistencia, distintos usuarios pueden tener canónica legacy o CIAM. El journal de traslado conserva origen para reversión; no permite que SyncClaims reabra una fuente retirada ni autentica dos métodos arbitrarios. No se cambia ningún UserId/FK.

## 010-B: entidades conservadas como diseño futuro

AccountOperation, ProofReceipt, inventario/último método, avatar/storage y estados de eliminación. IdentityBinding/ExternalIdentities[] múltiple solo si se selecciona Alternative B mediante ADR y evidencia; no crear ese esquema para A. Los probes federated/mail y google.com quedan como hechos del CIAM, no nuevas entidades de dominio.

## Persistencia y validación futura

Cambios aditivos en Users, PostgreSQL/Docker y constraints de canónica/username/reserva. Aplicados únicamente en bases QA/tests aisladas; las tres APIs no tienen cambios de modelo EF pendientes. El rollback administrativo conserva schema/journal/perfil, sin Down destructivo. El estado transitorio no se lee directamente desde otros contextos. [Resultados PostgreSQL](evidence/validation.md).
