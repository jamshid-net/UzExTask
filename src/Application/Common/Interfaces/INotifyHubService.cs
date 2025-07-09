namespace ProjectTemplate.Application.Common.Interfaces;
public interface INotifyHubService
{
    Task NotifyAllAsync(string message);
    
    Task NotifyByUsersAsync(string message, params int[] userIds);
    
    Task NotifyByRolesAsync(string message, params int[] roleIds);
    
}
