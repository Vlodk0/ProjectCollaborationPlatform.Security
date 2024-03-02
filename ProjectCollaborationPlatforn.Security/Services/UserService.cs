using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using MimeKit.Text;
using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Interfaces;
using ProjectCollaborationPlatforn.Security.Models;
using MailKit.Net.Smtp;

namespace ProjectCollaborationPlatforn.Security.Services
{
    public class UserService : IUserService
    {
        readonly UserManager<User> _userManager;
        readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public UserService(UserManager<User> userManager, RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
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
                var addRoleResult = await _userManager.AddToRoleAsync(createdUser, userDTO.RoleName);
                if (!addRoleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(createdUser);
                }
                return addRoleResult.Succeeded;
            }
            else
            {
                return false;
            }
        }

        public Task<string> CallBackUrl(User user, string code)
        {
            var ngrok = ConstantLink.ngrok;
            var callbackUrl = ngrok + "/api/User/VerificateEmail" + $"?userId={user.Id}&code={code}";

            return Task.FromResult(callbackUrl);

        }

        public async Task<User> GetUserById(string id)
        {
            return await _userManager.FindByIdAsync(id);

        }

        public async Task<bool> SendEmailVerification(User user)
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
        <th colspan=""2"">Welcome to [Your Company Name]!</th>
      </tr>
      <tr>
        <td colspan=""2"">Hi {user.UserName},</td>
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
          features, receive updates, and enjoy all that [Your Company Name] has
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
                await smtp.AuthenticateAsync("collabro.pcp@gmail.com", "ntkc fqfa ogwn zclj");

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> VerifyEmail(User user, string code)
        {
            var result = await _userManager.ConfirmEmailAsync(user, code);

            return result.Succeeded ? true : false;
        }
    }
}
