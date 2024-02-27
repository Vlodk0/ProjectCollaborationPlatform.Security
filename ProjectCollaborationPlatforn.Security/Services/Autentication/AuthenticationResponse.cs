namespace ProjectCollaborationPlatforn.Security.Services.Autentication
{
    public class AuthenticationResponse
    {
        public string AccessToken { get; set; } 
        public DateTime AccessTokenExpirationTime {  get; set; }
        public string RefreshToken {  get; set; }
    }
}
