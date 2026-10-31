# Ensayo de traslado Microsoft — estado 31/10/2026

**PARTIAL; sin cutover real.** [Diseño aprobado](../migration.md), [recursos](010a-qa-resource-plan.md), [validación técnica](validation.md).

El servicio/journal y runner administrativo están implementados. Tests PostgreSQL reales prueban reserva única, prohibición de provisión de candidato reservado, origen/destino ocupados, corte/versiones, cutover/rollback sobre la misma fila/UserId y conservación de grupo/membresía/perfil. El verificador externo se sustituye en esas pruebas; siete pruebas del verificador Graph comprueban respuestas controladas de organización/identidad/issuer/cuenta habilitada. No sustituyen federación real ni un manifiesto completo de negocio.

En QA se preparó una federación Microsoft exclusiva y limitada al piloto, sin modificar registros legacy. No se obtuvo una identidad CIAM destino cuya relación inmutable con el origen quedase acreditada. Por eso no se ejecutó reserva/aplicación/rollback real ni se redirige la Web Microsoft a ese proveedor. No se utilizó email, Google o nombre para reparar la equivalencia. Microsoft continúa por su configuración original.

El30/10 se crearon fixtures de grupo/membresía/listas/producto/compra/gasto por APIs QA para preparar un manifiesto; no hubo traslado. Su digest no prueba conservación y sus datos tmpfs se perdieron al reiniciar. El31/10 se inicia QA persistente con únicamente la tupla/UserId Microsoft copiada desde lectura del origen; no asumir continuidad del fixture anterior ni historial completo.

Pendiente T019/T048/A-AC06/07: acreditar issuerAssignedId/origen real, reservar candidato, obtener manifiesto de referencias antes/después, ejecutar cutover con alta pausada, validar token CIAM/UserId, nuevas compras, rollback sin pérdida y rechazo de fuentes retiradas. Mantener legacy hasta acreditarlo. El fallo de login Google por domain_hint se corrigió independientemente; no demuestra insuficiencia del modelo de transición ni autoriza Alternative B.

No nuevas transiciones CA/G3, licencias/trials, SMS, tenant o credenciales para superar este pendiente. Revisar los recursos QA retenidos antes de2026-11-06 según inventario privado. No se cierra migración por coexistencia ni por tests con verificador sustituido.
