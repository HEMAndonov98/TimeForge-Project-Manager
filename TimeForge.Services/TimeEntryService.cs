using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using TimeForge.Common.Enums;
using TimeForge.Infrastructure.Repositories.Interfaces;
using TimeForge.Models;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.TimeEntry;

namespace TimeForge.Services;

/// <summary>
/// Service for managing time entries, including start, pause, resume, stop, and retrieval operations.
/// </summary>
public class TimeEntryService : ITimeEntryService
{
    private readonly ITimeForgeRepository timeForgeRepository;
    private readonly UserManager<User> userManager;
    private readonly ILogger<TimeEntryService> logger;

    /// <summary>
/// Initializes a new instance of the <see cref="TimeEntryService"/> class.
/// </summary>
/// <param name="timeForgeRepository">The repository for data access.</param>
/// <param name="userManager">The user manager instance.</param>
/// <param name="logger">The logger instance.</param>
public TimeEntryService(ITimeForgeRepository timeForgeRepository,
        UserManager<User> userManager,
        ILogger<TimeEntryService> logger)
    {
        this.timeForgeRepository = timeForgeRepository;
        this.userManager = userManager;
        this.logger = logger;
        
        this.logger.LogInformation("Initializing TimeEntryService with timeForgeRepository");
    }
    
/// <summary>
/// Starts a new time entry for a specific task and user.
/// </summary>
/// <param name="taskId">The task ID.</param>
/// <param name="userId">The user ID.</param>
public async Task StartEntryAsync(string taskId, string userId)
    {
        try
        {
            //Check if a task exists and a user with this id exists
            var task = await this.timeForgeRepository
                .All<ProjectTask>(pt => pt.Id == taskId)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            var user = await this.userManager.FindByIdAsync(userId);

            if (task == null || user == null)
            {
                this.logger.LogError("Task with ID: {TaskId} or user with ID: {UserId} does not exist", taskId, userId);
                throw new InvalidOperationException("Task with ID does not exist");
            }

            if (task.IsCompleted)
            {
                this.logger.LogError("Task with ID: {TaskId} is completed", taskId);
                throw new InvalidOperationException("Task with ID is completed");
            }

            //Task and User exists and Task is not completed
            
            //Sanity check: if there are any currently running Timers for this user
            var runningTimer = await this.timeForgeRepository
                .All<TimeEntry>(te => te.UserId == userId && te.State == TimeEntryState.Running)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (runningTimer != null)
            {
                await this.PauseEntryAsync(runningTimer.Id);
            }
            

            //Create a TimeEntry class and populate with data

            TimeEntry newTimeEntry = new TimeEntry()
            {
                Start = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                TaskId = taskId,
                UserId = userId
            };

            //Add TimeEntry to the database

            await this.timeForgeRepository.AddAsync(newTimeEntry);
            await this.timeForgeRepository.SaveChangesAsync();
            this.logger.LogInformation("Successfully created a new time entry for task with ID: {TaskId}", taskId);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception)
        {
            this.logger.LogError("Error occurred while creating a new time entry for task with ID: {TaskId}", taskId);
            throw;
        }
    }

/// <summary>
/// Resumes a paused time entry for a user.
/// </summary>
/// <param name="entryId">The time entry ID.</param>
/// <param name="userId">The user ID.</param>
public async Task ResumeEntryAsync(string entryId, string userId)
    {
        try
        {
            var timeEntry = await this.ValidateTimeEntryAsync(entryId);

            //Check if the Timer was paused before resuming
            if (timeEntry.State != TimeEntryState.Paused || !timeEntry.LastPausedAt.HasValue)
            {
                this.logger.LogError("TimeEntry with ID: {EntryId} is not paused", entryId);
                throw new InvalidOperationException("TimeEntry with ID is not paused");
            }
            
            this.logger.LogInformation("Resuming time entry with ID: {EntryId}", entryId);


            //Sanity check: if there are any currently running Timers for this user
            var runningTimer = await this.timeForgeRepository
                .All<TimeEntry>(te => te.UserId == userId && te.State == TimeEntryState.Running)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (runningTimer != null)
            {
                await this.PauseEntryAsync(runningTimer.Id);
            }
            
            var totalPausedDuration = (DateTime.UtcNow - timeEntry.LastPausedAt!.Value).TotalMilliseconds;
            var totalPausedDurationTimeSpan = TimeSpan.FromMilliseconds(totalPausedDuration);

            timeEntry.State = TimeEntryState.Running;
            timeEntry.TotalPausedDuration += totalPausedDurationTimeSpan;
            timeEntry.LastPausedAt = null;
            timeEntry.LastModified = DateTime.UtcNow;

            this.timeForgeRepository.Update(timeEntry);
            await this.timeForgeRepository.SaveChangesAsync();

            this.logger.LogInformation("Successfully resumed time entry with ID: {EntryId}", entryId);
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception)
        {
            this.logger.LogError("Error occurred while resuming time entry with ID: {EntryId}", entryId);
            throw;
        }
    }

/// <summary>
/// Pauses a running time entry.
/// </summary>
/// <param name="entryId">The time entry ID.</param>
public async Task PauseEntryAsync(string entryId)
    {
        try
        {
            var timeEntry = await this.ValidateTimeEntryAsync(entryId);
            
            //Check if TimeEntry is currently running
            if (timeEntry.State != TimeEntryState.Running)
            {
                this.logger.LogError("TimeEntry with ID: {EntryId} is not running", entryId);
                throw new InvalidOperationException("TimeEntry with ID is not running");
            }
            this.logger.LogInformation("Pausing time entry with ID: {EntryId}", entryId);
            
            timeEntry.State = TimeEntryState.Paused;
            timeEntry.LastPausedAt = DateTime.UtcNow;
            timeEntry.LastModified = DateTime.UtcNow;
            
            this.timeForgeRepository.Update(timeEntry);
            await this.timeForgeRepository.SaveChangesAsync();
            this.logger.LogInformation("Successfully paused time entry with ID: {EntryId}", entryId);
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception)
        {
            this.logger.LogError("Error occurred while pausing time entry with ID: {EntryId}", entryId);
            throw;
        }
    }

/// <summary>
/// Stops a running time entry.
/// </summary>
/// <param name="entryId">The time entry ID.</param>
public async Task StopEntryAsync(string entryId)
    {
        try
        {
            var timeEntry = await ValidateTimeEntryAsync(entryId);

            if (timeEntry.State == TimeEntryState.Paused && timeEntry.LastPausedAt.HasValue)
            {
                timeEntry.TotalPausedDuration = DateTime.UtcNow - timeEntry.LastPausedAt.Value;
                timeEntry.LastPausedAt = null;
            }

            timeEntry.State = TimeEntryState.Completed;
            timeEntry.End = DateTime.UtcNow;
            timeEntry.LastModified = DateTime.UtcNow;

            this.timeForgeRepository.Update(timeEntry);
            await this.timeForgeRepository.SaveChangesAsync();
            this.logger.LogInformation("Successfully stopped time entry with ID: {EntryId}", entryId);
        }
        catch (ArgumentNullException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception)
        {
            this.logger.LogError("Error occurred while stopping time entry with ID: {EntryId}", entryId);
            throw;
        }
    }


/// <summary>
/// Retrieves the current running time entry for a user, if any.
/// </summary>
/// <param name="userId">The user ID.</param>
/// <returns>The running time entry view model, or null if none exists.</returns>
public async Task<TimeEntryViewModel?> GetCurrentRunningTimeEntryByUserIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(userId, "User ID cannot be null or empty");

