# Checklist de aceptación — 010

**Estado**: diseño de aceptación, **cero casos ejecutados/acreditados en esta formalización**. No marcar por presencia de un test/archivo. Evidencia futura incluye SHA, entorno, pasos, resultado, fallos y artefactos sin tokens/PII. «Real» exige CIAM/proveedor/sesiones de QA autorizadas; los negativos controlados complementan, no sustituyen.

| Caso obligatorio | Resultado verificable | Referencias | Evidencia futura |
| --- | --- | --- | --- |
| AC-01 Google sin Microsoft | A solo tiene Google; botón Google → Google autentica → External ID emite token backend → UserId → uso normal. No crear cuenta Microsoft | US1; FR-001/020; T047 | Google real + manifest de UserId y acceso a lista |
| AC-02 Email sin Microsoft | Registro/acceso email gestionado por CIAM; sin contraseña en MiKompri | US1; FR-012; T047 | Email real, caso OTP expirado/denegado y sesión válida |
| AC-03 Entra existente | oldTid/oid → externalTid/oid mantiene exactamente UserId=A, sin INSERT de otro User en migración autorizada | US2; FR-003/010; T048 | Entra real + destino real + journal mínimo |
| AC-04 Ownership/historia | Mismas listas, grupos, membresías, AddedBy/PaidBy/PurchasedBy/gastos/auditoría; sin FK huérfanas ni recalcular reparto | US2; FR-022; T043/048 | Manifiestos antes/después y rollback con escrituras |
| AC-05 Añadir método desde Google | Prueba reciente de ambas partes, consentimiento, mismo UserId; segundo método permite login real | US3; FR-006/009; T049 | Google→email u otro método aprobado real; G1 acreditado |
| AC-06 Mismo email no vincula | Dos accesos/proveedores con mismo email quedan independientes o conflicto, jamás linking automático | US2/3; FR-004/007; T049 | Negativos de email, nombre, username, teléfono/avatar y email cambiado |
| AC-07 Último método | Solo Google no puede quitarse; con alternativa probada puede; dos unlink concurrentes no dejan cero | US3; FR-008; T028/049 | Prueba real e integración con carreras/Graph incierto |
| AC-08 Personalizaciones | Cambios Google de nombre/avatar no sustituyen DisplayName/avatar MiKompri; clear sigue personalizado; reimportar es explícito | US4; FR-014; T036/050 | Login/sync real tras editar, versiones y snapshot separados |
| AC-09 Token CIAM / no Google directo | Access token destino correcto aceptado en tres APIs; token Google directo, Graph, ID token SPA y scope/issuer/aud incorrectos rechazados | US5; FR-020/021; T020/051 | Matriz firmada y QA real, sin publicar JWTs |
| AC-10 B no se vincula a A | Cambiar UserId/OperationId/state/nonce no da acceso A ni crea relación; candidato ocupado no se fusiona | US3; FR-006/007; T027/028/049 | Dos personas reales + ataques controlados y trazas minimizadas |
| AC-11 Logout/cambio cuenta | B nunca ve datos/caché/borradores privados de A, incluso respuesta A tardía, recarga y cambio de issuer | US5; FR-018/023; T025/051 | Browser real y tests de requests pendientes |
| AC-12 Recuperación | Alternativa probada o recovery aprobado recupera el mismo UserId; sin prueba se deniega, no se crea sustituto | US3; FR-008/010; T033/052 | Recovery real y pérdida total de factores documentada |

## Gates de aceptación (todos pendientes)

- [ ] Google/email reales, Microsoft adicional y migración legacy validados; no solo TestAuthHandler/mocks.
- [ ] A demostrado para casos obligatorios, o ADR revisado a B con evidencia y modelo/contratos coherentes.
- [ ] Frescura/step-up por método y permisos Graph probados; no asumir auth_time/MFA ni garantías desde una configuración.
- [ ] Negativos del threat model, idempotencia, carreras, reconciliación y último método acreditados.
- [ ] Perfil/username/avatar y minimización aprobados; campos futuros no recogidos; recuperación y política de borrado/retención revisadas.
- [ ] APIs, roles, grupos y referencias financieras sin regresión; PostgreSQL/Docker y rollback ensayados.
- [ ] Web 320/390/430, teclado/lector/zoom, errores/cancelación/return intent y aislamiento de cuentas validados manualmente cuando corresponda.
- [ ] CI y builds/imágenes del HEAD exacto; budgets 003/007/008/009 conservados sin darlos por satisfechos por este trabajo.
- [ ] ADR/enmienda constitucional aprobados antes de activar confianza nueva; no eliminar soporte legado sin plan para no migrados.
- [ ] MAUI: diseño/SDK/deep links revisados cuando corresponda a 006; no afirmar aceptación productiva de una app inexistente.

## Trazabilidad FR → trabajo

| Requisitos | Tareas principales |
| --- | --- |
| FR-001, FR-012 | T002/006/020–024/047 |
| FR-002, FR-026 | T001/003/007–012/055 |
| FR-003, FR-004, FR-005, FR-010, FR-011 | T013–019/026/043/048 |
| FR-006, FR-007, FR-008, FR-009 | T003/004/027–033/049/052 |
| FR-013, FR-014, FR-015, FR-016, FR-017 | T012/034–040/050 |
| FR-018, FR-019 | T005/023–025/051/053/054 |
| FR-020, FR-021, FR-022, FR-023, FR-024 | T020/021/026/041–046/051 |
| FR-025 | T006/040/046/055 |

SC-001→AC-01–12; SC-002→AC-03/04; SC-003→AC-06/07/10; SC-004→AC-08; SC-005→AC-09 y regresiones; SC-006→AC-11 y gate manual; SC-007→AC-12 y fallos de operación; SC-008→gates/ADR/enmienda. El cierre documental para architecture review no marca esos resultados de implementación como alcanzados.
