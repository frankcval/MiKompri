using Microsoft.EntityFrameworkCore;
using MiKompri.Users.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiKompri.Users.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UsersDbContext _context;

        public UserRepository(UsersDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // D1 canonical lookup already read the primary within this request. Reuse its
            // tracked entity for profile/handlers; DbContext is never shared across requests.
            return await _context.Users.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<User?> GetByCanonicalIdentityAsync(
            string identityProvider,
            string tenantId,
            string objectId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(
                    u => u.IdentityProvider == identityProvider &&
                         u.TenantId == tenantId &&
                         u.ObjectId == objectId,
                    cancellationToken);
        }

        public async Task<User?> GetByExternalIdAsync(
            string identityProvider,
            string externalUserId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .FirstOrDefaultAsync(
                    u => u.IdentityProvider == identityProvider &&
                         u.ExternalUserId == externalUserId,
                    cancellationToken);
        }

        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await using var tx = _context.Database.ProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL"
                ? await _context.Database.BeginTransactionAsync(cancellationToken) : null;
            await IdentityTupleLock.AcquireAsync(_context, cancellationToken, (user.IdentityProvider, user.TenantId!, user.ObjectId!));
            if (await IsIdentityReservedAsync(user.IdentityProvider, user.TenantId!, user.ObjectId!, cancellationToken))
                throw new UnauthorizedAccessException("Authentication failed.");
            try
            {
                await _context.Users.AddAsync(user, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                if (tx is not null) await tx.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
            {
                _context.Entry(user).State = EntityState.Detached;
                throw new UserConflictException();
            }
        }

        public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Update(user);
            try { await _context.SaveChangesAsync(cancellationToken); }
            catch (DbUpdateConcurrencyException) { throw new ProfileConflictException(); }
            catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" }) { throw new UserConflictException(); }
        }

        public Task<bool> IsIdentityReservedAsync(string provider, string tenantId, string objectId, CancellationToken cancellationToken = default) =>
            _context.IdentityMigrations.AnyAsync(x =>
                (x.SourceProvider == provider && x.SourceTenantId == tenantId && x.SourceObjectId == objectId) ||
                (x.TargetProvider == provider && x.TargetTenantId == tenantId && x.TargetObjectId == objectId), cancellationToken);

        public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
            _context.Users.FirstOrDefaultAsync(x => x.Username == username, cancellationToken);
    } //Se Guarda cambios inmediatamente porque User no es Aggregate Root de otros.
}
