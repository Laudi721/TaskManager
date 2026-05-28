using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Forge.Database.SoftDelete;
using Microsoft.EntityFrameworkCore;

namespace Forge.Database.Models
{
    [Table("Users")]
    public class User : ISoftDeletable
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Login
        /// </summary>
        [Required]
        public string Login { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        [Required]
        public string PasswordHash { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Does it archived
        /// </summary>
        public bool IsArchive { get; set; }

        /// <summary>
        /// Identyfikator wybranego motywu kolorystycznego (np. "azure", "green").
        /// Null = użytkownik nie wybrał, stosuje się domyślny motyw aplikacji.
        /// </summary>
        public string? ThemePreference { get; set; }

        /// <summary>
        /// User roles
        /// </summary>
        public List<Role> Roles { get; set; } = new List<Role>();

        /// <summary>
        /// Assigned tasks
        /// </summary>
        public List<TaskItem> Tasks { get; set; } = new List<TaskItem>();

        /// <summary>
        /// Audit logs
        /// </summary>
        public List<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

        public bool IsDeleted { get; set; }

        public DateTime? DeletedTime { get; set; }
    }
}
