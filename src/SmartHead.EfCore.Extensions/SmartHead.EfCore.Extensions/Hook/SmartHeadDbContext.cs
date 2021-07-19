using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartHead.EfCore.Extensions.Interfaces;

namespace SmartHead.EfCore.Extensions.Hook
{
    public class SmartHeadDbContext<TUser, TRole, TUserRole, TKey>
        : IdentityDbContext<TUser, TRole, TKey, IdentityUserClaim<TKey>, TUserRole, IdentityUserLogin<TKey>,
            IdentityRoleClaim<TKey>, IdentityUserToken<TKey>>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TKey : IEquatable<TKey>
    {
        public SmartHeadDbContext([NotNull] DbContextOptions options) : base(options)
        {
        }

        public override int SaveChanges()
        {
            CreationTimeCommit();
            ModificationTimeCommit();

            return base.SaveChanges();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            CreationTimeCommit();
            ModificationTimeCommit();

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = new CancellationToken())
        {
            CreationTimeCommit();
            ModificationTimeCommit();

            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            CreationTimeCommit();
            ModificationTimeCommit();

            return base.SaveChangesAsync(cancellationToken);
        }

        protected virtual void CreationTimeCommit()
        {
            var entries = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Added && x.Entity is IHasCreationTime)
                .ToArray();

            foreach (var entry in entries)
            {
                if (entry.Entity is IHasCreationTime entity)
                    entity.CreationTime = DateTime.UtcNow;
            }
        }

        protected virtual void ModificationTimeCommit()
        {
            var entries = ChangeTracker.Entries()
                .Where(x => x.State == EntityState.Modified && x.Entity is IHasModificationTime)
                .ToArray();

            foreach (var entry in entries)
            {
                if (entry.Entity is IHasModificationTime entity)
                    entity.LastModificationTime = DateTime.UtcNow;
            }
        }
    }
}