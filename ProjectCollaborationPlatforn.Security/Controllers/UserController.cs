using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Interfaces;
using ProjectCollaborationPlatforn.Security.Services.Autentication;
using System.Security.Claims;

namespace ProjectCollaborationPlatforn.Security.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        readonly IUserService _userService;
        readonly ITokenGenerator _tokenGenerator;

        public UserController(IUserService userService, ITokenGenerator tokenGenerator)
        {
            _userService = userService;
            _tokenGenerator = tokenGenerator;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn([FromBody] SignInDTO userSignInDTO)
        {
            if (await _userService.IsUserExists(userSignInDTO.Email))
            {
                return StatusCode(StatusCodes.Status404NotFound, "No user exists");
            }

            if (!await _userService.CheckUserPassword(userSignInDTO.Email, userSignInDTO.Password))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Incorrect password!");
            }

            var token = await _userService.GenerateTokens(userSignInDTO.Email);
            if (token == null)
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
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);
            }

            if (await _userService.IsUserExists(userSignUpDTO.Email))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "User is already exists!");
            }

            var user = await _userService.AddUser(userSignUpDTO);

            if (user != null)
            {
                return Created("/api/user", user);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error occured while creating user on server");
            }

        }

        [HttpGet]
        public async Task<IActionResult> EmailVerification([FromQuery] string userId, [FromQuery] string code)
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

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO refreshTokenDTO)
        {
            if (!ModelState.IsValid)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            var result = await _tokenGenerator.RefreshAccessToken(refreshTokenDTO.AccessToken,
                refreshTokenDTO.RefreshToken);

            if (result == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("resetcode")]
        public async Task<IActionResult> ResettingCode([FromBody] EmailDTO emailDTO)
        {
            var id = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userService.IsUserExists(emailDTO.To);

            if (!user)
            {
                return StatusCode(StatusCodes.Status400BadRequest, "User doesn't exists");
            }

            var userGetResetCode = await _userService.GetUserById(id);
            if (await _userService.SendPasswordResetCode(userGetResetCode))
            {
                return StatusCode(StatusCodes.Status200OK, "Reset code has sent!");
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Server error");
            }
        }

        [AllowAnonymous]
        [HttpPost("verifyresetcode")]
        public async Task<IActionResult> VerifyingResetCode([FromBody] ResetCodeDTO resetCodeDTO)
        {
            if (!await _userService.IsUserExists(resetCodeDTO.Email))
            {
                return StatusCode(StatusCodes.Status400BadRequest, "Invalid email or doesn't exists");
            }

            var user = await _userService.GetUserByEmail(resetCodeDTO.Email);
            if (await _userService.VerifyPasswordResetCode(user, resetCodeDTO.ResetCode, resetCodeDTO.NewPassword))
            {
                return StatusCode(StatusCodes.Status200OK, "Password reset successfully");
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Server error");
            }
        }
    }
}
