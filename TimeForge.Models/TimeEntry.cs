using System.ComponentModel.DataAnnotations.Schema;
using TimeForge.Models.Common;

namespace TimeForge.Models;

public class TimeEntry : BaseDeletableModel<string>
{
   public DateTime Start { get; set; }

   public DateTime End { get; set; }

   public double Duration => (this.End - this.Start).TotalHours;

   [ForeignKey(nameof(Task))]
   public string TaskId { get; set; }

   public Task Task { get; set; }

   [ForeignKey(nameof(CreatedBy))]
   public string UserId { get; set; }

   public User CreatedBy { get; set; }
}