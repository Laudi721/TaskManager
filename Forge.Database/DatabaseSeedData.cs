using System.Data;
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
            SeedAdminRole(context);
            SeedAdminUser(context);
        }

        private void SeedAdminRole(ForgeDbContext context)
        {
            var adminRoleExists = context.Set<Role>().Any(r => r.Name == "Admin");

            if (!adminRoleExists)
            {
                var adminRole = new Role { Name = "Admin" };

                context.Set<Role>().Add(adminRole);
                context.SaveChanges();
            }
        }

        private void SeedAdminUser(ForgeDbContext context)
        {
            const string adminLogin = "Admin";
            const string adminPassword = "powerAdmin";

            var adminExists = context.Set<User>().Any(u => u.Login == adminLogin);

            if (adminExists)
                return;

            var adminRole = context.Set<Role>().First(r => r.Name == "Admin");

            var admin = new User
            {
                Login = adminLogin,
                PasswordHash = _passwordService.Hash(adminPassword),
                Name = "Admin User",
                Roles = new List<Role> { adminRole }
            };

            context.Set<User>().Add(admin);
            context.SaveChanges();
        }

    }
}
