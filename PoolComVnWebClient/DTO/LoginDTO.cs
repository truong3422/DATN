using System.ComponentModel.DataAnnotations;

namespace PoolComVnWebClient.DTO
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Địa chỉ email không hợp lệ.(form: abcd@gmail.com")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string Password { get; set; }
    }
}
