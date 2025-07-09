namespace ProjectTemplate.Application.Common.Interfaces;

public interface ICurrentUser
{
    int? Id { get; }
    int? RoleId { get; }
}
