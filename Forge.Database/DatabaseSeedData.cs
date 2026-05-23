using Forge.Common.Interfaces;
using Forge.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Forge.Database
{
    public class DatabaseSeedData
    {
        private readonly IPasswordService _passwordService;

        public DatabaseSeedData(IPasswordService passwordService)
        {
            _passwordService = passwordService;
        }

        public void Seed(ForgeDbContext context)
        {
            var permissions = SeedPermissions(context);
            var roles = SeedRoles(context, permissions);
            SeedAdminUser(context, roles);

            context.SaveChanges();
        }

        private static Dictionary<string, Permissions> SeedPermissions(ForgeDbContext context)
        {
            var requiredNames = new[]
            {
                "CreateTask",
                "EditTask",
                "DeleteTask",
                "ViewTask",
                "FinishTask",
                "FailedTask"
            };

            var existing = context.Set<Permissions>()
                .Where(p => requiredNames.Contains(p.Name))
                .ToDictionary(p => p.Name);

            foreach (var name in requiredNames)
            {
                if (existing.ContainsKey(name))
                {
                    continue;
                }

                var permission = new Permissions { Name = name };
                context.Set<Permissions>().Add(permission);
                existing.Add(name, permission);
            }

            return existing;
        }

        private static Dictionary<string, Role> SeedRoles(
            ForgeDbContext context,
            Dictionary<string, Permissions> permissions)
        {
            var definitions = new Dictionary<string, string[]>
            {
                ["Admin"] = permissions.Keys.ToArray(),
                ["User"] = new[] { "EditTask", "ViewTask", "FinishTask", "FailedTask" }
            };

            var existing = context.Set<Role>()
                .Include(r => r.Permissions)
                .Where(r => definitions.Keys.Contains(r.Name))
                .ToDictionary(r => r.Name);

            foreach (var (name, permissionNames) in definitions)
            {
                var rolePermissions = permissionNames.Select(n => permissions[n]).ToList();

                if (existing.TryGetValue(name, out var role))
                {
                    foreach (var permission in rolePermissions)
                    {
                        if (!role.Permissions.Any(p => p.Name == permission.Name))
                        {
                            role.Permissions.Add(permission);
                        }
                    }

                    continue;
                }

                var newRole = new Role { Name = name, Permissions = rolePermissions };
                context.Set<Role>().Add(newRole);
                existing.Add(name, newRole);
            }

            return existing;
        }

        private void SeedAdminUser(ForgeDbContext context, Dictionary<string, Role> roles)
        {
            const string adminLogin = "admin";

            var adminExists = context.Set<User>().Any(u => u.Login == adminLogin);
            if (adminExists)
            {
                return;
            }

            var admin = new User
            {
                Login = adminLogin,
                PasswordHash = _passwordService.Hash("powerAdmin"),
                Name = "Admin User",
                Roles = new List<Role> { roles["Admin"] }
            };

            context.Set<User>().Add(admin);
        }
    }
}
