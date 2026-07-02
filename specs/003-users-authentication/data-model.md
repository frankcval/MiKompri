# Data Model: 003 — Users Authentication & Groups

**Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md) | **Date**: 2026-07-12

---

## Entidades del dominio

### Entity (base abstract)

```csharp
// MiKompri.Users.Domain/Abstractions/Entity.cs  — MODIFICAR
public abstract class Entity
{
	public Guid       Id        { get; protected set; }
	public DateTime   CreatedAt { get; protected set; }
	public DateTime   UpdatedAt { get; protected set; }
}
```

Todos los constructores de agregado deben inicializar:
```csharp
Id        = Guid.NewGuid();
CreatedAt = DateTime.UtcNow;
UpdatedAt = DateTime.UtcNow;
```

---

### GroupRole (enum)

```csharp
// MiKompri.Users.Domain/Users/GroupRole.cs  — MODIFICAR
public enum GroupRole
{
	Owner  = 1,
	Admin  = 2,   // ← NUEVO
	Member = 3    // antes era 2
}
```

> **Seguridad de migración**: `GroupMembershipConfiguration` usa `HasConversion<string>()` — los valores de BD son strings ("Owner", "Admin", "Member"). El cambio de `Member = 2` a `Member = 3` no afecta BD. No hay migración previa que actualizar.

---

### User

```csharp
// MiKompri.Users.Domain/Users/User.cs  — MODIFICAR
public class User : Entity
{
	public string  DisplayName       { get; private set; } = string.Empty;
	public string? Email             { get; private set; }
	public string  IdentityProvider  { get; private set; } = string.Empty;
	public string  ExternalUserId    { get; private set; } = string.Empty;

	private readonly List<GroupMembership> _memberships = new();
	public IReadOnlyCollection<GroupMembership> Memberships => _memberships.AsReadOnly();

	private User() { }  // EF

	public User(string identityProvider, string externalUserId,
				string? displayName, string? email)
	{
		Id               = Guid.NewGuid();
		CreatedAt        = DateTime.UtcNow;
		UpdatedAt        = DateTime.UtcNow;
		IdentityProvider = identityProvider;
		ExternalUserId   = externalUserId;
		DisplayName      = displayName ?? string.Empty;
		Email            = email;
	}

	// FR-004: actualización voluntaria de nombre por el usuario
	public void UpdateProfile(string displayName, string? email = null)
	{
		if (string.IsNullOrWhiteSpace(displayName))
			throw new InvalidOperationException("El nombre no puede estar vacío.");
		DisplayName = displayName;
		// email opcional: si se pasa null no se modifica
		if (email is not null) Email = email;
		UpdatedAt = DateTime.UtcNow;
	}

	// FR-016: refresco explícito desde claims del token
	public void SyncClaims(string? displayName, string? email)
	{
		bool changed = false;
		if (displayName is not null && displayName != DisplayName)
		{
			DisplayName = displayName;
			changed = true;
		}
		if (email != Email)
		{
			Email = email;
			changed = true;
		}
		if (changed) UpdatedAt = DateTime.UtcNow;
	}
}
```

---

### Group

```csharp
// MiKompri.Users.Domain/Users/Group.cs  — MODIFICAR
public class Group : Entity
{
	public string Name    { get; private set; } = string.Empty;
	public Guid   OwnerId { get; private set; }   // denormalizado para consultas rápidas

	private readonly List<GroupMembership> _memberships = new();
	public IReadOnlyCollection<GroupMembership> Memberships => _memberships.AsReadOnly();

	private Group() { }  // EF

	public Group(string name, Guid ownerId)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("El nombre del grupo no puede estar vacío.");

		Id        = Guid.NewGuid();
		CreatedAt = DateTime.UtcNow;
		UpdatedAt = DateTime.UtcNow;
		Name      = name;
		OwnerId   = ownerId;

		// FR-006: el creador queda como Owner automáticamente
		_memberships.Add(GroupMembership.Create(Id, ownerId, GroupRole.Owner));
	}

	// FR-007: agregar miembro — regla de negocio: sin duplicados
	public GroupMembership AddMember(Guid userId, GroupRole role)
	{
		if (_memberships.Any(m => m.UserId == userId))
			throw new InvalidOperationException("El usuario ya pertenece al grupo.");

		var membership = GroupMembership.Create(Id, userId, role);
		_memberships.Add(membership);
		UpdatedAt = DateTime.UtcNow;
		return membership;
	}

	// FR-009: eliminar miembro — con reglas de privilegio de rol
	// requestingRole: rol del caller; targetUserId: miembro a eliminar
	public void RemoveMember(Guid targetUserId, GroupRole requestingRole)
	{
		var target = _memberships.FirstOrDefault(m => m.UserId == targetUserId)
			?? throw new KeyNotFoundException("El usuario no es miembro del grupo.");

		// Nadie puede eliminar al Owner
		if (target.Role == GroupRole.Owner)
			throw new InvalidOperationException("No se puede eliminar al propietario del grupo.");

		// Admin no puede eliminar a otro Admin (Q2)
		if (requestingRole == GroupRole.Admin && target.Role == GroupRole.Admin)
			throw new InvalidOperationException("Un Admin no puede eliminar a otro Admin.");

		_memberships.Remove(target);
		UpdatedAt = DateTime.UtcNow;
	}

	// FR-010: protección de último Owner (llamar antes de RemoveMember si el target es Owner)
	public bool HasSingleOwner() =>
		_memberships.Count(m => m.Role == GroupRole.Owner) == 1;

	public bool IsMember(Guid userId) =>
		_memberships.Any(m => m.UserId == userId);

	public GroupRole? GetMemberRole(Guid userId) =>
		_memberships.FirstOrDefault(m => m.UserId == userId)?.Role;

	// Mantener compatibilidad con handlers existentes
	public bool IsOwner(Guid userId) => OwnerId == userId;
}
```

