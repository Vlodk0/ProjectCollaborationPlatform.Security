using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using MimeKit.Text;
using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Interfaces;
using ProjectCollaborationPlatforn.Security.Models;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using ProjectCollaborationPlatforn.Security.DataAccess;
using ProjectCollaborationPlatforn.Security.Services.Autentication;
using ProjectCollaborationPlatforn.Security.Helpers.ErrorFilter;
using Microsoft.AspNetCore.Mvc;

namespace ProjectCollaborationPlatforn.Security.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ITokenGenerator _tokenGenerator;

        public UserService(UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager,
            ApplicationDbContext context, ITokenGenerator tokenGenerator)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _tokenGenerator = tokenGenerator;
        }


        public async Task<bool> AddUser(SignUpDTO userDTO)
        {
            var user = new User()
            {
                UserName = userDTO.Name,
                Email = userDTO.Email,
            };
            var createdResult = await _userManager.CreateAsync(user, userDTO.Password);
            if (createdResult.Succeeded)
            {
                var createdUser = await _userManager.FindByEmailAsync(user.Email);
                var addRoleResult = await AddUserRole(createdUser, userDTO.RoleName);

                var isSent = await SendEmailVerification(createdUser);
                if (!isSent)
                    await _userManager.DeleteAsync(createdUser);
                return true;
            }
            else
            {
                throw new CustomApiException()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Title = "Error creating user",
                    Detail = "User doesn't created"
                };
            }
        }

        private async Task<string> AddUserRole(User user, string userRole)
        {
            var result = await _userManager.AddToRoleAsync(user, userRole);

            if (result.Succeeded)
            {
                var role = await _userManager.GetRolesAsync(user);
                return role.First();
            }
            else
            {
                throw new CustomApiException()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Title = "Error adding role",
                    Detail = "Role doesn't added"
                };
            }
        }

        private Task<string> CallBackUrl(User user, string code)
        {
            var ngrok = ConstantLink.ngrok;
            var callbackUrl = ngrok + "/api/User/EmailVerification" + $"?userId={user.Id}&code={code}";

            return Task.FromResult(callbackUrl);

        }

        public async Task<bool> CheckUserPassword(string email, string password)
        {
            var userByEmail = await _userManager.FindByEmailAsync(email);
            return await _userManager.CheckPasswordAsync(userByEmail, password);
        }

        public async Task<bool> DeleteUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public Task<bool> IsUserExists(string email)
            => _context.Users.AnyAsync(u => u.Email == email);

        public async Task<UserDTO> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            return new UserDTO()
            {
                Id = user.Id,
                Email = user.Email,
            };
        }

        private async Task<bool> SendEmailVerification(User user)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var callBackUrl = await CallBackUrl(user, code);

            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse("collabro.pcp@gmail.com"));
                email.To.Add(MailboxAddress.Parse(user.Email));
                email.Subject = "Email verification";
                email.Body = new TextPart(TextFormat.Html)
                {
                    Text = $@"<!DOCTYPE html>
<html lang=""en"">
  <head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Email Verification</title>
    <style>
      body {{
        font-family: Arial, sans-serif;
        margin: 0;
        padding: 0;
        background-color: #f5f5f5;
      }}

      table {{
        max-width: 600px;
        margin: 20px auto;
        padding: 30px;
        background-color: #fff;
        border-radius: 5px;
        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
      }}

      th {{
        font-size: 24px;
        color: #333;
        padding: 10px 0;
        text-align: center;
      }}

      td {{
        font-size: 16px;
        color: #666;
        line-height: 1.5;
        padding: 10px;
      }}

      a {{
        text-decoration: none;
        color: #007bff;
        font-weight: bold;
      }}

      .verify-link {{
        display: inline-block;
        padding: 10px 20px;
        background-color: #007bff;
        color: #fff;
        border-radius: 5px;
        transition: all 0.2s ease-in-out;
      }}

      .verify-link:hover {{
        background-color: #0067cc;
      }}
    </style>
  </head>
  <body>
    <table>
      <tr>
        <th colspan=""2"">Welcome to Collabro!</th>
      </tr>
      <tr>
        <td colspan=""2"">Hi, {user.UserName},</td>
      </tr>
      <tr>
        <td colspan=""2"">
          Thank you for joining our amazing community! To ensure a smooth
          experience, please verify your email address by clicking the link
          below:
        </td>
      </tr>
      <tr>
        <td colspan=""2"" style=""text-align: center"">
          <a href=""{callBackUrl}"" class=""verify-link""
            >Verify Your Email</a
          >
        </td>
      </tr>
      <tr>
        <td colspan=""2"">
          Once you verify your email, you'll be able to access exclusive
          features, receive updates, and enjoy all that Collabbro has
          to offer.
        </td>
      </tr>
    </table>
  </body>
</html>
"


                };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync("collabro.pcp@gmail.com", "nqaw axfe aumh ifpr");

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SendPasswordResetCode(UserDTO userDTO)
        {
            var user = new User()
            {
                Email = userDTO.Email,
            };
            var resetCode = await _userManager.GeneratePasswordResetTokenAsync(user);
            var ngrok = ConstantLink.ngrok;
            var callbackUrl = ngrok + "/api/User/VerifyPasswordResetCode" + $"?userId={userDTO.Id}&code={resetCode}";

            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse("collabro.pcp@gmail.com"));
                email.To.Add(MailboxAddress.Parse(userDTO.Email));
                email.Subject = "Reset Password";
                email.Body = new TextPart(TextFormat.Html)
                {
                    Text = $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <title>Project Collaboration Platform: Password Reset</title>
  <style>
    body {{
      font-family: Arial, sans-serif;
      margin: 0;
      padding: 0;
      background-color: #f5f5f5;
    }}

    table {{
      max-width: 600px;
      margin: 20px auto;
      padding: 30px;
      background-color: #fff;
      border-radius: 5px;
      box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
    }}

    th {{
      font-size: 20px;
      color: #333;
      padding: 10px 0;
      text-align: center;
    }}

    td {{
      font-size: 16px;
      color: #666;
      line-height: 1.5;
      padding: 10px;
    }}

    .code {{
      font-size: 18px;
      font-weight: bold;
      background-color: #f0f0f0;
      padding: 5px 10px;
      border-radius: 3px;
    }}

    a {{
      text-decoration: none;
      color: #007bff;
    }}
  </style>
</head>
<body>
  <table>
    <tr>
      <th colspan=""2"">Project Collaboration Platform: Password Reset</th>
    </tr>
    <tr>
      <td colspan=""2"">Hi [Name],</td>
    </tr>
    <tr>
      <td colspan=""2"">You recently requested a password reset for your Project Collaboration Platform account. Your temporary password is:</td>
    </tr>
    <tr>
      <td colspan=""2"">Please use this temporary password to log in and reset your password to a new one you can remember. We recommend choosing a strong password that is unique to this account.</td>
    </tr>
    <tr>
      <td colspan=""2"">Click the link below to reset your password:</td>
    </tr>
    <tr>
      <td colspan=""2"" style=""text-align: center;"">
        <a href=""{callbackUrl}"">Reset Password</a>
      </td>
    </tr>
    <tr>
      <td colspan=""2"">This temporary password will expire in 24 hours. If you don't reset your password within this timeframe, you will need to request a new password reset.</td>
    </tr>
    <tr>
      <td colspan=""2"">If you did not request a password reset, please ignore this email.</td>
    </tr>
    <tr>
      <td colspan=""2"">Sincerely,<br>The Project Collaboration Platform Team</td>
    </tr>
  </table>
</body>
</html>
"
                };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync("collabro.pcp@gmail.com", "nqaw axfe aumh ifpr");

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> VerifyEmail(UserDTO user, string code)
        {
            var userVerify = await _userManager.FindByEmailAsync(user.Email);
            var result = await _userManager.ConfirmEmailAsync(userVerify, code);

            return result.Succeeded;
        }

        public async Task<bool> VerifyPasswordResetCode(UserDTO userDTO, string password, string newPassword)
        {

            var user = await _userManager.FindByEmailAsync(userDTO.Email);
            var result = await _userManager.ResetPasswordAsync(user, password, newPassword);

            return result.Succeeded;
        }

        public async Task<UserDTO> GetUserByEmail(string email)
        {
            var user = await _context.Users
                .Where(i => i.Email == email)
                .Select(u => new UserDTO()
                {
                    Id = u.Id,
                    Email = u.Email,
                    UserName = u.UserName,
                })
                .FirstOrDefaultAsync();
            return user;
        }

        public async Task<AuthenticationResponse> GenerateTokens(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return await _tokenGenerator.GenerateTokens(user);
        }
    }
}
