using System.Collections.Concurrent;
using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Application.Common.Models;

namespace ProjectTemplate.Web.WebSocket.Services;

public class HubConnectionManager : IHubConnectionManager
{
    private readonly ConcurrentDictionary<int, List<UserConnectedInfo>> _userConnections = [];
    public void AddConnection(int userId, UserConnectedInfo userConnectedInfo)
    {
        _userConnections.AddOrUpdate(userId, [userConnectedInfo], (_, updateValue) =>
        {
            updateValue.Add(userConnectedInfo);
            return updateValue;
        });
    }

    public void RemoveConnection(int userId, string connectionId)
    {
        if (!_userConnections.TryGetValue(userId, out var connections)) return;

        connections.RemoveAll(c => c.ConnectionId == connectionId);
        if (connections.Count == 0)
        {
            _userConnections.TryRemove(userId, out _);
        }
    }

    public List<string> GetConnections(int userId)
    {
        return _userConnections.TryGetValue(userId, out var connections)
            ? connections.Select(c => c.ConnectionId).ToList()
            : [];
    }

    public List<string> GetConnections(params int[] userIds)
    {
        List<string> result = [];

        foreach (var userId in userIds)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                result.AddRange(connections.Select(c => c.ConnectionId));
            }
        }

        return result;
    }

    public List<string> GetConnectionsByRoleIds(params int[] roleIds)
    {
        return _userConnections
            .Where(kvp => kvp.Value.Any(v => roleIds.Contains(v.RoleId)))
            .SelectMany(x => x.Value.Select(c => c.ConnectionId))
            .ToList();
    }

    public List<UserConnectedInfo> GetOnlineUsers()
    {
        return _userConnections.SelectMany(x => x.Value)
                               .ToList();
    }
}
