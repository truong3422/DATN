using System.ComponentModel.DataAnnotations;

namespace PoolComVnWebAPI.DTO
{
    public class ContactDTO
    {
        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [RegularExpression(@"^[\p{L}\s']{2,50}$", ErrorMessage = "Họ và tên phải chứa ít nhất 2 kí tự và không chứa kí tự đặc biệt.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]{2,}@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Tin nhắn là bắt buộc")]
        [StringLength(500, ErrorMessage = "Tin nhắn không được vượt quá 500 kí tự")]
        public string Message { get; set; }
    }
}
