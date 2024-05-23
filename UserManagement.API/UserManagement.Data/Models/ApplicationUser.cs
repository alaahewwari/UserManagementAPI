﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserManagement.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }


    }
}
