using Microsoft.AspNetCore.SignalR;
using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Web.WebSocket.Hubs;

namespace ProjectTemplate.Web.WebSocket.Services;

public class NotifyHubService(IHubContext<NotificationHub> hubContext,
                              IHubConnectionManager connectionManager) : INotifyHubService
{
    public Task NotifyAllAsync(string message)
    {
        return hubContext.Clients.All.SendAsync("ReceiveMessage", message);
    }

    public Task NotifyByUsersAsync(string message, params int[] userIds)
    {
        var connections = connectionManager.GetConnections(userIds);
        return hubContext.Clients.Clients(connections).SendAsync("ReceiveMessage", message);
    }

    public Task NotifyByRolesAsync(string message, params int[] roleIds)
    {
        var connections = connectionManager.GetConnectionsByRoleIds(roleIds);
        return hubContext.Clients.Clients(connections).SendAsync("ReceiveMessage", message);
    }
}
