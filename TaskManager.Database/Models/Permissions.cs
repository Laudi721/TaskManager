using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Database.Models
{
    public class Permissions
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<Role> Roles { get; set; } = new List<Role>();
    }
}
