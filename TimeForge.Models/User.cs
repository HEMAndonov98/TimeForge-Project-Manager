using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TimeForge.Models;

/// <summary>
/// Represents an application user, including projects, tags, and management relationships.
/// </summary>
public class User : IdentityUser
{
    /// <summary>
    /// Gets or sets the list of projects created by the user.
    /// </summary>
    [InverseProperty(nameof(Project.CreatedBy))]
    public virtual List<Project> Projects { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of tags created by the user.
    /// </summary>
    [InverseProperty(nameof(Tag.CreatedBy))]
    public virtual List<Tag> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of time entries created by the user.
    /// </summary>
    [InverseProperty(nameof(TimeEntry.CreatedBy))]
    public virtual List<TimeEntry> TimeEntries { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of projects assigned to the user.
    /// </summary>
    [InverseProperty(nameof(Project.AssignedTo))]
    public virtual List<Project> AssignedProjects { get; set; } = new();


    [InverseProperty(nameof(TaskItem.User))]
    public virtual List<TaskItem> TaskCollections { get; set; } = new();

    // Manager properties

    /// <summary>
    /// Gets or sets a value indicating whether the user is a manager.
    /// </summary>
    public bool IsManager { get; set; }

    /// <summary>
    /// Gets or sets the list of users managed by this user.
    /// </summary>
    public virtual List<User> ManagedUsers { get; set; } = new();

    /// <summary>
    /// Gets or sets the manager's user ID, if applicable.
    /// </summary>
    public string? ManagerId { get; set; } = null!;

    /// <summary>
    /// Gets or sets the manager user entity, if applicable.
    /// </summary>
    public User? Manager { get; set; } = null!;
}