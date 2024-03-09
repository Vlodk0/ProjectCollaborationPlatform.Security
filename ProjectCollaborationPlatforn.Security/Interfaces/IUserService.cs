using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Models;
using ProjectCollaborationPlatforn.Security.Services.Autentication;

namespace ProjectCollaborationPlatforn.Security.Interfaces
{
    public interface IUserService
    {
        public Task<UserDTO> AddUser(SignUpDTO user);
        public Task<bool> DeleteUser(string email);
        public Task<UserDTO> GetUserById(string id);
        public Task<UserDTO> GetUserByEmail(string email);
        public Task<AuthenticationResponse> GenerateTokens(string email);
        Task<bool> IsUserExists(string email);
        Task<bool> VerifyEmail(UserDTO user, string code);
        Task<bool> SendPasswordResetCode(UserDTO user);
        Task<bool> VerifyPasswordResetCode(UserDTO user, string code, string newPassword); 
        Task<bool> CheckUserPassword(string email, string password);

    }
}
