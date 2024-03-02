using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Models;

namespace ProjectCollaborationPlatforn.Security.Interfaces
{
    public interface IUserService
    {
        public Task<bool> AddUser(SignUpDTO user);
        public Task<User> GetUserById(string id);
        Task<bool> SendEmailVerification(User user);
        Task<string> CallBackUrl(User user, string code);
        Task<bool> VerifyEmail(User user, string code);

    }
}
