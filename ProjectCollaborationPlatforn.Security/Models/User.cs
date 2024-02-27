using Microsoft.AspNetCore.Identity;

namespace ProjectCollaborationPlatforn.Security.Models
{
    public class User : IdentityUser<Guid>
    {
        public string? RefreshToken { get; set; }
    }
}
