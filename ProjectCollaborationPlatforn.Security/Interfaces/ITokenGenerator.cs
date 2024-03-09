using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Models;
using ProjectCollaborationPlatforn.Security.Services.Autentication;

namespace ProjectCollaborationPlatforn.Security.Interfaces
{
    public interface ITokenGenerator
    {
        Task<AuthenticationResponse> GenerateTokens(User user);
        Task<AuthenticationResponse> RefreshAccessToken(string accessToken, string refreshToken);
    }
}
