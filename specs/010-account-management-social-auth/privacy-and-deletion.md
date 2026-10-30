# Privacidad, perfil y eliminación — diseño 010

Diseño de minimización, no dictamen jurídico ni plazo legal de conservación. Producto/privacidad deben aprobar finalidades y retención antes de almacenar nuevos campos o habilitar eliminación. Clasificaciones siguientes son operativas; no afirman que una categoría sea «especial» por una ley concreta.

| Dato | Necesidad inicial / obligatoriedad | Riesgo, verificación y borrabilidad |
| --- | --- | --- |
| UserId, binding issuer/tid/oid | Obligatorios para acceso y correlación | Identificadores personales pseudónimos, privados. Prueba del emisor, unicidad y acceso restringido. Retención mínima/tombstone si historia requiere referencia; no se publican como username |
| Métodos/credenciales/MFA | Necesarios en CIAM, no en Users | Secretos altamente sensibles; no copiar password/OTP/token ni toda la colección Graph a logs. Inventario mínimo temporal para gestionar acceso |
| DisplayName | Requerido con fallback neutro | Visible a miembros autorizados según contratos; usuario controla su valor. Anonimizable y personalización prioritaria |
| Username | Opcional y público opt-in | Identificador público, no email/login; evitar enumeración de contactos. Reservados/único, política de cambio y no reutilización abusiva; borrar/despublicar al eliminar según política |
| Nombres/apellidos/avatar | Opcionales | PII de presentación; no extraer rasgos biométricos. Permitir quitar sin reimportación automática. Snapshot mínimo y purga; avatar propio separado de credenciales |
| Email contacto | Opcional en Users; login email pertenece a CIAM | PII de contacto, privado por defecto. Claim email/upn/preferred_username no implica verificado. Guardar autoridad/fecha solo si acreditadas; cambio borra verificación. No inferir linking/recovery |
| Idioma/preferencias | Opcionales, tipados | Datos funcionales mínimos; borrar/restablecer a defaults. No perfilado publicitario |
| Nacimiento | No se recoge en MVP | Riesgo aumentado para edad/perfilado; no finalidad actual para comprar. Extensión opcional futura tras aprobación, sin columna vacía preventiva |
| Teléfono/dirección | No se recogen en MVP | Contacto/localización de riesgo elevado. Sin finalidad de compra actual. MFA CIAM no justifica duplicarlos; verificación futura separada |
| Pruebas OAuth y operaciones | Efímeras; journal mínimo necesario | Código/verifier/nonce protegidos y efímeros; journal almacena IDs/digests/resultados, no tokens. TTL propuesto 10min y frescura 5min no son plazos legales de historia |
| Auditoría y datos financieros compartidos | Referencias existentes preservadas | Acceso por recurso. No borrar historia ajena para limpiar perfil. Retención y anonimización requieren política por tipo de registro y obligaciones aplicables, no una cifra inventada aquí |

## Reglas de autoridad

External ID es autoridad para autenticación, métodos, credenciales y pruebas de seguridad. Users lo es para perfil funcional y sus personalizaciones. Un cambio externo actualiza como máximo snapshot importado, nunca dato User. Email cambiado en IdP no reasigna UserId, no acredita otro método ni verifica automáticamente contacto editado.

No solicitar scopes Google/Graph para nombre/foto por conveniencia sin necesidad aprobada. Si el CIAM no entrega avatar, usar iniciales; no descargar arbitrary URLs. Una foto propia se procesa y sirve conforme a [modelo de avatar](data-model.md), sin EXIF ni ficheros activos; no desplegar almacenamiento ahora.

## Qué significa eliminar una cuenta

1. Exigir sesión propia, prueba reciente, confirmación de consecuencias e idempotencia; no confundir «desvincular Google», logout y eliminar cuenta MiKompri.
2. Marcar DeletionRequested y bloquear nuevas mutaciones/acceso según política; revocar bindings/sesiones en Users y coordinar IdP. Verificar estado en tres APIs, pues tokens ya emitidos pueden seguir criptográficamente válidos.
3. Resolver grupos: no dejar último Owner sin alternativa; transferencia/archivo requiere proceso autorizado, no cambio silencioso de OwnerId. Conservar derechos/datos de otros miembros.
4. Clasificar listas propias, compartidas, gastos e historial: borrar solo lo autorizado; anonimizar presentación/PII cuando proceda y mantener UserId tombstone para integridad/auditoría si hay retención. No cascada ciega de membresías ni eliminación de deudas ajenas.
5. Eliminar perfil/snapshots/avatar y métodos/objeto CIAM apropiados cuando sea seguro. No borrar identidad de proveedor Google/Microsoft original ni otros usuarios del directorio. Documentar el orden y fallos parciales como operación reconciliable.
6. Recuperación durante un período reversible solo si está aprobado y antes de anonimización irreversible; conservar UserId. Después de purga irreversible no prometer restauración; backups tienen su propio ciclo de expiración y control de acceso.

**Entrega inicial:** diseño, estados y pruebas de política, pero endpoint de borrado productivo y jobs de purga fuera de alcance hasta aprobación de retención/ownership/recuperación. No habilitar botón que aparente borrar sin poder completar el proceso. Revisar obligaciones concretas con quien corresponda antes de fijar períodos. El soft-delete de un objeto CIAM no equivale a eliminar MiKompri ni sus backups.

Precisiones de [D1/D3](architecture-review.md): bloqueo local en siguiente control autoritativo tras commit, sin caché entre requests ni cancelación retroactiva; reactivación no revive tokens anteriores. ProofReceipt guarda solo datos mínimos de evidencia/versión, no OTP ni tokens; iat para corte de emisión no se guarda como «última autenticación». MFA SMS/registro de factores CIAM, si se aprueba, no autoriza copiar teléfono a perfil Users; no se amplía la recopilación de PII por esta revisión.
