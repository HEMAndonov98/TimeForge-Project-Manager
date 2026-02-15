using System.ComponentModel.DataAnnotations.Schema;

using TimeForge.Common.Enums;
using TimeForge.Models.Common;

namespace TimeForge.Models;

/// <summary>
/// Represents a time entry for a project task, including timing and state tracking.
/// </summary>
public class TimerSession : BaseDeletableModel<string>
{
   public TimerSession() : base()
   {
      StartTime = DateTime.UtcNow;
   }
   public string UserId { get; private set; } = String.Empty;
   public User User { get; private set; } = null!;

   public string TaskId { get; private set; } = String.Empty;
   public ProjectTask Task { get; private set; } = null!;

   public DateTime StartTime { get; private init; }
   public DateTime? EndTime { get; private set; }
   public int DurationInSeconds { get; private set; }

   public bool IsActive => EndTime == null;

   public static TimerSession Start(string userId, string taskId)
   {
      var timerSession = new TimerSession
      {
         UserId = userId,
         TaskId = taskId,
         StartTime = DateTime.UtcNow
      };
      return timerSession;
   }

   public void Stop()
   {
      if (EndTime.HasValue)
         throw new ArgumentException("Timer session already stopped");

      EndTime = DateTime.UtcNow;
      DurationInSeconds = (int)(EndTime.Value - StartTime).TotalSeconds;
      this.MarkModified();
   }
}