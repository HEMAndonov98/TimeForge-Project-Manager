using Microsoft.AspNetCore.Identity;

namespace TimeForge.Models;

/// <summary>
/// Represents an application user, including projects, time entries, and management relationships.
/// </summary>
public class User : IdentityUser
{
   public string FirstName { get; private set; } = String.Empty;

   public string LastName { get; private set; } = String.Empty;

   public string? AvatarUrl { get; private set; }

   public DateTime CreatedAt { get; private set; }

   public DateTime? LastModified { get; private set; }


   //Soft delete properties
   public bool IsDeleted { get; private set; } = false;
   public DateTime? DeletedAt { get; private set; }

   //Properties for UI
   public string FullName => $"{FirstName} {LastName}";
   public string AvatarInitials => $"{FirstName[0]}{LastName[0]}".ToUpper();

   //Relationships
   public ICollection<Project> OwnedProjects { get; private set; } = new List<Project>();
   public ICollection<CalendarEvent> CalendarEvents { get; private set; } = new List<CalendarEvent>();
   public ICollection<TimerSession> TimerSessions { get; private set; } = new List<TimerSession>();
   public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();


   //Friendships
   public ICollection<Friendship> ReceivedFriendships { get; private set; } = new List<Friendship>();
   public ICollection<Friendship> SentFriendships { get; private set; } = new List<Friendship>();

   //Chat messages
   public ICollection<ChatMessage> SentMessages { get; private set; } = new List<ChatMessage>();
   public ICollection<ChatMessage> ReceivedMessages { get; private set; } = new List<ChatMessage>();

   //business logic
   public static User CreateCustomUser(string firstName, string lastName, string email)
   {
      //validate email
      if (string.IsNullOrEmpty(email))
         throw new ArgumentException("Email is required");

      return new User()
      {
         FirstName = firstName,
         LastName = lastName,
         Email = email.ToLowerInvariant(),
         CreatedAt = DateTime.UtcNow
      };
   }

   public void UpdateProfile(string firstName, string lastName, string? avatarUrl)
   {
      FirstName = firstName;
      LastName = lastName;
      AvatarUrl = avatarUrl;
      LastModified = DateTime.UtcNow;
   }

   public void MarkAsDeleted()
   {
      IsDeleted = true;
      DeletedAt = DateTime.UtcNow;
      LastModified = DateTime.UtcNow;
   }
}