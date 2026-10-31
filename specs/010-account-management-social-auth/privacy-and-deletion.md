# Privacidad, perfil y eliminación — 010-A / futuro 010-B

## Aplicabilidad de alcance (30/10/2026)

010-A recoge solo perfil básico y tupla canónica mínima: Username opt-in, DisplayName, nombres opcionales, PreferredLanguage/preferencias con finalidad actual e iniciales. No foto propia/del proveedor, inventario de métodos en Users, fecha de nacimiento, teléfono ni dirección. El email ya existente conserva compatibilidad/procedencia, sin convertirse en nuevo recovery o correlación. Credenciales y recuperación pertenecen al proveedor original/CIAM; MiKompri no sustituye identidades perdidas.

La estrategia de eliminación se conserva **como diseño 010-B**, sin endpoint, jobs ni nuevos estados preventivos en A. Pruebas/operaciones de linking, sus TTL y su inventario de métodos también son B. En A solo queda registro mínimo del traslado Microsoft con finalidad de integridad/rollback, no un broker genérico. [Alcance](scope.md), [modelo](data-model.md).


Diseño de minimización, no dictamen jurídico ni plazo legal de conservación. Producto/privacidad deben aprobar finalidades y retención antes de almacenar nuevos campos o habilitar eliminación. Clasificaciones siguientes son operativas; no afirman que una categoría sea «especial» por una ley concreta.

| Dato | Necesidad inicial / obligatoriedad | Riesgo, verificación y borrabilidad |
| --- | --- | --- |
| UserId, binding issuer/tid/oid | Obligatorios para acceso y correlación | Identificadores personales pseudónimos, privados. Prueba del emisor, unicidad y acceso restringido. Retención mínima/tombstone si historia requiere referencia; no se publican como username |
| Métodos/credenciales/MFA | En proveedor/CIAM; gestión de métodos B | Secretos altamente sensibles; no copiar password/OTP/token ni colección Graph a logs. A no necesita inventario propio de métodos |
| DisplayName | Requerido con fallback neutro | Visible a miembros autorizados según contratos; usuario controla su valor. Anonimizable y personalización prioritaria |
| Username | Opcional y público opt-in | Identificador público, no email/login; evitar enumeración de contactos. Reservados/único, política de cambio y no reutilización abusiva; borrar/despublicar al eliminar según política |
| Nombres/apellidos | Opcionales en A | PII de presentación, permitir quitar sin reimportación automática. Foto/avatar propio o del proveedor se difiere a B; A deriva iniciales del perfil propio |
| Email contacto | Opcional en Users; login email pertenece a CIAM | PII de contacto, privado por defecto. Claim email/upn/preferred_username no implica verificado. Guardar autoridad/fecha solo si acreditadas; cambio borra verificación. No inferir linking/recovery |
| Idioma/preferencias | Opcionales, tipados | Datos funcionales mínimos; borrar/restablecer a defaults. No perfilado publicitario |
| Nacimiento | No se recoge en MVP | Riesgo aumentado para edad/perfilado; no finalidad actual para comprar. Extensión opcional futura tras aprobación, sin columna vacía preventiva |
| Teléfono/dirección | No se recogen en MVP | Contacto/localización de riesgo elevado. Sin finalidad de compra actual. MFA CIAM no justifica duplicarlos; verificación futura separada |
| Flujo OAuth y registro de traslado | State/nonce/PKCE efímeros; registro de traslado mínimo A | No almacenar tokens. Broker/ProofReceipt y TTL10min/frescura5min de operaciones sensibles son B, no retención legal ni requisito para login A |
| Auditoría y datos financieros compartidos | Referencias existentes preservadas | Acceso por recurso. No borrar historia ajena para limpiar perfil. Retención y anonimización requieren política por tipo de registro y obligaciones aplicables, no una cifra inventada aquí |

## Reglas de autoridad

External ID es autoridad para autenticación, métodos, credenciales y pruebas de seguridad. Users lo es para perfil funcional y sus personalizaciones. Los claims inicializan campos permitidos al primer registro; un cambio externo no sobrescribe datos propios ni borrados voluntarios. No crear snapshots permanentes sin finalidad. Email cambiado en IdP no reasigna UserId, no acredita otro método ni verifica automáticamente contacto editado.

No solicitar scopes Google/Graph para nombre/foto por conveniencia sin necesidad aprobada. Si el CIAM no entrega avatar, usar iniciales; no descargar arbitrary URLs. Fotos propias/del proveedor quedan en 010-B: antes de incluirlas se exigirán formatos/límites, ownership, eliminación EXIF y protección SSRF/XSS. No desplegar almacenamiento en A.

## Qué significa eliminar una cuenta — diseño 010-B

1. Exigir sesión propia, prueba reciente, confirmación de consecuencias e idempotencia; no confundir «desvincular Google», logout y eliminar cuenta MiKompri.
2. Marcar DeletionRequested y bloquear nuevas mutaciones/acceso según política; revocar bindings/sesiones en Users y coordinar IdP. Verificar estado en tres APIs, pues tokens ya emitidos pueden seguir criptográficamente válidos.
3. Resolver grupos: no dejar último Owner sin alternativa; transferencia/archivo requiere proceso autorizado, no cambio silencioso de OwnerId. Conservar derechos/datos de otros miembros.
4. Clasificar listas propias, compartidas, gastos e historial: borrar solo lo autorizado; anonimizar presentación/PII cuando proceda y mantener UserId tombstone para integridad/auditoría si hay retención. No cascada ciega de membresías ni eliminación de deudas ajenas.
5. Eliminar perfil/snapshots/avatar y métodos/objeto CIAM apropiados cuando sea seguro. No borrar identidad de proveedor Google/Microsoft original ni otros usuarios del directorio. Documentar el orden y fallos parciales como operación reconciliable.
6. Recuperación durante un período reversible solo si está aprobado y antes de anonimización irreversible; conservar UserId. Después de purga irreversible no prometer restauración; backups tienen su propio ciclo de expiración y control de acceso.

**Entrega 010-A:** conservar este diseño y minimización; no implementar estados de borrado, endpoint ni jobs de purga. 010-B requiere aprobación de retención/ownership/recuperación. No habilitar botón que aparente borrar sin poder completar el proceso. Revisar obligaciones concretas con quien corresponda antes de fijar períodos. El soft-delete de un objeto CIAM no equivale a eliminar MiKompri ni sus backups.

Precisiones de [D1/D3](architecture-review.md): bloqueo local en siguiente control autoritativo tras commit, sin caché entre requests ni cancelación retroactiva; reactivación no revive tokens anteriores. En 010-B, ProofReceipt guardará solo datos mínimos de evidencia/versión, no OTP ni tokens; iat para corte de emisión no se guarda como «última autenticación». MFA SMS/registro de factores CIAM, si se aprueba, no autoriza copiar teléfono a perfil Users; no se amplía la recopilación de PII por esta revisión.
