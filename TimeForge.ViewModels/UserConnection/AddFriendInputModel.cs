using System.ComponentModel.DataAnnotations;

using TimeForge.Common.GlobalErrorMessages;

namespace TimeForge.ViewModels.UserConnection;

public class AddFriendInputModel
{
    [Required(ErrorMessage = UserConnectionErrorMessages.EmailAddressIsRequired)]
    [EmailAddress(ErrorMessage = UserConnectionErrorMessages.InvalidEmailAddress)]
    public string Email { get; set; }


    public string SenderId { get; set; }
}