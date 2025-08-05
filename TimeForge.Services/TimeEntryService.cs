using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeForge.Common.Enums;
using TimeForge.Infrastructure.Repositories.Interfaces;
using TimeForge.Models;
using TimeForge.Services.Interfaces;
using TimeForge.ViewModels.TimeEntry;

namespace TimeForge.Services;

public class TimeEntryService : ITimeEntryService
{
    private readonly ITimeForgeRepository timeForgeRepository;
    private readonly UserManager<User> userManager;
    private readonly ILogger<TimeEntryService> logger;

    public TimeEntryService(ITimeForgeRepository timeForgeRepository,
        UserManager<User> userManager,
        ILogger<TimeEntryService> logger)
    {
        this.timeForgeRepository = timeForgeRepository;
        this.userManager = userManager;
        this.logger = logger;
        
        this.logger.LogInformation("Initializing TimeEntryService with timeForgeRepository");
    }
    
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

    public async Task ResumeEntryAsync(string entryId)
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

            timeEntry.State = TimeEntryState.Running;
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
            timeEntry.TotalPausedDuration += DateTime.UtcNow - timeEntry.LastPausedAt.Value;
            
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