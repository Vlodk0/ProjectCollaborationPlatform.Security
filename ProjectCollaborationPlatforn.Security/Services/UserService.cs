using Microsoft.AspNetCore.Identity;
using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Interfaces;
using ProjectCollaborationPlatforn.Security.Models;

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
                var addRoleResult = await _userManager.AddToRoleAsync(createdUser, "Dev");
                return addRoleResult.Succeeded;
            }
            else
            {
                return false;
            }
        }
    }
}
