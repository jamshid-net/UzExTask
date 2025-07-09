using ProjectTemplate.Application.Common.Models;

namespace ProjectTemplate.Application.Common.Interfaces;

public interface IHubConnectionManager
{
    void AddConnection(int userId, UserConnectedInfo userConnectedInfo);
    void RemoveConnection(int userId, string connectionId);
    List<string> GetConnections(int userId);
    List<string> GetConnections(params int[] userIds);
    List<string> GetConnectionsByRoleIds(params int[] roleIds);
    List<UserConnectedInfo> GetOnlineUsers();
}
