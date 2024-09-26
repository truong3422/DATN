using System.ComponentModel.DataAnnotations;

namespace PoolComVnWebAPI.DTO
{
    public class CreateManageAccountDTO
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Mật khẩu mới là bắt buộc")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Xác nhận mật khẩu mới là bắt buộc")]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu mới phải giống với mật khẩu mới.")]
        public string ConfirmNewPassword { get; set; }
    }
}
