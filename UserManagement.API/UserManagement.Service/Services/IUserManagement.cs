
using Microsoft.AspNetCore.Identity;
using UserManagement.Data.Models;
using UserManagement.Service.Models;
using UserManagement.Service.Models.Authentication.Login;
using UserManagement.Service.Models.Authentication.SignUp;
using UserManagement.Service.Models.Authentication.User;

namespace UserManagement.Service.Services
{
    public interface IUserManagement
    {
        Task<ApiResponse<CreateUserResponse>> CreateUserWithTokenAsync(RegisterUser registerUser);
        Task<ApiResponse<List<string>>> AssignRoleToUserAsync(List<string> roles,ApplicationUser user);
        Task<ApiResponse<LoginOtpRespons>> GetOtpByLoginAsync(LoginModel loginModel);
        Task<ApiResponse<LoginResponse>> GetJwtTokenAsync(ApplicationUser user);
        Task<ApiResponse<LoginResponse>> LoginUserWithJWTokenAsync(string otp, string userName);
        Task<ApiResponse<LoginResponse>> RenewAccessTokenAsync(LoginResponse tokens);
    }
}
