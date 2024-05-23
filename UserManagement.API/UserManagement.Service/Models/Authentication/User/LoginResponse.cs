using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Service.Models.Authentication.User
{
    public class LoginResponse
    {
        public JwtToken AccessToken { get; set; } = null!;
        public JwtToken RefreshToken { get; set; }

    }
}
