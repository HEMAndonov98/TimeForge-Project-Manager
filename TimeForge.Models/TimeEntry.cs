using System.ComponentModel.DataAnnotations.Schema;
using TimeForge.Common.Enums;
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

   public TimeEntryState State { get; set; } = TimeEntryState.Running;

   public DateTime? LastPausedAt { get; set; }
   
   public TimeSpan TotalPausedDuration { get; set; } = TimeSpan.Zero;

   [NotMapped]
   public TimeSpan? Duration
   {
      get
      {
         if (End.HasValue)
         {
            return (End.Value - Start) - TotalPausedDuration;
         }

         if (State == TimeEntryState.Running && LastPausedAt.HasValue)
         {
            return (LastPausedAt.Value - Start) - TotalPausedDuration;
         }

         if (State == TimeEntryState.Running)
         {
            return (DateTime.UtcNow - Start) - TotalPausedDuration;
         }

         return null;
      }
   }

   [ForeignKey(nameof(ProjectTask))]
   public string TaskId { get; set; } = null!;

   public ProjectTask ProjectTask { get; set; } = null!;

   [ForeignKey(nameof(CreatedBy))]
   public string UserId { get; set; } = null!;

   public User CreatedBy { get; set; } = null!;
}