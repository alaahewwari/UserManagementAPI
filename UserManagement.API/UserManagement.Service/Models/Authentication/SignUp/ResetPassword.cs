using System.ComponentModel.DataAnnotations;

namespace UserManagement.Service.Models.Authentication.SignUp
{
    public class ResetPassword
    {
        [Required]
        public string Password { get; set; }
        [Compare ("Password",ErrorMessage="The password and configuration password do not match")]
        public string ConfirmPassword { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }


    }
}