---

### GroupMembership

```csharp
// MiKompri.Users.Domain/Users/GroupMembership.cs  — MODIFICAR
public class GroupMembership : Entity
{
	public Guid      GroupId  { get; private set; }
	public Guid      UserId   { get; private set; }
	public GroupRole Role     { get; private set; }
	public DateTime  JoinedAt { get; private set; }   // ← NUEVO

	private GroupMembership() { }  // EF

	private GroupMembership(Guid groupId, Guid userId, GroupRole role)
	{
		Id        = Guid.NewGuid();
		CreatedAt = DateTime.UtcNow;
		UpdatedAt = DateTime.UtcNow;
		GroupId   = groupId;
		UserId    = userId;
		Role      = role;
		JoinedAt  = DateTime.UtcNow;
	}

	public static GroupMembership Create(Guid groupId, Guid userId, GroupRole role)
		=> new(groupId, userId, role);

	public void ChangeRole(GroupRole newRole)
	{
		Role      = newRole;
		UpdatedAt = DateTime.UtcNow;
	}
}
```

---

## Esquema de base de datos — tabla por tabla

### `Users`

| Columna | Tipo | Restricciones |
|---------|------|--------------|
| `Id` | `uuid` | PK |
| `DisplayName` | `varchar(100)` | NOT NULL |
| `Email` | `varchar(200)` | nullable |
| `IdentityProvider` | `varchar(100)` | NOT NULL |
| `ExternalUserId` | `varchar(200)` | NOT NULL |
| `CreatedAt` | `timestamp with time zone` | NOT NULL |
| `UpdatedAt` | `timestamp with time zone` | NOT NULL |

**Índice único**: `(IdentityProvider, ExternalUserId)` — ya configurado en `UserConfiguration.cs`.

---

### `Groups`

| Columna | Tipo | Restricciones |
|---------|------|--------------|
| `Id` | `uuid` | PK |
| `Name` | `varchar(100)` | NOT NULL |
| `OwnerId` | `uuid` | NOT NULL, FK implícita a `Users.Id` (no forzada en DB por ahora) |
| `CreatedAt` | `timestamp with time zone` | NOT NULL |
| `UpdatedAt` | `timestamp with time zone` | NOT NULL |

**Índice**: `OwnerId` — ya configurado.

---

### `GroupMemberships`

| Columna | Tipo | Restricciones |
|---------|------|--------------|
| `Id` | `uuid` | PK |
| `GroupId` | `uuid` | NOT NULL, FK → `Groups.Id` CASCADE DELETE |
| `UserId` | `uuid` | NOT NULL, FK → `Users.Id` CASCADE DELETE |
| `Role` | `varchar(50)` | NOT NULL (`"Owner"` / `"Admin"` / `"Member"`) |
| `JoinedAt` | `timestamp with time zone` | NOT NULL |
| `CreatedAt` | `timestamp with time zone` | NOT NULL |
| `UpdatedAt` | `timestamp with time zone` | NOT NULL |

**Índice único**: `(GroupId, UserId)` — ya configurado.
**Índices simples**: `UserId`, `GroupId` — ya configurados.

---

## Actualizaciones de configuración EF Core

### `UserConfiguration.cs` — añadir timestamps

```csharp
builder.Property(u => u.CreatedAt).IsRequired();
builder.Property(u => u.UpdatedAt).IsRequired();
```

### `GroupConfiguration.cs` — añadir timestamps

```csharp
builder.Property(g => g.CreatedAt).IsRequired();
builder.Property(g => g.UpdatedAt).IsRequired();
```

### `GroupMembershipConfiguration.cs` — añadir JoinedAt y timestamps

```csharp
builder.Property(m => m.JoinedAt).IsRequired();
builder.Property(m => m.CreatedAt).IsRequired();
builder.Property(m => m.UpdatedAt).IsRequired();
```

---

## Migración inicial de Users

```powershell
# Desde la raíz del repositorio
dotnet ef migrations add InitialUsers `
  --project MiKompri.Users.Infrastructure `
  --startup-project MiKompri.Users.Api `
  --output-dir Persistence/Migrations
```

> **Prerequisito**: `MiKompri.Users.Api` debe tener `UsersDbContext` registrado y connection string válida en `appsettings.Development.json` antes de ejecutar este comando.

---

## Referencia cruzada con ShoppingList (futuro)

`GroupId` (`Guid`) es el identificador que actuará como referencia canónica cuando ShoppingList implemente listas compartidas. El modelo de datos no expone ninguna referencia directa hacia ShoppingList en esta iteración — el vínculo se añadirá en la siguiente feature.
