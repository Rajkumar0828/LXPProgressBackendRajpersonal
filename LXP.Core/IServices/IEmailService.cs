using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LXP.Core.IServices
{
    namespace LXP.Core.IServices
    {
        public interface IEmailService
        {
            Task<bool> SendEmailAsync(string recipientEmail, string subject, string body, string smtpServer, int port);
            //Task<bool> SendEmailAsync(string recipientEmail, string subject, string body);
            string GenerateOTP();
        }
    }
}
