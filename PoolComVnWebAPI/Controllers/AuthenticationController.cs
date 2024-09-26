using BusinessObject.Models;
using DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PoolComVnWebAPI.Authentication;
using PoolComVnWebAPI.Common;
using PoolComVnWebAPI.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace PoolComVnWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AccountDAO _accountDAO;
        private IConfiguration _config;
        private readonly IEmailSender _emailSender;

        public AuthenticationController(AccountDAO accountDAO, IConfiguration configuration, IEmailSender emailSender)
        {
            _accountDAO = accountDAO;
            _config = configuration;
            _emailSender = emailSender;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDTO loginDTO)
        {
            Account account = _accountDAO.AuthenAccount(loginDTO.Email, loginDTO.Password);
            if (account != null)
            {
                int checkAccount = _accountDAO.CheckAccountStatus(account.AccountId);
                if (checkAccount == Constant.AccountStatusReady)
                {
                    return Ok(new { token = GenerateToken(account) });
                }
                else if (checkAccount == Constant.AccountStatusBanned)
                {
                    return BadRequest();
                }
                else
                {
                    return Forbid();
                }
            }
            return Unauthorized();
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgorPassword([FromBody] string email)
        {

            
            string newPassword = TokenManager.GenerateSecretString(6);

            try
            {
                     await _emailSender.SendMailResetPasswordAsync(email, newPassword);

      
                    _accountDAO.ForgotPassword(email, newPassword);

                    return Ok();
                
            }
            catch (Exception ex)
            {
              
                return StatusCode(500, "Failed to send reset password email.");
            }

        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
        {
            if (_accountDAO.IsEmailExist(registerDto.Email) || _accountDAO.IsUsernameExist(registerDto.Username))
            {
                return BadRequest("Email or username already exist");
            }

            _accountDAO.RegisterAccount(registerDto.Username, registerDto.Email,
                registerDto.Pass, registerDto.isBusiness);

            Account account = _accountDAO.AuthenAccount(registerDto.Email, registerDto.Pass);

            return Ok(new { token = GenerateToken(account) });
        }

        [HttpPost("testAuthorize")]
        [Authorize]
        public IActionResult Authorize()
        {
            // Lấy giá trị token từ header
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            // Giải mã token để lấy các claims
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            // Xử lý logic với các claims
            var RoleClaim = jsonToken?.Claims.FirstOrDefault(claim => claim.Type == "Role");

            if (Constant.UserRole.ToString().Equals(RoleClaim.Value))
            {
                return Ok("authen Thanh cong");
            }

            // Xử lý khi không tìm thấy claim "roleId"
            return BadRequest("Không tìm thấy thông tin về roleId trong token.");
        }

        private string GenerateToken(Account account)
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim("Role", account.RoleId.ToString()),
                new Claim("Account", account.AccountId.ToString()),
            };

            var token = new JwtSecurityToken(_config["Jwt:Issuer"], _config["Jwt:Audience"], claims,
                            expires: DateTime.Now.AddDays(1),
                            signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("SendVerifyCode")]
        public async Task<IActionResult> SendVerifyCode(int accountId)
        {
            var account = _accountDAO.GetAccountById(accountId);
            string verifyCode = TokenManager.GenerateSecretString(6);
            _accountDAO.SetVerifyCode(accountId, verifyCode);
            await _emailSender.SendMailAsync(account.Email, account.Email, verifyCode);
            return Ok();
        }


        [HttpPost("VerifyAccount")]
        public IActionResult VerifyAccount([FromBody] VerifyAccountDTO verifyAccountDTO)
        {
            bool checkVerify = _accountDAO.CheckVerifyAccount(verifyAccountDTO.AccountId, verifyAccountDTO.VerifyCode);
            var account = _accountDAO.GetAccountById(verifyAccountDTO.AccountId);
            if (checkVerify)
            {
                return Ok(new
                {
                    token = GenerateToken(account),
                    Email = account.Email
                });
            }
            return Unauthorized();
        }

        [HttpGet("GetIdByEmail")]
        public IActionResult GetIdByEmail(string email)
        {
            var id = _accountDAO.GetAccountByEmail(email).AccountId;
            return Ok(id);
        }
        
        [HttpPost("SendEmailContact")]
        public async Task<IActionResult> SendEmailContact([FromBody] ContactDTO contactDTO)
        {
            try
            {
                var subject = $"[PoolCom]Yêu Cầu Hỗ Trợ từ {contactDTO.Name}";
                var body = $"Name: {contactDTO.Name}\nEmail: {contactDTO.Email}\n\n{contactDTO.Message}";
                await _emailSender.SendMailContact("nguyenphilong28072002@gmail.com", subject, body);

                return Ok("Email sent successfully!");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }

        [HttpPost("ChangePassword")]
        public IActionResult ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO)
        {
            try
            {
                var existingAccount = _accountDAO.GetAccountByEmail(changePasswordDTO.Email);

                if (existingAccount == null)
                {
                    return BadRequest("Account not found");
                }

                // Kiểm tra tính hợp lệ của mật khẩu cũ
                bool isOldPasswordCorrect = BCrypt.Net.BCrypt.Verify(changePasswordDTO.OldPassword, existingAccount.Password);

                if (!isOldPasswordCorrect)
                {
                    return BadRequest("Incorrect old password");
                }

                // Thực hiện thay đổi mật khẩu
                bool isPasswordChanged = _accountDAO.ChangePasswordAccount(existingAccount.Email, changePasswordDTO.OldPassword, changePasswordDTO.NewPassword);

                if (isPasswordChanged)
                {
                    return Ok("Password changed successfully");
                }
                else
                {
                    return BadRequest("Failed to change password");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error: {ex.Message}");
            }
        }




    }
}
