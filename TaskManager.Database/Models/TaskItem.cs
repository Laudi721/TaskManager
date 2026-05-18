using System;
using System.Collections.Generic;
using System.Text;
using TaskManager.Database.Enums;

namespace TaskManager.Database.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        /// <summary>
        /// Task Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Task description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Task status
        /// </summary>
        public TaskItemStatus Status { get; set; }

        /// <summary>
        /// Assigned user.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Assigned user id. Can be null
        /// </summary>
        public int? UserId { get; set; }

        /// <summary>
        /// Audit logs
        /// </summary>
        public List<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
