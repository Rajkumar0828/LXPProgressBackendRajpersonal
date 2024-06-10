using LXP.Common.ViewModels;
using LXP.Core.IServices;
using LXP.Core.IServices.LXP.Core.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.Mail;

namespace LXP.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly EmailSettings _emailSettings;
        private static ConcurrentDictionary<string, (string OTP, DateTime Timestamp)> emailOtpMap = new ConcurrentDictionary<string, (string OTP, DateTime Timestamp)>();

        public EmailController(IEmailService emailService, IOptions<EmailSettings> emailSettings)
        {
            _emailService = emailService;
            _emailSettings = emailSettings.Value;
        }

        private (string SmtpServer, int Port) GetSmtpSettings(string email)
            
        {
            
            //Determine the SMTP settings based on the email domain
            var domain = email.Split('@')[1];
            
            if (domain.Equals("gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                return (_emailSettings.GmailSmtpServer, _emailSettings.GmailSmtpPort);
            }
            else
            {
                // Default to Outlook SMTP settings for other domains
                return (_emailSettings.OutlookSmtpServer, _emailSettings.OutlookSmtpPort);
            }
        }

        ///<summary>
        /// Generating email to the respective mail they entered
        ///</summary>
        [HttpPost("EmailVerification")]
        public async Task<IActionResult> GenerateOTP([FromBody] string email)
        {
            // Generate the OTP
            string sOTP = _emailService.GenerateOTP();

            // Store the OTP data with the current timestamp
            emailOtpMap[email] = (sOTP, DateTime.Now);

            // Configure email settings
            string subject = "LXP Email Verification";
            string body = $"The OTP to Verify Your Email is: {sOTP}";

            // Get the SMTP settings based on the email domain
            var smtpSettings = GetSmtpSettings(email);

            // Send the email
            bool emailSent = await _emailService.SendEmailAsync(email, subject, body, smtpSettings.SmtpServer, smtpSettings.Port);

            if (emailSent)
            {
                // Return the generated OTP along with a success message
                return Ok(new { Message = "Email sent successfully", Email = email, OTP = sOTP });
            }
            else
            {
                // Handle failure (e.g., return an error response)
                return BadRequest(new { Message = "Error sending email" });
            }
        }


        //private readonly IEmailService _emailService;
        //private static ConcurrentDictionary<string, (string OTP, DateTime Timestamp)> emailOtpMap = new ConcurrentDictionary<string, (string OTP, DateTime Timestamp)>();

        //public EmailController(IEmailService emailService)
        //{
        //    _emailService = emailService;
        //}

        /////<summary>
        ///// Generating email to the respective mail they entered
        /////</summary>
        //[HttpPost("EmailVerification")]
        //public async Task<IActionResult> GenerateOTP([FromBody] string email)
        //{
        //    // Generate the OTP
        //    string sOTP = _emailService.GenerateOTP();

        //    // Store the OTP data with the current timestamp
        //    emailOtpMap[email] = (sOTP, DateTime.Now);

        //    // Configure email settings
        //    string subject = "LXP Email Verification";
        //    string body = $"The OTP to Verify Your Email is: {sOTP}";

        //    // Send the email
        //    bool emailSent = await _emailService.SendEmailAsync(email, subject, body);

        //    if (emailSent)
        //    {
        //        // Return the generated OTP along with a success message
        //        return Ok(new { Message = "Email sent successfully", Email = email, OTP = sOTP });
        //    }
        //    else
        //    {
        //        // Handle failure (e.g., return an error response)
        //        return BadRequest(new { Message = "Error sending email" });
        //    }
        //}

        ///<summary>
        /// Verifying the OTP for the respective email
        ///</summary>
        [HttpPost("VerifyOTP")]
        public IActionResult VerifyOTP([FromBody] OTPVerificationViewModel otpverify)
        {
            if (emailOtpMap.ContainsKey(otpverify.Email))
            {
                var (storedOTP, storedTimestamp) = emailOtpMap[otpverify.Email];
                DateTime currentTimestamp = DateTime.Now;
                TimeSpan timeDifference = currentTimestamp - storedTimestamp;

                // Check if the OTP is still valid (within 2 minutes)
                if (timeDifference.TotalMinutes < 2)
                {
                    if (storedOTP == otpverify.OTP)
                    {
                        emailOtpMap.TryRemove(otpverify.Email, out _);
                        Console.WriteLine($"OTP verified successfully for email: {otpverify.Email}");
                        return Ok("OTP verified successfully!");
                    }
                    else
                    {
                        Console.WriteLine($"Invalid OTP provided for email: {otpverify.Email}");
                        return BadRequest("Invalid OTP provided.");
                    }
                }
                else
                {
                    emailOtpMap.TryRemove(otpverify.Email, out _);
                    Console.WriteLine($"OTP has expired for email: {otpverify.Email}");
                    return BadRequest("OTP has expired.");
                }
            }
            else
            {
                Console.WriteLine($"No OTP data found for the provided email: {otpverify.Email}");
                return BadRequest("No OTP data found for the provided email.");
            }
        }
    }

}

