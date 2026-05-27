using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Forge.Database.Models
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        public int Id { get; set; }

        public int TaskId { get; set; }
        public TaskItem Task { get; set; }

        public int? UserId { get; set; }
        public User User { get; set; }

        public string Action { get; set; }

        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
