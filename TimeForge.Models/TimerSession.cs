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
      LastHeartbeat = DateTime.UtcNow;
      LastStartedAt = DateTime.UtcNow;
      State = TimeEntryState.Running;
   }
   public string UserId { get; private set; } = String.Empty;
   public User User { get; private set; } = null!;

   public string TaskId { get; private set; } = String.Empty;
   public ProjectTask Task { get; private set; } = null!;

   public DateTime StartTime { get; private init; }
   public DateTime? EndTime { get; private set; }
   
   public TimeEntryState State { get; private set; }
   public DateTime LastHeartbeat { get; private set; }
   public DateTime LastStartedAt { get; private set; }
   public int TotalSeconds { get; private set; }

   public bool IsActive => State == TimeEntryState.Running;

   public static TimerSession Start(string userId, string taskId)
   {
      return new TimerSession
      {
         UserId = userId,
         TaskId = taskId,
         StartTime = DateTime.UtcNow,
         LastHeartbeat = DateTime.UtcNow,
         LastStartedAt = DateTime.UtcNow,
         State = TimeEntryState.Running
      };
   }

   public void Pause()
   {
      if (State != TimeEntryState.Running)
         return;

      var now = DateTime.UtcNow;
      TotalSeconds += (int)(now - LastStartedAt).TotalSeconds;
      LastHeartbeat = now;
      State = TimeEntryState.Paused;
      this.MarkModified();
   }

   public void Resume()
   {
      if (State != TimeEntryState.Paused)
         return;

      LastStartedAt = DateTime.UtcNow;
      LastHeartbeat = LastStartedAt;
      State = TimeEntryState.Running;
      this.MarkModified();
   }

   public void Heartbeat()
   {
      if (State != TimeEntryState.Running)
         return;

      LastHeartbeat = DateTime.UtcNow;
      this.MarkModified();
   }

   public void Stop()
   {
      if (State == TimeEntryState.Completed)
         throw new InvalidOperationException("Timer session already completed");

      var now = DateTime.UtcNow;
      if (State == TimeEntryState.Running)
      {
         TotalSeconds += (int)(now - LastStartedAt).TotalSeconds;
      }

      EndTime = now;
      LastHeartbeat = now;
      State = TimeEntryState.Completed;
      this.MarkModified();
   }
}