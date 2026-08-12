# REST API Contract: Shared Lists & Settlement (MVP-3)

**Feature**: 005-shared-lists-settlement | **Version**: v1 | **Date**: 2026-08-06
**Base URL**: `/api/v1`

## Convenciones

- Autenticación: JWT Bearer emitido por proveedor OIDC externo.
- La identidad del caller se obtiene del claim `sub`.
- Autorización de grupo: validación por membresía/rol (`Owner`, `Admin`, `Member`) vía contrato con Users.
- Timestamps en UTC ISO 8601.
- Errores alineados a middleware estándar (`status`, `error`, `traceId`, `errors[]` cuando aplique).
- Códigos frecuentes: `200`, `201`, `204`, `400`, `401`, `403`, `404`.
- Para evitar enumeración de recursos, acceso no permitido no debe revelar existencia de la lista compartida.

---

## Comportamiento de autenticación y autorización

- `401 Unauthorized`: ausencia de token válido, token inválido o ausencia del claim `sub`.
- `403 Forbidden`: existe identidad válida, pero el caller no tiene membresía activa, no tiene permiso por rol o intenta operar fuera de su ámbito.
- Las respuestas de `401` y `403` deben seguir el mismo formato de error que el resto de la API.

---

## Listas compartidas

### `POST /api/v1/shared-lists`
Crea una lista compartida asociada a un grupo.

**Payload final (API Model): `CreateSharedListRequest`**

**Request**
```json
{
  "name": "Supermercado semana 32",
  "description": "Compra familiar",
  "groupId": "8d5fcd61-1b2e-4e70-97c7-8a9ee2a9c8f0"
}
```

**Responses**
- `201 Created` + `Guid` del recurso creado.
- `401 Unauthorized` si no existe identidad válida.
- `403 Forbidden` si el caller no es miembro activo del grupo.
- `400 Bad Request` por validación.

### `GET /api/v1/shared-lists/{sharedListId}`
Consulta detalle de lista compartida.

**Responses**
- `200 OK` cuando el caller tiene acceso.
- `401 Unauthorized` si no existe identidad válida.
- `403 Forbidden` si no hay membresía activa o permiso suficiente.

### `PATCH /api/v1/shared-lists/{sharedListId}`
Actualiza nombre/descripcion.

### `PATCH /api/v1/shared-lists/{sharedListId}/close`
Cierra lista para nuevas operaciones.

### `DELETE /api/v1/shared-lists/{sharedListId}`
Elimina lista compartida (solo `Owner`/`Admin` según matriz aprobada).

---

## Ítems compartidos

### `POST /api/v1/shared-lists/{sharedListId}/items`
Agrega ítem a la lista compartida. Registra automáticamente `AddedBy`.

**Payload final (API Model): `AddSharedItemRequest`**

**Request**
```json
{
  "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Leche Entera",
  "quantity": 2,
  "estimatedPrice": 1.45
}
```

**Responses**
- `201 Created`
- `401 Unauthorized` si no existe identidad válida.
- `403 Forbidden` si no es miembro activo o no tiene permiso.
- `400 Bad Request` por validación.

### `PATCH /api/v1/shared-lists/{sharedListId}/items/{itemId}`
Edita ítem (según permisos).

### `PATCH /api/v1/shared-lists/{sharedListId}/items/{itemId}/purchase`
Marca o revierte compra del ítem.

---

## Gastos por ítem

### `POST /api/v1/shared-lists/{sharedListId}/items/{itemId}/expenses`
Registra gasto real del ítem.

**Payload final (API Model): `RegisterExpenseRequest`**

**Request**
```json
{
  "paidBy": "f4d4b27d-b337-4e92-bf4a-6de3c949f9a2",
  "purchasedBy": "f4d4b27d-b337-4e92-bf4a-6de3c949f9a2",
  "realPaidPrice": 3.79,
  "currency": "EUR",
  "participants": [
    "f4d4b27d-b337-4e92-bf4a-6de3c949f9a2",
    "0a9a9f3d-2473-4d9b-a7f2-2cd0dc45d0d3"
  ]
}
```

**Reglas contractuales**
- `paidBy` obligatorio.
- `purchasedBy` opcional.
- Mínimo 1 participante activo.
- Sin participantes duplicados.
- `realPaidPrice > 0`.

**Responses**
- `201 Created`
- `401 Unauthorized` si no existe identidad válida.
- `403 Forbidden` por autorización o membresía.
- `400 Bad Request` por reglas de gasto/reparto.

### `PATCH /api/v1/shared-lists/{sharedListId}/items/{itemId}/expenses/{expenseId}`
Actualiza gasto y/o participantes (según permisos).

**Payload final (API Model): `UpdateExpenseRequest`**

### `DELETE /api/v1/shared-lists/{sharedListId}/items/{itemId}/expenses/{expenseId}`
Elimina gasto (solo rol autorizado por matriz).

---

## Reparto y liquidación

### `GET /api/v1/shared-lists/{sharedListId}/settlement/summary`
Devuelve por participante: total pagado, total correspondiente y balance neto.

**Response 200 (ejemplo)**
```json
{
  "sharedListId": "f1d5d9fd-b54d-4f17-ab56-b72be28c0f6d",
  "balances": [
    {
      "userId": "f4d4b27d-b337-4e92-bf4a-6de3c949f9a2",
      "totalPaid": 12.50,
      "totalOwed": 8.20,
      "netBalance": 4.30,
      "type": "Creditor"
    },
    {
      "userId": "0a9a9f3d-2473-4d9b-a7f2-2cd0dc45d0d3",
      "totalPaid": 2.00,
      "totalOwed": 6.30,
      "netBalance": -4.30,
      "type": "Debtor"
    }
  ]
}
```

### `GET /api/v1/shared-lists/{sharedListId}/settlement/proposal`
Devuelve propuesta de liquidación simplificada determinista.

**Regla determinista**
- Emparejar mayor deudor con mayor acreedor.
- En empate de importe, ordenar por `UserId` ascendente.
- El conjunto de transferencias debe ser estable para el mismo input.

**Response 200 (ejemplo)**
```json
{
  "sharedListId": "f1d5d9fd-b54d-4f17-ab56-b72be28c0f6d",
  "generatedAt": "2026-08-06T18:20:00Z",
  "transfers": [
    {
      "fromUserId": "0a9a9f3d-2473-4d9b-a7f2-2cd0dc45d0d3",
      "toUserId": "f4d4b27d-b337-4e92-bf4a-6de3c949f9a2",
      "amount": 4.30
    }
  ]
}
```

---

## Trazabilidad y membresías

### `GET /api/v1/shared-lists/{sharedListId}/audit-events`
Consulta eventos relevantes de trazabilidad de lista/gastos.

### Comportamiento ante baja de miembro

- Operaciones históricas del miembro se conservan en consultas de auditoría y settlement histórico.
- Nuevas operaciones no permiten usar miembros inactivos/eliminados como pagador/comprador/participante.

---

## Fuera de alcance contractual (MVP-3)

- Gestión de grupos/membresías desde ShoppingList.
- Acceso directo a BD de Users.
- Sincronización automática de precios con ProductCatalog.
- Pagos bancarios reales, transferencias monetarias reales, notificaciones push, OCR, presupuestos, recomendaciones.