        string createdBy = (await this.userManager.FindByIdAsync(userId))?.UserName ?? string.Empty;

        var timeEntry = await this.timeForgeRepository
            .All<TimeEntry>(te => te.UserId == userId &&
                                  te.State == TimeEntryState.Running)
            .Include(te => te.ProjectTask)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (timeEntry == null)
            return null;

        var viewModel = new TimeEntryViewModel()
        {
            Id = timeEntry.Id,
            Start = timeEntry.Start,
            End = timeEntry.End,
            TaskName = timeEntry.ProjectTask.Name,
            Duration = timeEntry.Duration ?? TimeSpan.Zero,
            State = timeEntry.State,
            CreatedBy = createdBy,
        };
        return viewModel;
    }

/// <summary>
/// Retrieves all paused time entries for a user.
/// </summary>
/// <param name="userId">The user ID.</param>
/// <returns>A collection of paused time entry view models.</returns>
public async Task<IEnumerable<TimeEntryViewModel>> GetCurrentPausedTimeEntryByUserIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                this.logger.LogError("User ID is null or empty");
                throw new ArgumentNullException(userId, "User ID cannot be null or empty");
            }
            
            this.logger.LogInformation("Retrieving current paused time entry for user with ID: {UserId}", userId);

            
            var userName = (await this.userManager.FindByIdAsync(userId))?.UserName ?? string.Empty;
            var pausedTimeEntries = await this.timeForgeRepository
                .All<TimeEntry>(te => te.UserId == userId &&
                                      te.State == TimeEntryState.Paused)
                .Include(te => te.ProjectTask)
                .AsNoTracking()
                .Select(te => new TimeEntryViewModel()
                {
                    Id = te.Id,
                    Start = te.Start,
                    End = te.End,
                    TaskName = te.ProjectTask.Name,
                    Duration = te.Duration ?? TimeSpan.Zero,
                    State = te.State,
                    CreatedBy = userName
                })
                .ToListAsync();

            if (pausedTimeEntries.Count == 0)
            { 
                this.logger.LogInformation("No paused time entries found for user with ID: {UserId}", userId);
                return pausedTimeEntries;
            }
            
            this.logger.LogInformation("Successfully retrieved current paused time entry for user with ID: {UserId}", userId);
            return pausedTimeEntries;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task<TimeEntry> ValidateTimeEntryAsync(string entryId)
    {
        //Check if ID is valid
        if (string.IsNullOrEmpty(entryId))
        {
            this.logger.LogError("Entry ID is null or empty");
            throw new ArgumentNullException(entryId, "Entry ID cannot be null or empty");
        }
            
        //Check if the TimeEntry exists
        var timeEntry = await this.timeForgeRepository
            .All<TimeEntry>(te => te.Id == entryId)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (timeEntry == null)
        {
            this.logger.LogError("TimeEntry with ID: {EntryId} does not exist", entryId);
            throw new InvalidOperationException("TimeEntry with ID does not exist");
        }

        return timeEntry;
    }
}