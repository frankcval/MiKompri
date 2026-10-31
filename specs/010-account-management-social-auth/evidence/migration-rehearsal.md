# Ensayo Microsoft→External ID — DEFERRED

**Decisión aprobada31/10/2026:** DEFERRED — Microsoft External ID migration. A-AC06/A-AC07 y T019/T048 siguen abiertos y no bloquean beta010-A. El resultado real se conserva **INCONCLUSIVE por colisión CIAM**, sin convertirlo en PASS ni FAIL.

No ejecutar nueva federación, preparación de destino, reserva, cutover/rollback, manifiestos o retiro legacy en esta entrega. Microsoft actual conserva issuer/app/configuración/usuarios/UserId; Google usa External ID. [Runbook futuro](../migration.md), [alcance beta](010-a-scope-review.md), [ADR-004](../../../docs/adr/ADR-004-ciam-identity.md).

Se conservan tests, runner, journal/reservation e infraestructura técnica; preparación de un objeto CIAM no acredita login ni migración. Futura iteración debe resolver la colisión por vía segura, probar origen inmutable y preservar exactamente UserId/referencias/rollback, nunca email matching. Recursos QA inventariados mantienen revisión/retiro previsto06/11, sin cambios cloud por esta decisión.

## Evidencia histórica íntegra

<details>
<summary>Intentos y límites previos al cambio de alcance: no son gates beta</summary>

# Ensayo de traslado Microsoft — estado 31/10/2026

**PARTIAL; sin cutover real.** [Diseño aprobado](../migration.md), [recursos](010a-qa-resource-plan.md), [validación técnica](validation.md).

El servicio/journal y runner administrativo están implementados. Tests PostgreSQL reales prueban reserva única, prohibición de provisión de candidato reservado, origen/destino ocupados, corte/versiones, cutover/rollback sobre la misma fila/UserId y conservación de grupo/membresía/perfil. El verificador externo se sustituye en esas pruebas; siete pruebas del verificador Graph comprueban respuestas controladas de organización/identidad/issuer/cuenta habilitada. No sustituyen federación real ni un manifiesto completo de negocio.

En QA se preparó una federación Microsoft exclusiva y limitada al piloto, sin modificar registros legacy. No se obtuvo una identidad CIAM destino cuya relación inmutable con el origen quedase acreditada. Por eso no se ejecutó reserva/aplicación/rollback real ni se redirige la Web Microsoft a ese proveedor. No se utilizó email, Google o nombre para reparar la equivalencia. Microsoft continúa por su configuración original.

El30/10 se crearon fixtures de grupo/membresía/listas/producto/compra/gasto por APIs QA para preparar un manifiesto; no hubo traslado. Su digest no prueba conservación y sus datos tmpfs se perdieron al reiniciar. El31/10 se inicia QA persistente con únicamente la tupla/UserId Microsoft copiada desde lectura del origen; no asumir continuidad del fixture anterior ni historial completo.

Pendiente T019/T048/A-AC06/07: acreditar issuerAssignedId/origen real, reservar candidato, obtener manifiesto de referencias antes/después, ejecutar cutover con alta pausada, validar token CIAM/UserId, nuevas compras, rollback sin pérdida y rechazo de fuentes retiradas. Mantener legacy hasta acreditarlo. El fallo de login Google por domain_hint se corrigió independientemente; no demuestra insuficiencia del modelo de transición ni autoriza Alternative B.

No nuevas transiciones CA/G3, licencias/trials, SMS, tenant o credenciales para superar este pendiente. Revisar los recursos QA retenidos antes de2026-11-06 según inventario privado. No se cierra migración por coexistencia ni por tests con verificador sustituido.

## Revisión del candidato a9ca37d — 31/10/2026

**Migración y rollback Microsoft reales: INCONCLUSIVE.** Tras revisar el proveedor OIDC existente y refrescar su asociación al flujo QA, el recorrido llega al tenant Microsoft origen correcto. El titular aprueba el desafío habitual; CIAM devuelve «La cuenta ya existe». No se pulsa Siguiente para adoptar la cuenta de correo anterior: un acceso por email no demostraría el origen Microsoft.

Antes de exponer ese piloto se pausaron altas únicamente en Users QA. Se preparó exactamente un nuevo objeto federado CIAM QA según el [ejemplo oficial de preprovisioning](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-entra-id-federation-customers): issuer del dominio inicial del origen + namespace del tenant externo; issuerAssignedId=oid Microsoft original. Organización CIAM y tupla exacta verificadas antes/después. Sin email, password, métodos, roles, invitación B2B ni vinculación al objeto mail. El inventario privado conserva el destino como PreparedNotAuthenticated para revisión/retirada antes del06/11. No modificar por ensayo objetos mail/Google ni adivinar otra correlación.

El segundo acceso real, nuevamente aprobado por el titular, vuelve a la colisión y no acredita que el token corresponda al destino preparado. No se ejecutan Reserve/Apply/Rollback de MiKompri, ni INSERT de User sustituto, ni cambio de identidad/ownership. La causa exacta de resolución/colisión interna de CIAM no está acreditada; no se atribuye a licencia, email matching necesario ni insuficiencia de la arquitectura. La preparación Graph no equivale a login.

**Restauración verificada tras recargar portal:** Microsoft010AQA no seleccionado; Google seleccionado; OTP anterior seleccionado. Después se restaura ProvisioningPaused=false y health200 en Users QA. Microsoft Web continúa por legacy. El helper inicial agotó su espera de arranque en frío; la lectura independiente posterior confirma health200 y la configuraciónfalse, sin confundir timeout del helper con fallo del servicio. Sin cambios CA/G3/defaults, licencias, métodos MFA ni credenciales nuevas en esta continuación.

PostgreSQL7/7 (verificador externo sustituido): reserva repetida idempotente; cambios de destino/aprobación rechazados; fallo de Graph previo a apply conserva origen y reserva y permite retry; rollback preserva nuevo perfil/grupo/membresía/suspensión y es idempotente, además de las cuatro regresiones previas. No se acreditan con ellos compras/gastos/listas reales tras traslado. T019/T048 y A-AC06/07 permanecen abiertos. Próximo paso indispensable: resolver el login del destino federado QA sin adoptar por email; solo entonces reservar, ejecutar y comparar manifiestos completos antes/después/rollback. Sin cutover productivo.

</details>
