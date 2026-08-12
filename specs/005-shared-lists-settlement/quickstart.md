# Quickstart: Validación MVP-3 Shared Lists & Settlement

**Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Data model**: [data-model.md](./data-model.md) | **Contract**: [contracts/shared-lists-settlement-api.md](./contracts/shared-lists-settlement-api.md)

## Objetivo

Validar de extremo a extremo colaboración en listas compartidas y reparto de gastos con trazabilidad, autorización por rol y settlement determinista.

## Prerrequisitos

- .NET 8 SDK instalado.
- Docker Desktop activo.
- Servicios del workspace levantables con compose.
- Usuario(s) autenticables contra proveedor OIDC configurado.
- Grupo existente en Users con al menos 3 miembros (`Owner`, `Admin`, `Member`) para pruebas de matriz.

## Setup rápido

1. Restaurar solución:

```powershell
dotnet restore MiKompri.sln
```

2. Levantar dependencias necesarias (DB/servicios):

```powershell
docker compose up -d
```

3. Iniciar APIs relevantes (Users + ShoppingList):

```powershell
dotnet run --project MiKompri.Users.Api
dotnet run --project MiKompri.ShoppingList.Api
```

## Validación técnica mínima

Ejecutar la regresión de la solución antes de validar los escenarios de la feature:

```powershell
dotnet test test\MiKompri.ShoppingList.Domain.Tests\MiKompri.ShoppingList.Domain.Tests.csproj --configuration Release
dotnet test test\MiKompri.ShoppingList.Application.Tests\MiKompri.ShoppingList.Application.Tests.csproj --configuration Release
dotnet test test\MiKompri.ShoppingList.Api.Tests\MiKompri.ShoppingList.Api.Tests.csproj --configuration Release

dotnet test test\MiKompri.Users.Domain.Tests\MiKompri.Users.Domain.Tests.csproj --configuration Release
dotnet test test\MiKompri.Users.Application.Tests\MiKompri.Users.Application.Tests.csproj --configuration Release
dotnet test test\MiKompri.Users.Api.Tests\MiKompri.Users.Api.Tests.csproj --configuration Release
```

## Escenario 1 — Crear lista compartida y validar acceso por membresía

1. Con token de miembro activo del grupo, crear lista compartida (`POST /api/v1/shared-lists`).
2. Consultar lista con otro miembro activo (`GET /api/v1/shared-lists/{id}`).
3. Intentar consultar con usuario ajeno al grupo.

**Resultado esperado**
- Miembros activos: creación/lectura exitosa.
- Usuario ajeno: rechazo de autorización sin exponer detalles del recurso.

## Escenario 2 — Matriz de permisos (Owner/Admin/Member)

1. `Owner` elimina un gasto y/o lista compartida.
2. `Admin` realiza la misma operación.
3. `Member` intenta eliminar gasto ajeno y lista.

**Resultado esperado**
- `Owner` y `Admin`: permitido según matriz aprobada.
- `Member`: operación rechazada en acciones no permitidas.

## Escenario 3 — Registro de gasto con reglas exactas de reparto

1. Agregar ítem compartido (verificar `AddedBy`).
2. Registrar gasto con:
   - `paidBy` obligatorio,
   - `purchasedBy` opcional,
   - mínimo 1 participante activo,
   - sin participantes duplicados.
3. Repetir caso donde pagador no participa en consumo.

**Resultado esperado**
- Gasto válido se registra.
- Gasto inválido (sin participantes, pagador ausente, precio <= 0, participante no activo/duplicado) se rechaza.

## Escenario 4 — Cálculo de balances y liquidación determinista

1. Registrar múltiples gastos con distintos pagadores/participantes.
2. Consultar resumen (`GET /settlement/summary`).
3. Consultar propuesta (`GET /settlement/proposal`) dos veces con mismos datos.

**Resultado esperado**
- `TotalPaid`, `TotalOwed` y `NetBalance` consistentes.
- Propuesta estable entre ejecuciones (determinista con desempate por `UserId`).

## Escenario 5 — Baja de miembro con histórico preservado

1. Remover/inactivar un miembro en Users con operaciones previas.
2. Recalcular settlement histórico de la lista.
3. Intentar usar al miembro inactivo en nuevo gasto.

**Resultado esperado**
- Histórico preservado y consultable.
- Nuevas operaciones con miembro inactivo rechazadas.

## Protocolo de validación de SC-003

1. Preparar una lista compartida de prueba con al menos 3 miembros activos y token JWT válido (`sub` GUID).
2. Ejecutar el flujo: crear lista (`CreateSharedListRequest`) → agregar ítem (`AddSharedItemRequest`) → registrar gasto (`RegisterExpenseRequest`) → consultar propuesta de liquidación.
3. Medir el tiempo desde la creación de la lista hasta la obtención de `/settlement/proposal`.
4. Guardar evidencia de tiempos y payloads utilizados en una nota de validación o salida de consola reproducible.
5. Repetir la ejecución con el mismo conjunto de datos para confirmar reproducibilidad (misma propuesta de transferencias).

**Criterio de aceptación**
- El flujo completo es ejecutable de forma reproducible y la evidencia queda documentada en el repositorio o en el artefacto de validación.

## Protocolo humano de validación de SC-006

1. Usar una plantilla breve (5-10 preguntas) sobre interpretación de balances y propuesta de liquidación.
2. Aplicar la validación a participantes humanos con contexto del dominio (no sustituir por asserts automatizados).
3. Registrar respuestas y porcentaje de acierto en una tabla o formulario simple.
4. Confirmar que al menos el 90% identifica correctamente deudores, acreedores y direcciones de pago.
5. Adjuntar fecha, versión del entorno y referencia a la lista de ejemplo utilizada para que el ejercicio sea repetible.

**Criterio de aceptación**
- La validación es humana, documentada y repetible; no se convierte en una prueba técnica automatizada artificial.

## No regresión de despliegue

1. Verificar `docker compose config` sin errores.
2. Confirmar que no cambian la plataforma de despliegue ni el modelo CD de la solución.
3. Confirmar que ShoppingList, Users y ProductCatalog siguen compilando y sus pipelines/flows existentes no se alteran.
4. Ejecutar `dotnet build MiKompri.sln --configuration Release --no-restore`.

## Notas

- Este quickstart valida comportamiento funcional; no incluye implementación detallada.
- Para detalles de entidades y reglas exactas usar `data-model.md`; para payloads y endpoints usar `contracts/shared-lists-settlement-api.md`.
