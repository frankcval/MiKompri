# 010 — Herramientas de spike aisladas

No se incluyen en la solución ni se referencian desde APIs/Web. No implementan IdentityBinding, linking, access-status ni migraciones. [Evidencia y límites](../../../specs/010-account-management-social-auth/evidence/spike-summary.md).

Prerequisitos: .NET8 runtime/SDK compatible, paquetes actuales, Docker con `postgres:15` ya disponible para benchmark; sesión Azure CLI autorizada **en QA** para Graph. Web node_modules instalado (`@azure/msal-browser`3.30.0 observado). Ningún runner instala SDK, descarga imágenes ni solicita secretos por argumento.

```powershell
dotnet build tools/spikes/010-external-id/ExternalIdSpike.csproj -c Release
dotnet tools/spikes/010-external-id/bin/Release/net8.0/ExternalIdSpike.dll jwt-controls
dotnet tools/spikes/010-external-id/bin/Release/net8.0/ExternalIdSpike.dll g3-timing-controls
pwsh -File tools/spikes/010-external-id/run-benchmark.ps1
pwsh -File tools/spikes/010-external-id/get-qa-inventory.ps1
pwsh -File tools/spikes/010-external-id/get-google-qa-inventory.ps1
pwsh -File tools/spikes/010-external-id/run-graph-probes.ps1
pwsh -File tools/spikes/010-external-id/run-graph-token-negative.ps1
pwsh -File tools/spikes/010-external-id/run-auth-harness.ps1
```

Primero leer el [plan de recursos](../../../specs/010-account-management-social-auth/evidence/spike-resource-plan.md). `get-qa-inventory.ps1` solo lee los nombres QA exactos, comprueba tipo CIAM y guarda IDs públicos en `.local/qa-inventory.json` ignorado; no configura Azure ni lee credenciales. No compartir ese inventario ni caches como evidence. Los otros runners requieren ese archivo si usan CIAM; benchmark/JWT/fechas no requieren Azure.

Los runners de resultados escriben por defecto en `.local/`. `-OutputDirectory` permite publicar solamente JSON saneado revisado. Cada corrida nueva debe identificarse; no mezclar con evidencia anterior sin explicar cambios. No guardar tokens, OTP, emails o nombres en esa carpeta ni en evidence. Fuente/helpers actuales corresponden al HEAD base indicado en el resumen; hashes de fuente permiten identificar el harness sin commit todavía.

Harness HTTP: `http://localhost:55412/`, solo loopback; Ctrl+C termina servidor. Inicio/reauth manejan credenciales en CIAM, mediante MSAL sessionStorage. El servidor no las registra. El resultado incluye solo hashes/timestamps y marca `provesG3=false`: hay que observar challenge real, cookies y negativos según matriz. `idTokenAsApiBearerStatus` prueba audiencia API distinta del cliente. `negative.html` usa otro cliente público consentido solo para diagnostic_only; `scopeObservation` acredita claims mediante backend y API devuelve403. Un token adquirido con cliente principal para scope solicitado distinto puede seguir conteniendo scope requerido; no considerar su aceptación un bypass sin inspeccionar el caso.

Google requiere proyecto OAuth de titular, modo Testing y cliente web exclusivo QA. Nunca colocar JSON de cliente en repo, argumentos ni chat. Destino privado previsto: `%LOCALAPPDATA%/MiKompri/qa-010/google-client.json`, ACL de usuario/SYSTEM. Las URLs QA de la guía oficial actual se preparan en `.local/google-redirects.txt`; no contienen secretos y no acreditan que Google esté configurado. Registro Google y secretos solo se realizan tras login/intervención del titular.

Google ya configurado en esta ejecución: `get-google-qa-inventory.ps1` reconstruye únicamente IDs públicos desde el JSON privado, comprobando hashes contra la evidencia QA. No copia client_secret a `.local/`, proceso Web ni navegador. `google-direct.html` usa Google Identity Services para el **negativo**, requiere origen JavaScript QA localhost:55412 registrado y tester autorizado. La página envía el token en memoria a diagnóstico (firma/aud Google) y a política CIAM (debe401); solo muestra flags/hash. No añade autenticación Google de negocio ni acredita G3.

Limpieza: benchmark elimina exactamente su contenedor etiquetado y tmpfs; probes borran permanentemente solo fixtures que acaban de crear. No quitar objetos ajenos. CIAM/apps/flujo/proyecto Google y cualquier secreto deben retirarse según inventario y procedimiento oficial al acabar QA, no por un borrado recursivo genérico. El proveedor ARM es compartido y no se desregistra automáticamente. No hay limpieza cloud programada.
