using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TaskManager.Database.Models
{
    [Table("Roles")]
    public class Role
    {
        public int Id { get; set; }

        /// <summary>
        /// Task Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Task description
        /// </summary>
        public List<User> Users { get; set; } = new List<User>();

        public List<Permissions> Permissions { get; set; } = new List<Permissions>();
    }
}
