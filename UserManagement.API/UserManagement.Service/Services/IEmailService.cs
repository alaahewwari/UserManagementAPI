using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.Service.Models;

namespace UserManagement.Service.Services
{
    public interface IEmailService
    {
       public void SendEmail(Message message);
        
    }
}
