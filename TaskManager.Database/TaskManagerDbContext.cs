using Microsoft.EntityFrameworkCore;
using TaskManager.Database.Models;

namespace TaskManager.Database
{
    public class TaskManagerDbContext : DbContext
    {
        public TaskManagerDbContext()
        {
            
        }

        public TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options) : base(options)
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
        }
    }
}
