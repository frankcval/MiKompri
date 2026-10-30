# Propuesta de enmienda constitucional — TP11

**Estado**: propuesta para revisión; `.specify/memory/constitution.md` NO se modifica.

**Versión vigente**: 1.1.0. **Versión propuesta**: **1.2.0 (MINOR)**, por añadir reglas de CIAM/migración/linking manteniendo las garantías y el primer despliegue Entra. Si la revisión decide retirar garantías incompatiblemente en lugar de ampliarlas, deberá evaluar MAJOR; esta propuesta no lo hace.

## Mapa exacto de TP11 vigente → propuesta

Los números enumeran las 13 reglas en su orden actual, no líneas de código.

| Regla TP11 actual | Resultado |
| --- | --- |
| 1 Entra ID oficial para primer despliegue | Conservar como hecho/política del primer despliegue; añadir que la evolución de consumidores usa External ID en external tenant, tras aprobación y transición |
| 2 registro independiente por cliente | Conservar; distinguir SPA, futuro MAUI y componente servidor de pruebas si hace falta, por entorno |
| 3 no correlacionar globalmente por sub | Conservar y reforzar: fallback legado no autoriza migración cross-tenant |
| 4 tupla canónica Entra tid/oid | Conservar para identidad externa validada; precisar nuevo tenant/objeto y que varios métodos CIAM pueden compartirlo, sin exigir oid igual entre tenants |
| 5 UserId Guid interno canónico | Conservar y explicitar inmutabilidad en migración/link/recovery |
| 6 Users posee correlación | Conservar; añadir puente old/new autorizado y reglas para N:1 si una decisión B posterior lo requiere |
| 7 referencias de dominio usan UserId | Conservar íntegramente, incluyendo historia financiera |
| 8 sin acceso cross-DB | Conservar; cambios de esquema Users no se consumen directamente por otros contextos |
| 9 audience backend común | Conservar. Coexistencia acotada liga issuer/tenant/audience, sin aceptar combinaciones cruzadas |
| 10 validar issuer/audience/firma/exp/scopes | Conservar; incluir exclusión de tokens sociales/Graph/ID tokens SPA en APIs de negocio |
| 11 autorización de negocio por contexto | Conservar, sin convertir roles/grupos CIAM en roles del dominio |
| 12 clientes nuevos sin duplicados | Conservar; exigir migración/linking explícitos y bloqueo del auto-provisioning de candidatos |
| 13 OBO si se separan audiences | Conservar SHOULD; 010 no separa recursos ni incorpora OBO |

## Texto normativo adicional propuesto (no vigente)

> Tras el primer despliegue Entra ID, MiKompri DEBE evolucionar el acceso de consumidores mediante Microsoft Entra External ID en un external tenant aprobado. Google y email DEBEN permitir acceso sin cuenta Microsoft. Las APIs de negocio SOLO DEBEN aceptar access tokens destinados a MiKompri de emisores/tenants explícitamente confiables; no tokens de proveedores sociales directos.
>
> Users DEBE conservar el UserId Guid existente y todas sus relaciones durante migración, linking y recuperación. `(tid, oid)` identifica una identidad externa del emisor validado, no una persona ni un UserId de dominio. El mapeo entre tenants requiere autorización y pruebas explícitas; MUST NOT deducirse de email, nombre, username, teléfono, avatar o sub global.
>
> Se DEBE preferir CIAM como agregador de métodos. Si una decisión arquitectónica demuestra su insuficiencia, Users PUEDE relacionar varias identidades del mismo CIAM con un UserId mediante prueba segura y unicidad global. Esa opción requiere ADR aprobado y no autoriza múltiples CIAM permanentes.
>
> Linking/unlinking DEBEN exigir consentimiento, prueba reciente de control, anti-replay y auditoría mínima. MUST NOT eliminarse el último método primario utilizable. Credenciales, reset y MFA pertenecen al IdP; personalizaciones del perfil pertenecen a Users y MUST NOT ser sobrescritas por sincronización automática.
>
> La transición de emisores DEBE tener ventana, población, inventario, rollback y gates de retiro aprobados. No se cambian claves de ownership/historia ni se usa coexistencia como excepción indefinida de confianza.

## Proceso y propagación futura

Maintainer/arquitectura revisan ADR-004 y esta propuesta conforme a gobernanza §4.1. Tras aprobación explícita, una ejecución posterior de `/speckit.constitution` actualizará versión/enmienda/sync report y templates spec/plan/tasks que citan TP11. Revisar contratos 003/007 y guías Web/MAUI con historial preservado. No cambiar ahora constitución/templates ni reclamar que el gate TP11 de implementación pasa por existir este archivo.
