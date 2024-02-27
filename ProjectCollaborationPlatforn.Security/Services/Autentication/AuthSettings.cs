using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ProjectCollaborationPlatforn.Security.Services.Autentication
{
    public class AuthSettings
    {
        public string SecretKey { get; set; }
        public double AccessTokenExpirationMinutes { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public double RefreshTokenExpirationMinutes { get; set; }
        public SecurityKey GetKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
        }
    }
}
