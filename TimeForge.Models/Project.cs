using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using TimeForge.Common.Constants;
using TimeForge.Models.Common;
using TaskStatus = TimeForge.Common.Enums.TaskStatus;

namespace TimeForge.Models;

/// <summary>
/// Represents a project entity with tasks and ownership information.
/// </summary>
public class Project : BaseDeletableModel<string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Project"/> class with a new unique identifier.
    /// </summary>
    public Project() : base()
    {
    }

    public string UserId { get; private set; } = String.Empty;

    public User User { get; private set; } = null!;

    public string Name { get; private set; } = String.Empty;

    public string Description { get; private set; } = String.Empty;

    public DateTime? DueDate { get; set; }

    public string Color { get; private set; } = "blue";


    private readonly List<ProjectTask> tasks = new();
    public IReadOnlyCollection<ProjectTask> Tasks => this.tasks.AsReadOnly();

    public ICollection<CalendarEvent> Events { get; private set; } = new List<CalendarEvent>();

    //Computed Properties for UI
    public int TasksDone => this.tasks.Count(t => t.Status == TaskStatus.Done);
    public int TasksTotal => this.tasks.Count;
    public int Progress => TasksTotal == 0
        ? 0
        : (int)((TasksDone / (double)TasksTotal) * 100);


    public static Project Create(
        string ownerId,
        string name,
        string description,
        DateTime? dueDate,
        string color = "blue")
    {
        return new Project
        {
            UserId = ownerId,
            Name = name,
            Description = description,
            DueDate = dueDate,
            Color = color,
        };
    }

    public void Update(
        string name,
         DateTime? dueDate,
         List<ProjectTask> tasks,
        string? description,
        string? color)
    {

        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Project name is required");

        Name = name;
        DueDate = dueDate;
        Description = description ?? Description;
        Color = color ?? Color;

        this.tasks.Clear();
        this.tasks.AddRange(tasks);

        this.MarkModified();
    }

    public ProjectTask AddTask(string taskName)
    {
        var task = ProjectTask.Create(Id, taskName);
        this.tasks.Add(task);
        this.MarkModified();
        return task;
    }

    public void RemoveTask(string taskId)
    {
        var task = this.tasks.FirstOrDefault(t => t.Id == taskId);
        if (task != null)
        {
            this.tasks.Remove(task);
            this.MarkModified();
        }
    }
}