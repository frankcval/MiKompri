# Revisión arquitectónica acotada — beta 010

## Aplicabilidad actual tras separar 010-A / 010-B

Revisión de alcance del 30/10/2026, [ADR actualizado](../../docs/adr/ADR-004-ciam-identity.md) y [decisión](evidence/010-a-scope-review.md). Se conserva a continuación la revisión original y sus comparaciones; sus gates del alcance completo no se convierten en dependencias de login básico.

| Decisión | Aplicabilidad actual |
| --- | --- |
| D1 estado autoritativo | 010-A antes de acceso productivo protegido; no bloquea primer login aislado. Una canónica activa/estado de cuenta, no colección genérica de bindings |
| D2 cliente | 010-A Web: MSAL.js + Code/PKCE browser-delegated. MAUI/Native Auth, versión/SDK/retorno se difieren |
| D3 / G3 | 010-B para gestión sensible de métodos/recuperación alternativa; no se implementan ni ofrecen en A. Límites300/600s y negativos se conservan, sin aceptación |
| Migración Microsoft | 010-A, traslado administrado del mismo origen federado acreditado y reservado; mismo UserId y rollback. No es linking autoservicio de proveedores arbitrarios ni requiere broker G3 genérico |
| G1 agregación y alternativa B | Futuro 010-B. No asumir linking resuelto ni introducir múltiples identidades para login básico |

El token c1 con política deshabilitada no prueba MFA; el candidato de desafío social queda INCONCLUSIVE, con restauración QA verificada. No repetir transiciones CA para implementar Microsoft+Google. La recuperación A se delega al proveedor original; no hay unlink, segundo método ni sustitución de identidad. Email OTP básico opcional tiene QA propio sin G3.

## Revisión original y comparaciones conservadas


Fecha **30/10/2026**, sobre `94d64eb`, rama `010-account-management-social-auth`. Solo decisiones propuestas sobre revocación, clientes y autenticación reciente; no cambia la elección provisional A/B ni autoriza implementación. Leídos artefactos 010, TP11 y Users/JWT/MSAL actuales. Fuentes revalidadas en [research](research.md), R20–R28. Las pruebas QA que se describen **no se han ejecutado**.

## D1 — Suspensión y access token ya emitido

### Garantía recomendada: opción 1 para beta

Suspensión MiKompri **prácticamente inmediata en el siguiente control de acceso autoritativo después de su commit en Users**. Un JWT firmado, no expirado y emitido antes sigue siendo criptográficamente válido; no basta para acceder. Users comprueba AccountStatus y el binding exacto del token, y las tres APIs deben aplicar el resultado antes del handler de negocio, también en GET.

«Inmediata» no significa deshacer respuestas/datos ya descargados ni cancelar una petición autorizada antes del commit; existe esa carrera. No es una garantía sobre conexiones persistentes/jobs futuros. Las operaciones sensibles de cuenta revalidan estado/versiones además en su confirmación transaccional. Una suspensión a confirmar debe escribirse en el almacén autoritativo antes de reportarse como completada.

### Comparación de opciones

| Opción / mecanismo | Garantía y coste | Decisión |
| --- | --- | --- |
| 1: Users en cada request autenticada | Lectura vigente tras commit, sin ventana TTL. Dependencia de Users/BD y salto de red adicional desde consumidores | **Beta recomendada**; lecturas simples y reutilización dentro de la petición |
| 2: solo expirar access token | Uso permitido hasta exp, incluso después de suspender localmente; un nuevo token puede seguir apareciendo si no hay bloqueo local/CIAM | No cumple D1; no elegir silenciosamente como implementación de FR-023 |
| 3: caché status/securityVersion, p.ej. 30s para lecturas y consulta vigente para escrituras | Mejora latencia/disponibilidad; una lectura puede autorizar hasta 30s tras suspensión. Invalidation distribuida puede fallar | Opción futura si se aprueba **otra garantía** y tests de TTL absoluto. No necesaria para la beta |
| Version/status en claim del token | Snapshot firmado no cambia tras suspensión. Compararlo con Users requiere lookup/invalidación igualmente | No claims dinámicos nuevos como fundamento de revocación beta |
| Access tokens cortos | Reducen exposición, no revocan inmediatamente; aumentan refresh/dependencia. Soporte de lifetime configurable específico del tenant no acreditado | Complemento futuro; no cambiar lifetime ni asumir que SIF acorta un token ya emitido |
| revokeSignInSessions / CAE | Refresh/session revocation no equivale a invalidar JWT ya emitido en una API propia. CAE requiere soporte emisor/recurso/cliente y señal aplicable | No usar como garantía beta ni extrapolar Microsoft 365/B2B a External ID/API propia |

