using TimeForge.Common.Enums;
using TimeForge.Models;

namespace TimeForge.Services.Interfaces;

public interface IConnectionService
{
    Task SendConnectionAsync(string fromUserId, string toUserEmail);
    
    Task UpdateConnectionAsync(UserConnection userConnection, ConnectionStatus status);
}