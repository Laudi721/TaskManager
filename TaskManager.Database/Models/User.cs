using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager.Database.Models
{
    public class User
    {
        public int Id { get; set; }

        /// <summary>
        /// Login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Does it archived
        /// </summary>
        public bool IsArchive { get; set; }

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
    }
}
