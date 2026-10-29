# Modelo — 009

PurchaseList conserva Status (operativo). Completada se deriva de Items.Count > 0 y todos IsPurchased; Historial incluye completadas/Closed/Archived.
Estimación derivada Σ(ProductPrice × Quantity) solo valores conocidos, con conteo de desconocidos; cero conocido conserva significado.

ItemExpenseRecord es registro canónico para compras personales y gastos compartidos. No copiar importe real en ListItem. Campos aditivos: MarketId nullable (referencia externa sin FK), PurchasedOn DateOnly nullable, Version GUID (concurrency token). ExpenseDate/CreatedAt no equivalen a fecha real elegida. Personales usan owner para paid/purchased/participante técnico interno, sin reparto UI; shared mantiene reglas 005.

PurchaseOperation: OperationId GUID único, ActorId/ListId/ItemId GUID, RequestHash SHA256 canónico y ExpenseId. Recibo duradero sin cascade desde expense; replay tras borrado produce conflicto, nunca recrea gasto. Hash incluye payload normalizado, no versión posterior del registro. Operaciones distintas permiten compras distintas del mismo ítem.

Migración aditiva sin inventar mercado/fecha histórica ni cambiar IDs. Version inicial de legado debe ser válida; snapshot EF se mantiene. Sin imágenes ni datos Users nuevos.
