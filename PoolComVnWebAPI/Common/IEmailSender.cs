namespace PoolComVnWebAPI.Common
{
    public interface IEmailSender
    {
        Task SendMailAsync(string email, string username, string verifyCode);
        Task SendMailResetPasswordAsync(string email, string verifyCode);
        Task SendMailContact(string recipientEmail, string subject, string body);
    }
}
