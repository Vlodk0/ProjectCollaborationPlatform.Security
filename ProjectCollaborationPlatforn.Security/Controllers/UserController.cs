using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Interfaces;
using ProjectCollaborationPlatforn.Security.Models;

namespace ProjectCollaborationPlatforn.Security.Controllers
{
    public class UserController : ControllerBase
    {
        readonly IUserService _userService;
        readonly ITokenGenerator _tokenGenerator;
        readonly UserManager<User> _userManager;

        public UserController(IUserService userService, ITokenGenerator tokenGenerator, UserManager<User> userManager)
        {
            _userService = userService;
            _tokenGenerator = tokenGenerator;
            _userManager = userManager;
        }

        [HttpPost("signIn")]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn([FromBody] SignInDTO userSignInDTO)
        {
            var user = await _userManager.FindByEmailAsync(userSignInDTO.Email);
            if(user == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, "No user exists");
            }

            var token = await _tokenGenerator.GenerateTokens(user);
            if(token == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occured while creating user on server");
            }
            else
            {
                return StatusCode(StatusCodes.Status200OK, token);
            }
        }

        [HttpPost("signUp")]
        [AllowAnonymous]
        public async Task<IActionResult> SignUp([FromBody] SignUpDTO userSignUpDTO)
        {
            try
            {
                if(!ModelState.IsValid)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, ModelState);
                }

                var user = await _userManager.FindByEmailAsync(userSignUpDTO.Email);
                if(user != null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, "User is already exists!");
                }

                if(await _userService.AddUser(userSignUpDTO))
                {
                    return StatusCode(StatusCodes.Status200OK, 
                        await _tokenGenerator.GenerateTokens(await _userManager.FindByEmailAsync(userSignUpDTO.Email)));
                }
            }
            catch (Exception) { }

            return StatusCode(StatusCodes.Status500InternalServerError, "Error occured while creating user on server");
        }
    }
}
