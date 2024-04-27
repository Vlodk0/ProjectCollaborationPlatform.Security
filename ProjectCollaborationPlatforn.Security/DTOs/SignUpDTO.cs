using System.ComponentModel.DataAnnotations;

namespace ProjectCollaborationPlatforn.Security.DTOs
{
    public class SignUpDTO
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string RoleName {  get; set; }
    }
}
