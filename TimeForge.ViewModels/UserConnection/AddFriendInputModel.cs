using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using TimeForge.Common.GlobalErrorMessages;

namespace TimeForge.ViewModels.UserConnection;

public class AddFriendInputModel
{
    [Required(ErrorMessage = UserConnectionErrorMessages.EmailAddressIsRequired)]
    [EmailAddress(ErrorMessage = UserConnectionErrorMessages.InvalidEmailAddress)]
    public string Email { get; set; }

    [ValidateNever]
    public string SenderId { get; set; }
}