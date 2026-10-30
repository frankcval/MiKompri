# Transición Microsoft — 010-A

**Diseño, no ejecutado.** Mantener `UserId=A` y todas sus relaciones. La nueva tupla CIAM puede ser distinta; no recrear User, OwnerId, memberships, AddedBy, PaidBy, PurchasedBy, compras, gastos ni historia.

## Ruta inicial y destino

Inicio de010-A: Microsoft sigue por el acceso actual; Google nuevo entra por CIAM. No redirigir usuarios existentes a una alta CIAM desconocida. Eso permite desarrollar Google sin esperar linking/G3.

Destino: Microsoft autenticado por el mismo tenant origen, federado en External ID, token CIAM y mismo UserId. Traslado por población/cohorte autorizada, no «Vincular cuenta»/«Cambiar proveedor» ni adopción de un Google arbitrario. Coexistencia inicial no acredita que la migración esté terminada: T048 exige ensayo real de traslado/rollback y el retiro productivo tiene su propio gate.

## Evidencia de equivalencia

La [guía Microsoft de federación Entra](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-entra-id-federation-customers), reconsultada30/10/2026, documenta el origen federado y creación de objeto CIAM con identidad referida al object ID del usuario en el tenant fuente. Es una vía documental de preparación; no es una migración MiKompri ya validada.

El operador autorizado parte de la canónica Microsoft existente en Users, comprueba tenant/objeto origen y la relación federada inmutable del objeto CIAM destino en el emisor configurado. Reserva `oldNamespace/tid/oid → UserId → newNamespace/tid/oid`. Deben acreditarse esquema de issuer/issuerAssignedId real, control de configuración y lectura de esa relación en QA; no copiar literalmente un ejemplo de URL sin validación.

Email puede ser atributo requerido por federación; **nunca es la prueba ni la clave de mapeo**. No usar nombre, invitación o sub global. Fuente incompleta/conflicto/objeto Google ya asociado →no traslado; conservar Microsoft. No tomar dos sesiones SSO del mismo email como equivalencia.

## Procedimiento acotado, sin broker G3

| Fase | Acción y gate |
|---|---|
| M0 inventario | Users produce IDs/tuplas y manifiestos de referencias mediante procesos autorizados; detectar null legacy/duplicados. Canónica incompleta solo se rellena por flujo legacy acreditado, no por inferencia |
| M1 preparar | Preparar/resolver objeto CIAM con vínculo al mismo origen Microsoft y reservar destino libre en registro de cohorte. Revisar equivalencia y permisos. No cambiar todavía la canónica del User |
| M2 aislar piloto | Antes de habilitar entrada Microsoft CIAM a la población de prueba, impedir aprovisionamiento genérico de candidatos. En QA/piloto usar ventana controlada con alta nueva pausada; desconocido no obtiene User sustituto. Sin pantalla de linking ni nuevo servicio de pruebas |
| M3 cutover | Transacción Users: versión/origen/destino reservados, cambiar canónica del User existente, marcar origen retirado y actualizar cortes D1/auditoría. Reintento idempotente por cohorte/entrada |
| M4 validar/revertir | Login Microsoft CIAM del usuario aprobado resuelve exactamente A. Comparar ownership/historia y nuevas compras. Ensayar rollback a origen autorizado, mismo A y cortes renovados, sin Down ni fusión |
| M5 retirar | Solo con inventario completo/plan de no migrados y rollback probado, retirar confianza/ruta fuente de esa población. No inventar fecha de retiro ni forzar usuarios sin equivalencia |

La pausa de nuevas altas del piloto evita clasificar un Microsoft CIAM desconocido como Google nuevo. No es un bloqueo permanente del producto ni un broker de proveedores. El login de cuentas conocidas continúa. Antes de producción aprobar duración/comunicación; no exponer un flujo Microsoft CIAM con auto-provisión abierta mientras haya usuarios existentes sin mapeo.

Para el primer release se mantiene Microsoft legacy hasta que su cutover esté acreditado. Un piloto QA reversible no obliga a mover producción; **no** se marca la tarea de migración completada solo por conservar legacy. Google y perfil pueden implementarse sin esperar a esa conmutación.

## Conflictos y reversión

- Destino perteneciente a otro UserId →conflicto, no merge ni cambio de listas.
- Reintento tras timeout →leer registro/versiones y destino antes de actuar; no volver a crear objetos a ciegas.
- Tupla fuente retirada →negar auto-provisión/fallback sub que produciría duplicado o cambiaría canónica de vuelta.
- Sin prueba de origen, no automatizar compensación; legacy sigue utilizable según estado de cuenta.
- Rollback restaura enrutamiento/canónica autorizada y conserva journal/perfil/nuevas escrituras. No deshace suspensión D1 ni acepta JWT anteriores al corte.

La autoridad del traslado es federación del **mismo origen Microsoft** e inventario/reserva aprobados; no una operación de usuario para probar Google+Microsoft. D3/G3 para cambio autoservicio entre proveedores permanece en010-B. Si la equivalencia inmutable no puede demostrarse, no sustituirla por email ni relajar G3: detener traslado y revisar diseño, conservando acceso Microsoft.

## Aceptación obligatoria

T014/017/019/043/048: A/B, legacy incompleto, destino ocupado, dos reintentos concurrentes, pausa de provisión/candidato desconocido, caída en cada frontera, login y rollback, igualdad exacta UserId/ownership antes/después y rechazo de JWT de fuente retirada. Manifiestos por contexto sin cross-DB en runtime ni PII en evidencia.

La QA del spike no acredita este recorrido de migración. No es gate G3 del primer login aislado, pero sí gate de cutover y cierre de la tarea de traslado. Recuperación si se pierde el acceso original se delega al proveedor; no sustituir identidad mediante otro proveedor en A.
