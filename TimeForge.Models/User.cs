using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TimeForge.Models;

public class User : IdentityUser
{
    [InverseProperty(nameof(Project.CreatedBy))]
    public virtual List<Project> Projects { get; set; } = new();

    [InverseProperty(nameof(Tag.CreatedBy))]
    public virtual List<Tag> Tags { get; set; } = new();

    [InverseProperty(nameof(TimeEntry.User))]
    public virtual List<TimeEntry> TimeEntries { get; set; } = new();
}