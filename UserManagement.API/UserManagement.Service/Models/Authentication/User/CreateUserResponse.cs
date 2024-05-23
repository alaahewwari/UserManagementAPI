using Microsoft.AspNetCore.Identity;
using UserManagement.Data.Models;


namespace UserManagement.Service.Models.Authentication.User
{
    public class CreateUserResponse
    {
        public ApplicationUser User { get; set; }
        public string Token { get; set; }
    }
}
