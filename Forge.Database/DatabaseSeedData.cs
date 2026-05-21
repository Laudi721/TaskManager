using System;
using System.Collections.Generic;
using System.Text;
using TaskManager.Common.Interfaces;
using TaskManager.Common.Security;
using TaskManager.Database.Models;

namespace TaskManager.Database
{
    public class DatabaseSeedData
    {
        private readonly IPasswordService _passwordService;

        public DatabaseSeedData(IPasswordService passwordService)
        {
            _passwordService = passwordService;
        }

        public void Seed(TaskManagerDbContext context)
        {
            //permissions
            var permissions = new List<Permissions>
            {
                new Permissions { Name = "CreateTask" },
                new Permissions { Name = "EditTask" },
                new Permissions { Name = "DeleteTask" },
                new Permissions { Name = "ViewTask" },
                new Permissions { Name = "FinishTask" },
                new Permissions { Name = "FailedTask" },
            };

            //roles
            var userPermissions = permissions.Where(p => p.Name == "EditTask" || p.Name == "ViewTask" || p.Name == "FinishTask" || p.Name == "FailedTask").ToList();
            var roles = new List<Role>
            {
                new Role { Name = "Admin", Permissions = permissions },
                new Role { Name = "User", Permissions = userPermissions }
            };

            //users
            var users = new List<User>()
            {
                new User 
                { 
                    Login = "admin", 
                    PasswordHash = _passwordService.Hash("powerAdmin"), 
                    Name = "Admin User", 
                    Roles = new List<Role> { roles.First(a => a.Name == "Admin") 
                    } 
                },
            };

            context.Set<Permissions>().AddRange(permissions);
            context.Set<Role>().AddRange(roles);
            context.Set<User>().AddRange(users);
            context.SaveChanges();
        }
    }
}
