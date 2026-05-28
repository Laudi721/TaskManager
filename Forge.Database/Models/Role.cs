using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Forge.Database.SoftDelete;
using Microsoft.EntityFrameworkCore;

namespace Forge.Database.Models
{
    [Table("Roles")]
    public class Role : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Task Name
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Task description
        /// </summary>
        public List<User> Users { get; set; } = new List<User>();

        public List<Permissions> Permissions { get; set; } = new List<Permissions>();


        public bool IsArchive { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedTime { get; set; }
    }
}
