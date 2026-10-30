# Migración Entra actual → External ID — propuesta 010

**No ejecutada.** Garantía central: `(oldTid, oldOid) → UserId=A` y `(externalTid, externalOid) → UserId=A`. OldOid y externalOid no se presuponen iguales. No copiar/recrear User ni modificar OwnerId, GroupMembership.UserId, AddedBy, PaidBy, PurchasedBy, auditoría o registros de listas/gastos.

## Reconocimiento y prueba

Un usuario existente se reconoce por su **binding legado validado**, no por similitud de perfil. Inicia «Migrar mi acceso» autenticado como A, acredita de nuevo control del método origen y autentica el destino mediante External ID dentro de la misma operación. Users deriva A; cliente no envía un UserId al que apropiarse.

La federación OIDC con el tenant antiguo puede facilitar el acceso a CIAM, pero no prueba automáticamente la equivalencia de ambos objetos. Obtener ambas pruebas desde sus emisores confiables o un atributo de origen cuya integridad se haya acreditado end-to-end. Un claim de correo/nombre/invitación no es ese atributo. No usar sub como clave global. Sin prueba suficiente, dejar Pending/ManualReview y conservar origen.

## Fases operativas futuras

| Fase | Diseño y gate antes de avanzar | Reversión |
| --- | --- | --- |
| M0 Inventario | En QA autorizado: counts/IDs y digests de usuarios, memberships/owners y todas las referencias intercontexto por exportaciones/contratos controlados; detectar null tid/oid, duplicados e índices. Nada de acceso cross-DB en runtime | Sin mutaciones |
| M1 Expand | Schema aditivo Users: bindings, operaciones/journal, versiones/estado, perfil con procedencia. Backfill por clave confiable existente; usuarios legados null quedan pendientes. Snapshot perfil existente se trata personalizado | Código anterior compatible; conservar columnas y registros nuevos |
| M2 Trust preparada | Configuración aislada de emisores y tuple issuer→tenant→audience→JWKS→scope→clientes. Probar tres APIs y forwarding ShoppingList→Users; no confiar en cualquier tenant ni tokens Google | Deshabilitar nueva entrada manteniendo origen |
| M3 Pilot autorizado | Cohorte pequeña; transacción de doble prueba. Bloquear auto-provisión de candidato; reservar binding único, revalidar destino libre/consentimiento, journal y commit idempotente | Desactivar destino y reactivar origen si sigue probado/seguro; no recrear usuario |
| M4 Coexistencia limitada | Nuevas altas seleccionan intención nueva vs cuenta existente; migrados conservan alias legado mientras la ventana se aprueba. Monitorizar errores y conflictos sin PII | Feature flags por cohorte; ambas tuplas reconocen A mientras permitidas |
| M5 Cutover | Comparar IDs/referencias/digests tras escrituras normales; gates de acceso, recuperación y proveedor perdidos. Avisar a no migrados por canal autorizado, sin tomar email como prueba | Enrutamiento reverso + bindings preservados, no Down ni restore indiscriminado |
| M6 Retire | Revisión explícita de plazo/cohortes no migradas, alternativas de acceso y rollback probado. Revocar solo confianza/bindings autorizados, conservar journal mínimo según política | Para reabrir origen, aprobación y validación; no habilitar cuenta/proveedor comprometidos automáticamente |

Ventana temporal y calendario no fijados sin inventario/acceso QA. No se presume que todos los usuarios existentes puedan migrar en una fecha. Usuarios sin control del origen requieren procedimiento de recuperación aprobado; nunca merge por email ni scripts que actualicen UserId desde CSV de correos.

## Máquina de operación y fallos

`Started → SourceProven → TargetProven → ConsentConfirmed → Prepared → Activated → LegacyRetired`. Terminales: Cancelled/Expired/Conflict; fallos inciertos: Reconciling/ManualReview. No activar derechos mientras está incierta una mutación CIAM.

- Proof source/target están ligados a OperationId, propósito, iniciador y redirect permitido; control reciente <=300s **al confirmar** y TTL operación <=600s según [D3](architecture-review.md#d3--control-reciente-y-step-up-para-beta), con reloj servidor. Exigir auth_time/challenge CIAM acreditado por método; tokens nuevos, iat, SSO/selector Google o prompt solicitado no sustituyen prueba. Sin origen controlable, no saltar a migración por email: recovery con alternativa ya vinculada, o denegar. No inventar prueba del proveedor perdido.
- Locks/constraints sobre User y clave destino previenen dos migraciones hacia usuarios distintos. Retry mismo OperationId devuelve el resultado; payload/versión divergentes 409.
- Timeout después de crear objeto CIAM: no repetir alta a ciegas. Resolver por IDs persistidos/correlation del journal, sin buscar usuario MiKompri por email. CIAM puede tener un objeto pendiente sin User MiKompri; esto no autoriza acceso a datos.
- Timeout después de guardar binding: consultar operación antes de reintentar. Si target ya fue provisionado a otro UserId, Conflict: no mover listas ni borrar al otro usuario. Soporte/manual con evidencia; fusión de cuentas fuera de alcance.
- A/Graph: no existe transacción distribuida con PostgreSQL. Antes de cambiar collection identities, leer/validar inventario y propiedad, registrar versión/fingerprint, bloquear concurrencia, reintentar/reconciliar sin perder el último método. No destruir objeto candidato ni robar identidad ligada a otro usuario por compensación automática.
- B, si se selecciona: vincular la nueva identidad CIAM demostrada a A atómicamente en Users; no necesita mover subjects Google entre objetos, pero añade bindings permanentes y debe probar revocación/último método por binding.

## Altas, duplicación y recuperación

El nuevo login desconectado de una migración autorizada puede ser otra persona; no existe deduplicación segura por email. UI ofrece «Ya tengo cuenta → migrar/vincular» antes de finalizar alta y confirmación explícita de alta nueva. En linking/migración nunca se llama la ruta genérica de auto-provisioning. Para una migración autorizada: cero UserIds nuevos, incluso en carreras y reintentos. Si alguien ya creó dos UserIds, se detecta conflicto y no se fusionan por esta feature.

Recuperación normal sucede en CIAM o con otro método ya probado del mismo usuario. Recuperación asistida sin ninguno exige criterios de seguridad aprobados y revisión humana segregada; si no puede probarse control, acceso denegado y cuenta no recuperable. No usar posesión de un contacto no verificado, nombres de grupos o historial como única prueba. La recuperación nunca crea otro User para sustituir A.

## Ensayo reproducible requerido

Fixture PostgreSQL con A/B, propietario y roles, listas personal/compartida, gastos/participantes, compradores/pagadores y auditoría; manifiesto inicial de IDs/referencias. Probar cada fase con nuevas escrituras, rollback de tráfico, no migrados, null claims, dos destinos, email collision, replay, caída Graph/Users, colisiones de índice y revocación. Comparar manifiesto final y registrar counts/digests, no exportar PII a evidencia pública. Repetir en QA con cuenta Entra existente y Google/email reales. Aún **pendiente**, sin conexión a BD real en esta ejecución.

Durante coexistencia y rollback rige [D1](architecture-review.md#d1--suspensión-y-access-token-ya-emitido): lookup de estado autoritativo en cada API, sin reabrir el binding suspendido por cambiar tráfico a código viejo. El código/entorno de rollback debe conservar esta barrera; si no la soporta, no se habilita esa ruta. Reactivar requiere renovar cortes y no revivir JWT anteriores, incluso emitidos durante suspensión; no deshacer una revocación para recuperar disponibilidad.
