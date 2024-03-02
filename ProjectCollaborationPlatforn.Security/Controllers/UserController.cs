using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Interfaces;
using ProjectCollaborationPlatforn.Security.Models;
using ProjectCollaborationPlatforn.Security.Services.Autentication;

namespace ProjectCollaborationPlatforn.Security.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
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

        [HttpPost]
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

        [HttpPost]
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
                    var usr = await _userManager.FindByEmailAsync(userSignUpDTO.Email);

                    if (await _userService.SendEmailVerification(usr))
                    {
                        return StatusCode(StatusCodes.Status200OK);
                    }
                    //return StatusCode(StatusCodes.Status200OK, 
                    //    await _tokenGenerator.GenerateTokens(await _userManager.FindByEmailAsync(userSignUpDTO.Email)));
                }

            }
            catch (Exception) { }

            return StatusCode(StatusCodes.Status500InternalServerError, "Error occured while creating user on server");
        }

        [HttpGet]
        public async Task<IActionResult> VerificateEmail([FromQuery] string userId, [FromQuery] string code)
        {
            if (userId == null || code == null)
                return BadRequest(new AuthResponse()
                {
                    Errors = new List<string>()
                    {
                        "Invalid email confirmation url"
                    },
                    Result = false
                });

            var user = await _userService.GetUserById(userId);

            if (user == null)
            {
                return BadRequest(new AuthResponse()
                {
                    Errors = new List<string>()
                    {
                        "Invalid email parameter"
                    },
                    Result = false
                });
            }

            code = code.Replace(' ', '+');

            if (await _userService.VerifyEmail(user, code))
            {
                return Content(VerifiedMessage.SuccessMessage, "text/html");
            }
            else
            {
                return Content(VerifiedMessage.FailureMessage, "text/html");
            }

        }
    }
}
