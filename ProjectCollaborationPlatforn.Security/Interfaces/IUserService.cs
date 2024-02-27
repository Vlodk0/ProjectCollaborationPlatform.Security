using ProjectCollaborationPlatforn.Security.DTOs;

namespace ProjectCollaborationPlatforn.Security.Interfaces
{
    public interface IUserService
    {
        public Task<bool> AddUser(SignUpDTO user);
    }
}
