using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Forge.Database.Models;
using Forge.Database.SoftDelete;

namespace Forge.Database
{
    public class ForgeDbContext : DbContext
    {
        public ForgeDbContext()
        {

        }

        public ForgeDbContext(DbContextOptions<ForgeDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permissions> Permissions { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique()
                .HasFilter("IsDeleted = 0");

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            modelBuilder.Entity<User>()
                .HasMany(a => a.Roles)
                .WithMany(a => a.Users);

            modelBuilder.Entity<User>()
                .HasMany(a => a.Tasks)
                .WithOne(a => a.User);

            modelBuilder.Entity<Role>()
                .HasMany(a => a.Permissions)
                .WithMany(a => a.Roles);

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.User)
                .WithMany(a => a.AuditLogs);

            modelBuilder.Entity<AuditLog>()
                .HasOne(a => a.Task)
                .WithMany(a => a.AuditLogs);

            ApplySoftDeleteFilter(modelBuilder);
        }

        private static void ApplySoftDeleteFilter(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
                {
                    continue;
                }

                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var notDeleted = Expression.Not(property);
                var lambda = Expression.Lambda(notDeleted, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
