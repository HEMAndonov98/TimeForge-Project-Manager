using System.ComponentModel.DataAnnotations.Schema;

using TimeForge.Common.Enums;
using TimeForge.Models.Common;

namespace TimeForge.Models;

/// <summary>
/// Represents a time entry for a project task, including timing and state tracking.
/// </summary>
public class TimeEntry : BaseDeletableModel<string>
{
   /// <summary>
/// Initializes a new instance of the <see cref="TimeEntry"/> class with a new unique identifier.
/// </summary>
public TimeEntry()
   {
      this.Id = Guid.NewGuid().ToString();
   }
   
/// <summary>
/// Gets or sets the start time of the time entry.
/// </summary>
public DateTime Start { get; set; }

/// <summary>
/// Gets or sets the end time of the time entry.
/// </summary>
public DateTime? End { get; set; }

/// <summary>
/// Gets or sets the current state of the time entry.
/// </summary>
public TimeEntryState State { get; set; } = TimeEntryState.Running;

/// <summary>
/// Gets or sets the last paused time of the time entry.
/// </summary>
public DateTime? LastPausedAt { get; set; }
   
/// <summary>
/// Gets or sets the total duration for which the time entry was paused.
/// </summary>
public TimeSpan TotalPausedDuration { get; set; } = TimeSpan.Zero;

/// <summary>
/// Gets the calculated duration of the time entry, excluding paused time.
/// </summary>
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

/// <summary>
/// Gets or sets the task ID this time entry is associated with.
/// </summary>
[ForeignKey(nameof(ProjectTask))]
public string TaskId { get; set; } = null!;

/// <summary>
/// Gets or sets the project task associated with this time entry.
/// </summary>
public ProjectTask ProjectTask { get; set; } = null!;

/// <summary>
/// Gets or sets the user ID who created the time entry.
/// </summary>
[ForeignKey(nameof(CreatedBy))]
public string UserId { get; set; } = null!;

/// <summary>
/// Gets or sets the user who created the time entry.
/// </summary>
public User CreatedBy { get; set; } = null!;
}