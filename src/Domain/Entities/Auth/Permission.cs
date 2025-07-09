using ProjectTemplate.Domain.Enums;

namespace ProjectTemplate.Domain.Entities.Auth;
public class Permission : BaseEntity
{
    public string Name { get; private set; } = null!;
    public required EnumPermission EnumPermission
    {
        get => _enumPermission;
        set
        {
            _enumPermission = value;
            Name = value.ToString(); 
        }
    }
    private EnumPermission _enumPermission;
    public virtual ICollection<Role> Roles { get; set; } = [];
}
