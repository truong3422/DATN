using System.Net.Mail;
using System.Net;

namespace PoolComVnWebAPI.Common
{
    public class EmailSender : IEmailSender
    {
        public Task SendMailAsync(string email, string username, string verifyCode)
        {
            var mail = "poolcomvn@gmail.com";
            var pw = "ipak mpci yijj mkvv";
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(mail, pw),
            };

            var message = new MailMessage(from: mail, to: email);
            message.Subject = "Verify Code - PoolComVN";
            message.Body = CreateVerifyEmail(username, verifyCode);
            message.IsBodyHtml = true;
            return client.SendMailAsync(message);
        }
        public Task SendMailResetPasswordAsync(string email, string newPassword)
        {
            var mail = "poolcomvn@gmail.com";
            var pw = "ipak mpci yijj mkvv";
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(mail, pw),
            };

            var message = new MailMessage(from: mail, to: email);
            message.Subject = "ResetPassword - PoolComVN";
            message.Body = CreateChangePasswordEmail(email, newPassword);
            message.IsBodyHtml = true;
            return client.SendMailAsync(message);
        }


        private string CreateChangePasswordEmail(string email, string newPassword)
        {
            string controllerDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(
                Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory))));

            string relativePath = Path.Combine("EmailTemplate", "NewPassword.html");
            string filePath = Path.Combine(controllerDirectory, relativePath);
            try
            {
                string body = string.Empty;
                using (StreamReader reader = new StreamReader(filePath))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("{email}", email);
                body = body.Replace("{newPassword}", newPassword);
                return body;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private string CreateVerifyEmail(string username, string verifyCode)
        {
            string controllerDirectory = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(
                Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory))));

            string relativePath = Path.Combine("EmailTemplate", "RegisterConfirm.html");
            string filePath = Path.Combine(controllerDirectory, relativePath);
            try
            {
                string body = string.Empty;
                using (StreamReader reader = new StreamReader(filePath))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("{username}", username);
                body = body.Replace("{verifyCode}", verifyCode);
                return body;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task SendMailContact(string recipientEmail, string subject, string body)
        {
            var fromAddress = new MailAddress("poolcomvn@gmail.com", "poolcomvn");
            var toAddress = new MailAddress(recipientEmail, "Recipient Name");
            const string fromPassword = "vyfi yuwu qwrx znhm";

            try
            {
                using (var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                })
                {
                    using (var message = new MailMessage(fromAddress, toAddress)
                    {
                        Subject = subject,
                        Body = body
                    })
                    {
                        await smtp.SendMailAsync(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
                throw; 
            }
        }
    }
}
