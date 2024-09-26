using BusinessObject.Models;
using DataAccess;
using Microsoft.AspNetCore.Mvc;
using PoolComVnWebAPI.DTO;

namespace PoolComVnWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AccountDAO _accountDAO;
        private readonly AddressDAO _addressDAO;
        public AccountController(AccountDAO accountDAO,AddressDAO addressDAO)
        {
            _accountDAO = accountDAO;
            _addressDAO = addressDAO;
        }
        [HttpGet]
        public ActionResult<IEnumerable<AccountDTO>> Get()
        {
            try
            {
                var allAccounts = _accountDAO.GetAllAccounts();
                var allAccountDTOs = allAccounts.Select(account => new AccountDTO
                {
                    AccountID = account.AccountId,
                    Email = account.Email,
                    Password = account.Password,
                    RoleID = account.RoleId,
                    PhoneNumber = account.PhoneNumber,
                    verifyCode = account.VerifyCode,
                    Status = account.Status
                });

                return Ok(allAccountDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetManagerAccounts")]
        public ActionResult<IEnumerable<AccountDTO>> GetManagerAccounts()
        {
            try
            {
                var managerAccounts = _accountDAO.GetAllManagerAccounts();
                var managerAccountDTOs = new List<AccountDTO>();

                foreach (var account in managerAccounts)
                {
                    var accountDTO = new AccountDTO
                    {
                        AccountID = account.AccountId,
                        Email = account.Email ?? string.Empty, 
                        Password = account.Password ?? string.Empty,
                        RoleID = account.RoleId,
                        PhoneNumber = account.PhoneNumber ?? string.Empty,
                        verifyCode = account.VerifyCode ?? string.Empty,
                        Status = account.Status
                    };

                    managerAccountDTOs.Add(accountDTO);
                }

                return Ok(managerAccountDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpGet("GetUserByAccount")]
        public ActionResult<UserDTO> GetUserByAccountId(int accountId)
        {
            try
            {

                var user = _accountDAO.GetUserByAccountById(accountId);
                if (user == null)
                {
                    return NotFound(); 
                }
                var user2 = new UserDTO
                {
                    FullName = user.FullName,
                    Address = user.Address,
                    Avatar = user.Avatar,
                    Dob = user.Dob,
                    CreatedDate = user.CreatedDate,
                    UpdatedDate = user.UpdatedDate,
                    AccountId = user.AccountId,
                    WardCode = user.WardCode,
                    UserId = user.UserId
                };

                return Ok(user2); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }
        }
        [HttpPost("CreateUser")]
        public ActionResult CreateUser([FromBody] UserDTO userDTO)
        {
            try
            {

                var account = _accountDAO.GetAccountById(userDTO.AccountId);
                var ward = _addressDAO.GetWardsByWardCode(userDTO.WardCode);
                var user = new User
                {
                   
                    FullName = userDTO.FullName,
                    Dob = userDTO.Dob,
                    Avatar = userDTO.Avatar,
                    CreatedDate = userDTO.CreatedDate,
                   UpdatedDate = userDTO.UpdatedDate,
                   AccountId = userDTO.AccountId,
                   Address = userDTO.Address,
                   WardCode = userDTO.WardCode,
                   Account = account,
                   WardCodeNavigation = ward
                };
                _accountDAO.AddUser(user);
                return Ok(_accountDAO.GetLastestUser().UserId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }    

        [HttpGet("GetAccountByEmail/{email}")]
        public ActionResult<AccountDTO> GetAccountByEmail(string email)
        {
            try
            {
                var account = _accountDAO.GetAccountByEmail(email);

                if (account == null)
                {
                    return NotFound("Không tìm thấy tài khoản cho email đã cung cấp.");
                }
                var accountDto = new AccountDTO
                {
                    AccountID = account.AccountId,
                    Email = account.Email,
                    PhoneNumber = account.PhoneNumber,
                    RoleID = account.RoleId,
                    verifyCode = account.VerifyCode,
                    Status = account.Status,
                    Password = account.Password
                };

                return Ok(accountDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy thông tin tài khoản: {ex.Message}");
            }
        }
        [HttpGet("GetAccountById/{accountId}")]
        public ActionResult<AccountDTO> GetAccountById(int accountId)
        {
            try
            {
                var account = _accountDAO.GetAccountById(accountId);

                if (account == null)
                {
                    return NotFound($"Không tìm thấy tài khoản với ID: {accountId}");
                }

                var accountDto = new AccountDTO
                {
                    AccountID = account.AccountId,
                    Email = account.Email,
                    PhoneNumber = account.PhoneNumber,
                    RoleID = account.RoleId,
                    verifyCode = account.VerifyCode,
                    Status = account.Status,
                    Password = account.Password
                };

                return Ok(accountDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi lấy thông tin tài khoản: {ex.Message}");
            }
        }


        [HttpGet("ByUsername/{username}")]
        public ActionResult<AccountDTO> GetByUsername(string username)
        {
            try
            {
                var account = _accountDAO.GetAccountByUsername(username);

                if (account == null)
                {
                    return NotFound();
                }
                var accountDTO = new AccountDTO
                {
                    AccountID = account.AccountId,
                    Email = account.Email,
                    Password = account.Password,
                    RoleID = account.RoleId,
                    PhoneNumber = account.PhoneNumber,
                    verifyCode = account.VerifyCode,
                    Status = account.Status
                };

                return Ok(accountDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult Post([FromBody] AccountDTO accountDTO)
        {
            try
            {
                var account = new Account
                {
                    AccountId = accountDTO.AccountID,
                    Email = accountDTO.Email,
                    Password = accountDTO.Password,
                    RoleId = accountDTO.RoleID,
                    PhoneNumber = accountDTO.PhoneNumber,
                    VerifyCode = accountDTO.verifyCode,
                    Status = accountDTO.Status
                };


                _accountDAO.AddAccount(account);
                var createdAccountDTO = new AccountDTO
                {
                    AccountID = account.AccountId,
                    Email = account.Email,
                    Password = account.Password,
                    RoleID = account.RoleId,
                    PhoneNumber = account.PhoneNumber,
                    verifyCode = account.VerifyCode,
                    Status = account.Status
                };

                return CreatedAtAction(nameof(Get), new { id = createdAccountDTO.AccountID }, createdAccountDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Update/{id}")]
        public ActionResult Put(int id, [FromBody] AccountDTO updatedAccountDTO)
        {
            try
            {
                if (id != updatedAccountDTO.AccountID)
                {
                    return BadRequest("Invalid Account ID");
                }

                var updatedAccount = new Account
                {
                    AccountId = updatedAccountDTO.AccountID,
                    Email = updatedAccountDTO.Email,
                    Password = updatedAccountDTO.Password,
                    RoleId = updatedAccountDTO.RoleID,
                    PhoneNumber = updatedAccountDTO.PhoneNumber,
                    VerifyCode = updatedAccountDTO.verifyCode,
                    Status = updatedAccountDTO.Status
                };

                _accountDAO.UpdateAccount(updatedAccount);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("CreateBusinessManagerAccount")]
        public IActionResult CreateBusinessManagerAccount([FromBody] CreateManageAccountDTO businessManagerDTO)
        {
            try
            {
                // Kiểm tra xem các trường thông tin cần thiết đã được cung cấp hay chưa
                if (string.IsNullOrEmpty(businessManagerDTO.Email) || string.IsNullOrEmpty(businessManagerDTO.NewPassword))
                {
                    return BadRequest("Email và mật khẩu là bắt buộc.");
                }

                // Kiểm tra xem email đã tồn tại trong hệ thống chưa
                if (_accountDAO.IsEmailExist(businessManagerDTO.Email))
                {
                    return BadRequest("Email đã tồn tại.");
                }
                if (string.IsNullOrEmpty(businessManagerDTO.NewPassword) != string.IsNullOrEmpty(businessManagerDTO.ConfirmNewPassword))
                {
                    return BadRequest("Mật khẩu và xác nhận mật khẩu phải trùng nhau.");
                }
                _accountDAO.CreateAccountBusinessManager(businessManagerDTO.Email, businessManagerDTO.NewPassword);

                // Trả về thông báo thành công
                return Ok("Tạo tài khoản Business Manager thành công");
            }
            catch (Exception ex)
            {
                // Trả về thông báo lỗi nếu có lỗi xảy ra
                return BadRequest($"Lỗi: {ex.Message}");
            }
        }


        [HttpPost("ChangeBusinessManagerStatus/{id}")]
        public ActionResult ChangeBusinessManagerStatus(int id)
        {
            try
            {
                _accountDAO.ChangeBusinessManagerStatus(id);
                 return Ok("Business manager status changed successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}