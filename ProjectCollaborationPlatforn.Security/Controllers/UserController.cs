using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectCollaborationPlatforn.Security.DTOs;
using ProjectCollaborationPlatforn.Security.Helpers.ErrorFilter;
using ProjectCollaborationPlatforn.Security.Interfaces;
using ProjectCollaborationPlatforn.Security.Services.Autentication;
using System.Security.Claims;

namespace ProjectCollaborationPlatforn.Security.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        //soldier diff. Houst best hog main
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
            if (!await _userService.IsUserExists(userSignInDTO.Email))
            {
                throw new CustomApiException()
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Title = "User not found",
                    Detail = "Error occured while finding user on database"
                };
            }

            if (!await _userService.CheckUserPassword(userSignInDTO.Email, userSignInDTO.Password))
            {
                throw new CustomApiException()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Title = "Bad request",
                    Detail = "Incorrect password!"
                };
            }

            var token = await _userService.GenerateTokens(userSignInDTO.Email);
            if (token == null)
            {
                throw new CustomApiException()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Title = "Server Error",
                    Detail = "Error occured while creating user on server"
                };
            }

            return Ok(token);
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
                throw new CustomApiException()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Title = "Bad request",
                    Detail = "User is already exists"
                };
            }

            var user = await _userService.AddUser(userSignUpDTO);

            if (user)
            {
                return Ok("User created");
            }
            else
            {
                throw new CustomApiException()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Title = "Server Error",
                    Detail = "Error occured while server running"
                };
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
                return Redirect("http://localhost:4200/email-success");
            }
            else
            {
                return Redirect("http://localhost:4200/email-failed");

            }

        }

        [AllowAnonymous]
        [HttpPost]  
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDTO refreshTokenDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _tokenGenerator.RefreshAccessToken(refreshTokenDTO.AccessToken,
                refreshTokenDTO.RefreshToken);

            if (result == null)
            {
                throw new CustomApiException()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Title = "Bad request",
                    Detail = "Token is null"
                };
            }

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("resetcode")]
        public async Task<IActionResult> ResettingCode([FromBody] EmailDTO emailDTO)
        {
            var user = await _userService.IsUserExists(emailDTO.To);

            if (!user)
            {
                throw new CustomApiException()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Title = "Bad request",
                    Detail = "No user exists"
                };
            }

            var userGetResetCode = await _userService.GetUserByEmail(emailDTO.To);
            if (await _userService.SendPasswordResetCode(userGetResetCode))
            {
                return Ok("Reset code has sent!");
            }
            else
            {
                throw new CustomApiException()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Title = "Server Error",
                    Detail = "Error occured while server running"
                };
            }
        }

        [AllowAnonymous]
        [HttpPost("verifyresetcode")]
        public async Task<IActionResult> VerifyingResetCode([FromBody] ResetCodeDTO resetCodeDTO)
        {
            if (!await _userService.IsUserExists(resetCodeDTO.Email))
            {
                throw new CustomApiException()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Title = "Bad request",
                    Detail = "Invalid email or doesn't exists"
                };
            }

            var user = await _userService.GetUserByEmail(resetCodeDTO.Email);
            if (await _userService.VerifyPasswordResetCode(user, resetCodeDTO.ResetCode, resetCodeDTO.NewPassword))
            {
                return Ok("Password reset successfully");
            }
            else
            {
                throw new CustomApiException()
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                    Title = "Server Error",
                    Detail = "Error occured while server running"
                };
            }
        }
    }
}
