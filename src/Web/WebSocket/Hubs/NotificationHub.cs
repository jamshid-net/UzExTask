using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using ProjectTemplate.Application.Common.Interfaces;
using ProjectTemplate.Application.Common.Models;
using ProjectTemplate.Shared.Constants;

namespace ProjectTemplate.Web.WebSocket.Hubs;

public class NotificationHub(IHubConnectionManager connectionManager) : Hub
{
    public override Task OnConnectedAsync()
    {
        
        var userId = Convert.ToInt32(Context.User?.FindFirstValue(StaticClaims.UserId));

        var userConnectedInfo = GetUserConnectedInfo(Context);

        if(userId > 0)
            connectionManager.AddConnection(userId, userConnectedInfo);
        
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Convert.ToInt32(Context.User?.FindFirstValue(StaticClaims.UserId));
        
        connectionManager.RemoveConnection(userId, Context.ConnectionId);

        return base.OnDisconnectedAsync(exception);
    }

    private UserConnectedInfo GetUserConnectedInfo(HubCallerContext context)
    {
        var request = Context.GetHttpContext()?.Request;
        
        var roleId = Convert.ToInt32(Context.User?.FindFirstValue(StaticClaims.RoleId));
        
        var forwardedHeader = Context.GetHttpContext()?.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        
        var ip = !string.IsNullOrWhiteSpace(forwardedHeader)
            ? forwardedHeader.Split(',')[0] 
            : Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();


        var userConnectedInfo = new UserConnectedInfo
        {
            ConnectionTimeUtc = DateTimeOffset.UtcNow,
            ConnectionId = Context.ConnectionId,
            DeviceName = request?.Headers["User-Agent"].ToString(),
            IpAddress = ip,
            RoleId = roleId,
            Referer = request?.Headers["Referer"].ToString(),
        };

        return userConnectedInfo;
    }
}
