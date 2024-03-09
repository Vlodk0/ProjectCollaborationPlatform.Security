using System.ComponentModel.DataAnnotations;

namespace ProjectCollaborationPlatforn.Security.DTOs
{
    public class SignInDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
