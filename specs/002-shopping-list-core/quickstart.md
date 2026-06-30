# Quickstart — MVP 1 Shopping List Core

**Spec**: [spec.md](spec.md) | **Plan**: [plan.md](plan.md)
**Fecha**: 2026-06-30

## Objetivo

Validar de extremo a extremo el flujo core del MVP 1: lista + ítems + progreso + trazabilidad básica.

## Prerrequisitos

- .NET SDK 8
- Docker Desktop + Docker Compose
- Solución restaurada

## 1) Restaurar y compilar

```powershell
dotnet restore MiKompri.sln
dotnet build MiKompri.sln --configuration Release --no-restore
```

## 2) Ejecutar tests del bounded context ShoppingList

```powershell
dotnet test test\MiKompri.ShoppingList.Domain.Tests\MiKompri.ShoppingList.Domain.Tests.csproj --configuration Release
dotnet test test\MiKompri.ShoppingList.Application.Tests\MiKompri.ShoppingList.Application.Tests.csproj --configuration Release
dotnet test test\MiKompri.ShoppingList.Api.Tests\MiKompri.ShoppingList.Api.Tests.csproj --configuration Release
```

## 3) Verificar flujo funcional mínimo

1. Crear una lista de compra.
2. Consultar la lista creada.
3. Agregar un ítem.
4. Editar el ítem.
5. Marcar el ítem como comprado.
6. Consultar progreso (debe actualizarse).
7. Eliminar el ítem.
8. Confirmar progreso en 0% cuando no hay ítems.

## 4) Verificar casos negativos

- Crear lista con nombre inválido → error de validación.
- Consultar lista inexistente → error de no encontrado.
- Editar/eliminar ítem inexistente → error de no encontrado.
- Agregar ítem duplicado en misma lista → error de conflicto de regla de negocio.

## 5) Criterio de salida para pasar a `/speckit.tasks`

- Flujos P1 y P2 trazados y verificables.
- Reglas de negocio y errores esperados mapeados a pruebas.
- No se amplía alcance fuera de MVP 1.
