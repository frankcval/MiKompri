# G3 — Revisión de alternativas de prueba reciente

Fecha: 2026-10-30. Rama010, base `b02947f` más spike local. **Research y propuesta de ensayo; ninguna política, contexto, factor, app ni credencial creada/modificada en esta revisión.** No implementación. [Resultado real anterior](assurance-spike.md): Google funciona para login, pero la ruta ensayada es NOT RELIABLE FOR G3. Email OTP sigue INCONCLUSIVE. No se cambian D3, UserId, TP11 ni A/B.

Continuación posterior autorizada y separada de esta revisión: [preparación/ensayo QA](g3-email-mfa-qa.md). Hay contexto/políticas propios deshabilitados y negativo real de c1 sin política efectiva; la transición de protección se revierte tras fallar activación. No es PASS del desafío email MFA social ni ratificación de este candidato.

## Fuentes oficiales revalidadas y discrepancias

| Fuente | Capacidad/límite documental; no resultado QA |
|---|---|
| [MFA en external tenants](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-multifactor-authentication-customers), actualizada2026-06-17 | Describe step-up con authentication context. Para proveedores externos enumera email OTP o SMS como segundo factor. Para email OTP primario solo enumera SMS; no passkey |
| [Configuración MFA](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-multifactor-authentication-customers), actualizada2026-05-28 | Condiciona email MFA a configuración local email/password; documenta SMS para social/OTP. No demuestra que Google+email MFA funcione manteniendo el flujo actual con OTP primario |
| [Matriz de external tenants](https://learn.microsoft.com/en-us/entra/external-id/customers/concept-supported-features-customers) | Enumera authentication context, sign-in frequency y sesión persistente. Authentication context no equivale a authentication strengths |
| [Integración de authentication context](https://learn.microsoft.com/en-us/entra/identity-platform/developer-guide-conditional-access-authentication-context) | Aplica expresamente a external tenants; claims challenge y `acrs` permiten exigir contexto. Puede emitir el contexto solicitado si no tiene políticas asignadas; también puede satisfacer requisitos previos sin reto nuevo |
| [Gestión de sesiones](https://learn.microsoft.com/en-us/entra/identity/conditional-access/howto-conditional-access-session-lifetime) | Every time tolera MFA completada en los cinco minutos anteriores. No garantiza un reto por operación ni acredita cuándo ocurrió el segundo factor |
| [Passkeys CIAM](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-sign-in-with-passkey) | Registro limitado actualmente a cuentas locales con password, dominio personalizado y MFA previo. No social/OTP; no enforcement de authentication strengths. No ampliar el MVP a password/passkeys para sortear G3 |
| [Google Security Bundle](https://developers.google.com/identity/siwg/security-bundle) | Diferencia sesión Google y de aplicación, no ofrece solicitudes de reauth Google. Claims avanzados requieren publicación/verificación; no publicar QA para desbloquearlos |
| [Pricing CIAM](https://azure.microsoft.com/en-us/pricing/details/microsoft-entra-external-id/) | Core incluye hasta50.000MAU gratuitos; SMS es add-on por intento. La página consultada no devuelve importes numéricos por país. La guía genérica de contexto cita P1: comprobar entitlement efectivo CIAM antes de presupuestar, sin asumir licencias workforce por consumidor ni coste cero total |

**Discrepancia conservada:** la guía conceptual permite email MFA a IdP externo, mientras el how-to condiciona el método a email/password local. Ninguna demuestra el caso exacto Google+flujo OTP de QA. No declarar que social solo admite SMS ni que email MFA ya funciona. Primero verificar soporte efectivo sin sustituir el login OTP existente.

## Comparación y recomendación

| Alternativa | Valor | Riesgo/decisión |
|---|---|---|
| Reauth Google con prompt/max_age/selector | Aprovecha login actual | Descartada como prueba G3 por el ensayo real; no insistir en forzar logout Google ni usar una captura de contraseña como evidencia |
| Contexto CA + desafío email OTP gestionado por CIAM | Evita teléfono y SMS; probaría control del objeto CIAM, no una contraseña Google nueva | **Primer candidato de ensayo**, INCONCLUSIVE: comprobar discrepancia de configuración, vinculación del factor y frescura verificable |
| Contexto CA + SMS gestionado por CIAM | Documentado para social y OTP primario | Reserva por revisar si email no aplica; requiere finalidad, enrolamiento seguro, presupuesto/regiones y disponibilidad. Sin SMS activado ni teléfono solicitado |
| Passkey CIAM o cambiar a email/password | Posible endurecimiento para cuentas compatibles | No resuelve la combinación beta Google/OTP actual sin ampliar alcance; diferir |
| B o autenticación/OTP propios | B puede resolver agregación, no frescura por sí misma | No implementar B ni un autenticador propio para tapar este problema |

Recomendar **ensayo acotado de contexto CA con factor CIAM**, empezando por verificar email OTP para Google. No recomendar aún esta ruta para producción ni activar MFA en todo el tenant. Un factor MFA no cuenta como método primario superviviente de unlink ni vincula automáticamente un login email al mismo objeto.

## Garantía exigida al broker propuesto

El callback conserva Code+PKCE, issuer/audience propios, nonce/state y binding a sesión, propósito, objeto CIAM esperado, operación y versiones. Exigir contexto configurado por entorno **y** política protectora habilitada/validada. Nunca interpretar un `c1` universal, `acrs` aislado o report-only como prueba. Configuración ausente/incierta/cambiada deshabilita el recorrido; no Graph por request ni nuevo panel de políticas.

Antes de aceptar `EvidenceKind`, demostrar una evidencia CIAM autenticada que identifique control efectivo del objeto y tiempo del reto satisfactorio. `auth_time` solo si el ensayo acredita esa semántica para el factor; no asumir que fecha de primer factor es fecha MFA. `amr=mfa`, token nuevo, contexto satisfecho o sign-in log no constituyen por sí solos el recibo. Logs diagnósticos pueden corroborar ensayos, sin convertir Graph/log polling en autorización de negocio.

Se mantienen `AuthenticatedAt >= ChallengeStartedAt-30s`, edad<=300s **al confirmar**, operación<=600s y recibo de un solo uso. La tolerancia CA de cinco minutos no amplía estos límites ni permite sustituir AuthenticatedAt por callback/iat. Si reutiliza MFA anterior al reto o no expone evidencia suficiente, negar G3 aunque el contexto esté presente. Resolver esa incompatibilidad mediante arquitectura; no relajar D3 silenciosamente.

El factor debe estar inscrito/vinculado con control acreditado. Una sesión Google robada no puede registrar el teléfono/email del atacante como factor y validarse a sí misma. El primer enrolamiento y cambio/recuperación de factores son parte del gate. Sin prueba de bootstrap seguro, mantener bloqueadas las mutaciones Google-only; no recuperar por coincidencia de email.

## Ensayo siguiente propuesto, no ejecutado

1. Verificar en QA soporte/entitlement y configuración efectiva para Google+OTP actual. Si exige cambiar el primer factor a password, detener esa rama y registrar el límite; no modificar consumidores existentes.
2. Tras autorizar configuración, planificar como máximo un contexto y una política QA dirigidos a ese contexto, con exclusión administrativa y retiro documentado. Reutilizar harness/tenant; adaptar solo instrumentación aislada si hace falta. No all-resources, apps productivas, SMS o nuevas credenciales por defecto.
3. Google con cookie antigua y sin MFA; luego MFA previa de0/240/301s. Registrar si hay reto nuevo y qué tiempo/factor/contexto verificables devuelve CIAM. Probar la interacción con max_age/login_hint y expiry, sin aceptar skew como ampliación de vida.
4. Negativos: contexto sin política, política deshabilitada/report-only, claim ausente, SSO/silent refresh, otro objeto, nonce/state/operación/sesión ajena, callback repetido; deben negar prueba. No deshabilitar políticas ajenas: usar solo el contexto QA creado para el ensayo.
5. Confirmar a300/301s, terminar a600/601s, repetir solicitud durante tolerancia MFA; verificar mismo oid, ningún login email implícitamente añadido y bootstrap del factor. Después evaluar linking; no mutar identidades reales sin doble prueba.
6. **PASS** solo con evidencia verificable y negativos consistentes; **FAIL de ruta** si reutiliza control no admisible o permite bootstrap inseguro; **INCONCLUSIVE** si no puede atribuirse tiempo/factor/configuración. G3 permanece abierto mientras tanto.

El siguiente trabajo concreto es este ensayo QA sujeto a configuración autorizada. Esta revisión no lo ejecuta. Decisión actual: **ALTERNATIVE A REQUIRES CHANGES**.
