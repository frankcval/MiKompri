# 010 — D1: coste local de dependencia Users

Fecha 2026-10-30; base `b02947f`. [Datos agregados](revocation-benchmark.json), [plan previo](spike-resource-plan.md), fuente `tools/spikes/010-external-id/Program.cs` y runner `run-benchmark.ps1`.

## Método ejecutado

PostgreSQL15 en contenedor independiente, imagen existente sin pull, 1CPU/384MiB, tmpfs256MiB, puerto solo loopback55410. EF crea **el modelo actual** Users en BD QA nueva; 10.000 usuarios sintéticos, email null. Sin migración EF, sin cambios de modelo ni acceso a la BD MiKompri existente. Fixture inexistente obliga abortar; no reutiliza una BD previa.

.NET8.0.32, EF9/Npgsql9 del repo; pool máximo20. Ocho series de1.000 muestras +100 warmup cada una, concurrencias1 y8. Consulta canónica AsNoTracking proyecta Id con índice compuesto existente. Kestrel local compara baseline, lookup y consumidor con un HTTP extra a lookup. Sin JWT/Graph/provisioning/sync de perfil: mide proxy del coste de lectura D1, **no /me actual ni access-status final**. No hay tablas de AccountStatus/Binding/SecurityVersion nuevas.

| Trayecto | p50ms C1 | p95ms C1 | p95ms C8 | Media ms C1 / C8 |
|---|---:|---:|---:|---:|
| Lookup Users EF | 3,072 | 5,178 | 7,098 | 3,371 / 4,433 |
| HTTP baseline | 0,369 | 0,716 | 0,203 | 0,418 / 0,130 |
| HTTP Users lookup | 3,467 | 6,155 | 5,081 | 3,737 / 3,782 |
| Consumidor + HTTP Users extra | 3,168 | 6,072 | 8,241 | 3,486 / 6,129 |

Delta de medias consumidor-baseline ~3,07ms C1 /6,00ms C8. Series separadas con ruido/calentamiento: no restar percentiles como si fueran distribución de coste incremental ni concluir que dos hops siempre son más rápidos que uno. Máximo de ese trayecto156,41ms en C8. Un EXPLAIN ANALYZE caliente: Index Scan,1fila,3 shared hits, planificación0,135ms, ejecución0,053ms. Esa muestra SQL no es p95 ni estimación cloud.

## Dictamen

**PASS de benchmark local reproducible:** no aparece coste SQL evidente que descarte D1 en beta. **INCONCLUSIVE de aceptación operativa D1:** falta cuenta/binding real, JWT, corte de token/suspensión, fail-closed, middleware Users, disponibilidad, red/cloud, carga representativa y tres APIs. No cambia budgets ni promete SLO. ProductCatalog tendrá dependencia Users aunque hoy no resuelva UserId en cada operación; el benchmark no elimina ese riesgo.

Se conserva D1 para el spike: lectura autoritativa por request, sin caché allow inter-request, Redis/bus ni Graph en ruta de datos. No se implementa `/access-status` en producción. Medir consulta final, llamadas por request y fallos antes de aprobar implementación/despliegue; no usar `/me` con provisioning/sync como sustituto de lectura mínima.

## Reproducción y limpieza

`pwsh -File tools/spikes/010-external-id/run-benchmark.ps1`

Por defecto escribe agregados en `.local/` ignorado. Para evidencia revisada, pasar OutputDirectory del spike. El runner verifica nombre/label, genera password solo en memoria/environment local, crea BD nueva y elimina contenedor en finally. Ejecución original: finalizó correctamente, contenedor eliminado, sin volumen. No se reconstruyeron imágenes ni se borraron recursos Docker ajenos.
