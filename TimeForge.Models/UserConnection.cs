using System.ComponentModel.DataAnnotations.Schema;
using TimeForge.Common.Enums;

namespace TimeForge.Models;
/// <summary>
/// Represents a single friend request
/// </summary>
public class UserConnection
{
    [ForeignKey(nameof(FromUser))]
    public string FromUserId { get; set; }
    
    [ForeignKey(nameof(ToUser))]
    public string ToUserId { get; set; }

    public ConnectionStatus Status { get; set; }

    public User FromUser { get; set; }

    public User ToUser { get; set; }
}