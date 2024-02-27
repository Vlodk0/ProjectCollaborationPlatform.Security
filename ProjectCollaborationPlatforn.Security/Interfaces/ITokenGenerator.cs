using ProjectCollaborationPlatforn.Security.Models;
using ProjectCollaborationPlatforn.Security.Services.Autentication;

namespace ProjectCollaborationPlatforn.Security.Interfaces
{
    public interface ITokenGenerator
    {
        Task<AuthenticationResponse> GenerateTokens(User user);
    }
}
