using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace ProjectTemplate.Shared.Constants;
public class AuthOptions
{
    public const int ExpireMinutes = 120;
    public const int ExpireMinutesRefresh = 360; 
    public const string Issuer = "AuthServer"; 
    public const string Audience = "AuthClient"; 
    public const int MaxDeviceCount = 3;
    const string SecretKey = "YourSuperSecureKey_ThatIsLongEnough!";
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new(Encoding.UTF8.GetBytes(SecretKey));
}
