using TimeForge.Common.Enums;
using TimeForge.Models;
using TimeForge.ViewModels.UserConnection;

namespace TimeForge.Services.Interfaces;

public interface IConnectionService
{
    Task SendConnectionAsync(string fromUserId, string toUserEmail);
    
    Task UpdateConnectionAsync(UserConnection userConnection, ConnectionStatus status);
    
    Task<UserConnectionViewModel> GetConnectionsByUserIdAsync(string userId);
}