Las páginas CAE generales no prueban entrega de eventos a nuestras APIs ni reflejan una suspensión que solo vive en Users. La documentación de session controls para external tenants enumera SIF y persistent browser session; no se declara «CAE imposible para siempre», sino **no acreditado para este escenario**. Tampoco `xms_cc` o MSAL por sí solos convierten la API en recurso CAE. [Session controls](https://learn.microsoft.com/en-us/entra/identity/conditional-access/concept-conditional-access-session), [CAE](https://learn.microsoft.com/en-us/entra/identity/conditional-access/concept-continuous-access-evaluation).

### Protocolo beta concreto

1. Validar JWT/issuer/audience/scope antes de la resolución. La clave de lookup viene del emisor y tid/oid validados, no de un UserId de cliente.
2. Users consulta su estado autoritativo local en cada request protegida. ShoppingList/ProductCatalog consultan un contrato mínimo Users `/api/v1/users/me/access-status` con el bearer de audiencia común. No usar `/me` actual como sustituto sin separar sus efectos de aprovisionamiento/sync. El endpoint de estado no crea usuarios ni llama a otra API ni a Graph.
3. Resultado mínimo: UserId, cuenta/binding activos, securityVersion/versión del binding, corte de tokens. No PII ni inventario de métodos. Se puede reutilizar en el **scope de esa petición**, no entre requests ni como caché compartida. Leer del almacén primario sin réplica con retraso ni caché ORM de otra petición.
4. Suspendido/revocado/no autorizado: **403** genérico; JWT inválido/expirado o anterior al corte de tokens: **401**. Cuenta no resuelta: denegar negocio, sin auto-provisioning. Alta intencional y callbacks aislados siguen sus propios contratos de prueba, nunca dan acceso a recursos privados por saltarse esta consulta.
5. Timeout/error Users o lectura de BD incierta: **503 fail-closed** en endpoints protegidos, sin servir un allow antiguo. Deadline de dependencia configurable (propuesta beta: 1s máximo, no SLO de latencia), sin retries automáticos en cadena ni replay de escrituras. Se preservan los presupuestos p95/performance existentes: medirlos es gate antes de habilitar.
6. Health/liveness y documentación pública configurada expresamente no consultan estado de cuenta; una dependencia caída no debe fingir que el proceso está muerto. No convertir endpoints protegidos de catálogo en públicos como escape.

### Qué cambia por API

- **Users:** lectura local de estado previa al perfil/negocio, sin loop HTTP a sí misma. No incrementar SecurityVersion por cada GET ni modificar perfil al consultar estado.
- **ShoppingList:** ya resuelve identidad por Users; reutilizar UserId/status dentro de request para evitar llamadas idénticas, sin sustituir sus comprobaciones de membresía/rol ni cachearlas para otra petición.
- **ProductCatalog:** hoy protege controllers con JWT y no resuelve UserId para cada operación. Incorporar la consulta es un **nuevo acoplamiento operativo** por contrato público: añade UsersApi/configuración/red/latencia y hace indisponible su API protegida si Users falla. Es control de cuenta, no permiso de grupo ni cambio del catálogo global. Probar incluso GET producto/mercado/historial.

Beta no añade Redis, bus de eventos, revocation list distribuida, introspección Google ni llamada Graph por request. Medir SQL/RPC y comparar antes/después; si latencia/disponibilidad no son aceptables, revisar explícitamente D1 frente al modelo híbrido en vez de añadir caché escondida. No se garantiza bloqueo de un disable ocurrido solo en Azure antes de que Users lo conozca; esa sincronización es otro mecanismo y no está acreditada aquí.

Un 401 por corte de emisión debe llevar a adquisición posterior/reauth explícita con la opción soportada por MSAL, sin bucle de acquireTokenSilent que devuelve el mismo token cacheado. Un 403 por suspensión no se arregla refrescando; 503 no concede acceso. No reenviar automáticamente una escritura financiera tras recuperar sesión ni prometer borrar datos que ya se descargaron.

### Versiones y tokens anteriores

SecurityVersion en Users invalida ProofReceipts/operaciones al cambiar estado o métodos críticos; **no se presume un claim equivalente emitido por CIAM**. Para evitar que un token anterior vuelva a funcionar al reactivar, User y Binding mantienen un corte server-side `TokensValidFromExclusive` (segundos UTC del emisor validados). Suspender/revocar y reactivar actualizan el corte aplicable; admitir solo `iat > max(cortes aplicables)` y JWT vigente. Si falta iat y se requiere corte, no hay fallback permisivo.

Aquí iat sirve exclusivamente para excluir tokens emitidos antes de un corte, **nunca para demostrar autenticación reciente**. Comparación estricta a precisión de segundos: token del mismo segundo puede requerir una adquisición posterior; el skew tolerado para exp/nbf no relaja el corte. Probar precisión de reloj y no resucitar tokens emitidos durante suspensión. Revocar binding B no bloquea otro binding activo C del mismo UserId salvo suspensión de cuenta. En A, el binding del objeto CIAM no identifica qué método social se usó: deshabilitar un método no garantiza poder distinguir tokens de Google vs email del mismo oid. Para un incidente de método comprometido, bloquear el binding completo y aplicar recovery/reauth aprobados; no prometer revocación selectiva por un claim `idp` opcional.

## D2 — Browser-delegated y Native Authentication

| Plataforma / biblioteca | Propuesta beta y límite |
| --- | --- |
| **Web React, MSAL.js browser/react** | **Authorization Code + PKCE + MSAL + browser-delegated**, redirect al CIAM y vuelta al callback exacto; hosted sign-in lleva a Google. No formularios MiKompri de passwords/OTP ni Native Auth JS SDK en esta entrega |
| **Web con Native Auth JavaScript SDK** | La documentación sí contiene un flujo social con popup/browser; no es «sin navegador». Posible opción de producto futura, sin necesidad para beta y con mayor superficie propia |
| **MAUI Android futuro, MSAL.NET** | Candidato: public client, Code+PKCE y navegador del sistema con redirect/deep link verificado. No fijar Native Authentication ni implementar un wrapper HTTP nativo antes de demostrar versión, SDK y retorno soportados |
| **Native Authentication SDK Android (Kotlin/Java)** | Tiene tutorial social con tramo web. No equivale a soporte de ese mismo SDK desde C#/MAUI, ni a que todo MFA/post-login sea nativo |

Microsoft publica una contradicción: [métodos](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-authentication-methods-customers) y [planificación](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-planning-your-solution) mantienen restricciones a federación/native, mientras [comparativa específica](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-choose-authentication-approach) y tutoriales JS/Android admiten social vía web. Se registra sin afirmar que todos los SDKs tengan las mismas capacidades. Web-view dentro de un flujo no significa MAUI Native Auth SDK; navegador del sistema y WebView embebida tampoco son equivalentes. Passkey no funciona necesariamente en WebViews. Registro QA, SDKs exactos, redirects, sesión social, cancelación y fallback deben verificarse después de autorización, sin crear recursos ahora.

## D3 — Control reciente y step-up para beta

### Qué se exige y qué no se infiere

**Autenticación reciente** y **MFA/step-up** son propiedades distintas. La beta exige prueba de control reciente por cada parte pertinente a la operación, con evidencia semántica del método; no promete MFA universal ni resistencia a phishing para email OTP. Una sesión actual, un access token nuevo/iat, prompt=login solicitado, selector de cuenta o consentimiento Google no prueban por sí mismos un nuevo reto de credencial.

Ruta preferente para el broker: intercambio Code+PKCE con nonce/state, solicitar reautenticación mediante parámetros admitidos (`max_age=0`/auth_time solicitado cuando se soporte) y **verificar** un ID token del emisor, audience del broker, identidad esperada y `auth_time` firmado. OIDC define auth_time/max_age, pero no se acredita aquí su emisión/semántica exacta en cada user flow External ID. Tampoco se presupone propagación de max_age/prompt hacia Google.

Habilitar un recorrido sensible solo después de G3: demostrar que el timestamp corresponde a autenticación activa/challenge del método, no renovación o nuevo SSO CIAM con cookie upstream antigua. Si existe otra respuesta CIAM autenticada de challenge con identificador/tiempo/resultado verificables server-side, puede aprobarse como evidencia equivalente; **no** hora de callback, evento JavaScript, captura DOM, booleano de UI o email recibido. No se cambia a Native Auth solo para conseguir esa evidencia.

Cada ProofReceipt exige `now - AuthenticatedAt <=300s`, tiempo no futuro fuera de skew técnico acotado, y `AuthenticatedAt >= ChallengeStartedAt -30s` para el reto de esa transacción ligado a nonce/desafío. TTL operación **600s** es máximo para terminar el recorrido, no permiso para reutilizar prueba de 10min: ambas partes se vuelven a comprobar al confirmar. Propuesta de skew de reloj: 30s solo para coherencia con el emisor, sin ampliar edad máxima de 300s. Comprobar timestamp no sustituye G3 sobre reto/SSO. Pruebas de otra operación y recibos ya consumidos se rechazan.

### Métodos: evidencia candidata y limitación

| Método | Evidencia admisible tras G3 | No asumir |
| --- | --- | --- |
| Email OTP primario | Nuevo desafío CIAM a ese acceso y resultado firmado/verificable ligado a operación; hash/recibo mínimo, no guardar OTP | No equivale a segundo factor ni demuestra independencia del canal email. No enviar un OTP propio para simular CIAM |
| Email/password | Nueva autenticación CIAM con tiempo/método verificables; SSPR es flujo distinto | Un refresh o sesión SSO anterior no es nueva contraseña acreditada |
| Google | Nueva prueba a través de CIAM, identidad candidata y tiempo de control acreditados; si usa reto CIAM adicional, registrar que se probó control de objeto CIAM | auth_time CIAM no prueba automáticamente que Google pidió password/passkey otra vez. `select_account`/`consent` no bastan; no introducir validación Google directa en APIs |
| MFA CIAM | Reto actual/verificable y método/assurance admitidos; claims firmados solo con semántica comprobada | Tener MFA registrado, `amr=mfa` viejo o sesión con MFA histórica no acredita frescura actual |
| Passkey CIAM | Solo si la combinación de cuenta/SDK/flujo está soportada y probada; reto reciente y user verification acreditados | Documentación actual limita passkeys a cuentas locales email+password, no OTP/social; no usarlas como solución universal para Google-only/OTP ni asumir native auth/embedded WebView |

La guía actual de passkeys indica que external tenants no permiten exigir authentication strengths en Conditional Access. Esto **no excluye authentication context**: la [guía MFA para CIAM](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-multifactor-authentication-customers) y la matriz de capacidades sí documentan contexto/step-up. El contexto solicitado puede emitirse sin una política protectora y no acredita por sí solo el instante de MFA. Los claims `amr`/`acr`/`acrs` requieren semántica/configuración comprobadas. [Límites de passkeys](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-sign-in-with-passkey), [revisión y ensayo candidato G3](evidence/g3-step-up-review.md).

### Matriz por operación: evita exigir una prueba imposible

| Operación | Control reciente requerido en beta |
| --- | --- |
| Vincular Google/otro acceso | Origen asociado a UserId + candidato, ambos probados en esa operación; confirmar el resultado por identidad validada, no email |
| Añadir email de **login** | Origen + prueba CIAM del email candidato. Verificación de ContactEmail no basta ni se cuenta como MFA |
| Migrar identidad | Legado + destino CIAM; conservar UserId y journal. Falta de origen no se salta por email; pasa a recovery aprobado |
| Desvincular / cambiar método crítico | Prueba reciente con un método **que permanecerá válido**, del mismo UserId, consentimiento y último método/versiones/inventario revalidados. Si hay otro destino nuevo, demostrarlo también. No obligar a reautenticar con el proveedor perdido/comprometido que se retira |
| Recovery con alternativa | Método alternativo **ya vinculado** con prueba reciente + nueva identidad si va a añadirse/reemplazarse; preservar UserId. No fingir doble prueba de un proveedor perdido |
| Sin ningún método de control | No recovery autoservicio beta. No vincular por email/perfil; soporte sin protocolo aprobado tampoco puede forzar el cambio |

Consentimiento/último método/versiones son controles adicionales, no una segunda identidad inventada. Reto de un factor y prueba de dos identidades no equivalen automáticamente a MFA. El medio de login inicial puede seguir siendo Google/email OTP; **no habilitar sus mutaciones sensibles** si no se logra una prueba reciente fiable. Esto es una limitación explícita de la beta: G3 bloquea esos casos y no se puede cerrar AC-05/07/12 o la feature completa sustituyéndolos por un botón, una cookie o un test simulado.

### Endurecimiento posterior, sin construirlo ahora

MFA/reauth por contexto de riesgo, prueba resistente a phishing donde esté soportada, mejor recuperación asistida y alertas/controles de sesión, Native Auth si aporta valor y SDK real, posible caché acotada de estado con garantía revisada. CAE solo tras acreditar escenario completo. Nada de esto se marca implementado ni se usa como premisa de seguridad beta.

## Pruebas de decisión pendientes

- **AR-01**: emitir token → suspender cuenta/binding → siguiente control en **cada** API (GET incluido) rechaza; otro binding activo conserva acceso si solo se suspendió uno.
- **AR-02**: Users/BD caídos →503, sin allow cacheado; petición autorizada antes de suspensión documentada; reactivar no revive tokens anteriores, incluidos tokens del mismo segundo/durante suspensión.
- **AR-03**: ProductCatalog añade dependencia sin cambiar su política global; medir p95/llamadas/fallos y respetar presupuestos. Sin Graph en ruta de datos.
- **AR-04**: perfil Web browser-delegated y matriz SDK/MAUI futura sin afirmar paridad; registrar discrepancia de fuentes y prueba exacta cuando corresponda.
- **AR-05**: G3 por método con SSO previo, token renovado/iat nuevo, auth_time ausente/viejo/futuro, max_age ignorado, callback/nonce manipulado, ambas pruebas al límite de 300s y confirmación tardía. Ninguno acredita por mera apariencia de login.
- **AR-06**: unlink/recovery con proveedor perdido usa alternativa ya vinculada; sin ella rechaza. OTP primario nunca etiquetado como MFA secundario; passkey no ofrecida a combinación no soportada.

Son pruebas futuras de diseño, no resultados ejecutados. D1–D3 quedan propuestas para aprobar; G1–G7 y ADR/enmienda siguen pendientes, sin nueva autorización de Azure o implementación.

## Validación de esta revisión documental

### Ejecución posterior autorizada: spike QA

El usuario aprueba D1–D3 **para un spike técnico aislado**, sin autorizar implementación completa ni enmienda constitucional. La afirmación de «sin autorización Azure» anterior describe la revisión documental original, no esta autorización posterior. [Plan previo](evidence/spike-resource-plan.md), [resultados y pendientes](evidence/spike-summary.md).

Se crea CIAM QA separado, API/SPA sin secretos y flujo email OTP. Harness fuera de producción acredita email/token y controles; D1 solo se mide mediante proxy local, no se implementa access-status. Probes Graph distinguen el OTP real `federated/mail` de `emailAddress`/password: ambas direcciones Google-shaped↔mail-shaped son editables con mismo oid, pero no acreditan Google real ni doble prueba. Graph acepta retirar último método explícito y dos PATCH concurrentes pierden una adición. No delegar estas garantías en el proveedor por existir identities.

G1/G3 siguen condicionando arquitectura final; no se reduce D3 ni se acepta A por los fixtures. La constitución/ADR general y fases3–8 permanecen pendientes de revisión/implementación posterior.

Continuación Google: acceso básico sin Microsoft validado en harness y correlacionado con objeto federated/google.com. Reauth con prompt=login/max_age=0, tras retirar login_hint en diagnóstico, emite auth_time nuevo aunque el titular solo selecciona/consiente. Resultado **NOT RELIABLE FOR G3** de la ruta observada; estudiar desafío CIAM verificable antes de habilitar linking/unlinking/migración/recovery, sin escoger B ni ampliar configuración automáticamente. [Evidencia por método](evidence/assurance-spike.md).

Revisión documental posterior: priorizar ensayo de authentication context CA + desafío CIAM; email OTP para Google es primer candidato condicionado por discrepancia oficial de configuración, SMS solo alternativa con finalidad/coste revisados. Every time puede reutilizar MFA de los cinco minutos anteriores: no satisface automáticamente el reto ligado a operación D3. Exigir política efectiva, factor/enrolamiento seguros y tiempo verificable; sin ello negar. [Comparación, límites y matriz QA propuesta](evidence/g3-step-up-review.md). No se configura CA ni se aprueba EvidenceKind o modificación de D3 por esta propuesta.

Prerrequisitos Spec Kit resuelven 010 y la rama Git es la solicitada. Comprobados 18 artefactos Markdown de 010/ADR, 97 enlaces locales/anchors sin errores, 26 FR/ocho SC/doce AC más seis AR, ocho fases y 55 tareas únicas abiertas. `git diff --check` correcto incluyendo el archivo nuevo. Diff de 13 archivos Markdown: no cambia feature activa, constitución/enmienda propuesta, código, configuración MSAL/JWT, workflows ni esquema/migración EF. No se han ejecutado tests funcionales/QA ni configurado tenant, registros, secretos o BD; las matrices anteriores siguen pendientes.
