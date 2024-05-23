using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using UserManagement.API.Models;
using UserManagement.Data.Models;
using UserManagement.Service.Models;
using UserManagement.Service.Models.Authentication.Login;
using UserManagement.Service.Models.Authentication.SignUp;
using UserManagement.Service.Models.Authentication.User;
using UserManagement.Service.Services;


namespace UserManagement.API.Controllers
{
    [Route("api/Authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IUserManagement _user;

        public AuthenticationController(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, IEmailService emailService, IConfiguration configuration1
            , SignInManager<ApplicationUser> signInManager, IUserManagement user)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _configuration = configuration1;
            _user = user;
        }

        [HttpPost]
        [Route("registration")]
        public async Task<IActionResult> Register([FromBody] RegisterUser registerUser)
        {
            var tokenResponse = await _user.CreateUserWithTokenAsync(registerUser);
            if (tokenResponse.IsSuccess && tokenResponse.Response != null)
            {
                await _user.AssignRoleToUserAsync(registerUser.Roles!, tokenResponse.Response.User);
                var confirmationLink = Url.Action("ConfirmEmail", "Authentication",
                                  new { tokenResponse.Response.Token, email = registerUser.Email! }, Request.Scheme);

                var message = new Message(new string[] { registerUser.Email! },
                              "Confirmation email link", confirmationLink!);
                _emailService.SendEmail(message);
                return StatusCode(StatusCodes.Status200OK,
                        new Response { Status = "Success", Message = "Email Verified Successfully" , IsSuccess = true });
            }

            return StatusCode(StatusCodes.Status500InternalServerError,
                  new Response { Message = tokenResponse.Message, IsSuccess = false });
        }
        
        
        [HttpGet]
        [Route("confirm-email")] 
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK,
                                      new Response { Status = "Success", Message = "Email confirmed successfully!" });
                }

            }
            return StatusCode(StatusCodes.Status404NotFound,
                                      new Response { Status = "Error", Message = "This User Doesn't Exist" });

        }

       [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            var loginOtpResponse=await _user.GetOtpByLoginAsync(loginModel);

            if (loginOtpResponse.Response!=null)
            {
                var user = loginOtpResponse.Response.User;
                if (await _userManager.CheckPasswordAsync(user, loginModel.Password)) {
                    if (user.TwoFactorEnabled)
                    {
                        var token = loginOtpResponse.Response.Token;
                        var message = new Message(new string[] { user.Email! }, "OTP Confrimation", token);
                        _emailService.SendEmail(message);

                        return StatusCode(StatusCodes.Status200OK,
                            new Response
                            {
                                IsSuccess = loginOtpResponse.IsSuccess,
                                Status = "Success",
                                Message = $"We have sent an OTP to your Email {user.Email}"
                            });
                    }
                    if (user != null)
                    {
                        var serviceResponse = await _user.GetJwtTokenAsync(user);
                        return Ok(serviceResponse);

                    }
                }
            }
            return Unauthorized();
        }//end of Login

        [HttpPost]
        [Route("login-2FA")]
        public async Task<IActionResult> LoginWithOTP(string code, string userName)
        {
            var jwt = await _user.LoginUserWithJWTokenAsync(code, userName);
            if (jwt.IsSuccess)
            {
                return Ok(jwt);
            }
            return StatusCode(StatusCodes.Status404NotFound,
                new Response { Status = "Success", Message = $"Invalid Code" });

        }

        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken(LoginResponse tokens)
        {
            var jwt = await _user.RenewAccessTokenAsync(tokens);
            if (jwt.IsSuccess)
            {
                return Ok(jwt);
            }
            return StatusCode(StatusCodes.Status404NotFound,
                new Response { Status = "Success", Message = $"Invalid Code" });
        }

        //reset password
        [HttpPost]
        [AllowAnonymous]
        [Route("forget-password")]
        public async Task<IActionResult> ForgetPassword([Required] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var clientAppUrl = "http://localhost:3000"; // Base URL of your React app
                var encodedToken = Uri.EscapeDataString(token);
                var encodedEmail = Uri.EscapeDataString(user.Email);
                var resetPasswordPath = $"/resetpassword?token={encodedToken}&email={encodedEmail}";

                // Correctly construct the full URL pointing to the React application
                var forgotPasswordLink = $"{clientAppUrl}{resetPasswordPath}";

                var message = new Message(new string[] { user.Email }, "Forgot Password Link", forgotPasswordLink);
                _emailService.SendEmail(message);
                return StatusCode(StatusCodes.Status200OK,
                       new Response { Status = "Success", Message = $"Password Change Request is sent to email {user.Email}" });
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                       new Response { Status = "Error", Message = "Couldn't send link to email, please try again" });
        }

        [HttpGet]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            var model = new ResetPassword { Token = token, Email = email };
            return Ok(new
            {
                model
            });
        } 

        [HttpPost]
        [AllowAnonymous]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPassword resetPassword)
        {
            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (user != null)
            {
                var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);
                if (!resetPassResult.Succeeded)
                {
                    foreach (var error in resetPassResult.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }
                    return Ok(ModelState);
                }
                return StatusCode(StatusCodes.Status200OK,
                       new Response { Status = "Success", Message = "Password reset successfully!" });
            }

            return StatusCode(StatusCodes.Status400BadRequest,
                       new Response { Status = "Error", Message = $"Couldn't send link to email, please try again" });

        } 
        
    }
}
//public async Task<IActionResult> TestEmail()
//{
//    var message = new Message(new string[]
//        { "alaahewwari@gmail.com" }, "Test", "This is the content of the email.");
//    _emailService.SendEmail(message);
//    return Ok("Email Sent Successfully!");
//}