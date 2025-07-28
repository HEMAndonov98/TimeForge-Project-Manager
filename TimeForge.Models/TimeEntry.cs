using System.ComponentModel.DataAnnotations.Schema;
using TimeForge.Models.Common;

namespace TimeForge.Models;

public class TimeEntry : BaseDeletableModel<string>
{
   public TimeEntry()
   {
      this.Id = Guid.NewGuid().ToString();
   }
   
   public DateTime Start { get; set; }

   public DateTime? End { get; set; }
   
   [ForeignKey(nameof(ProjectTask))]
   public string TaskId { get; set; } = null!;

   public ProjectTask ProjectTask { get; set; } = null!;

   [ForeignKey(nameof(CreatedBy))]
   public string UserId { get; set; } = null!;

   public User CreatedBy { get; set; } = null!;

   public bool IsRunning { get; set; }
}