using System.ComponentModel.DataAnnotations;

namespace PoolComVnWebClient.DTO
{
    public class CreateTournamentInputDTO
    {
        [Required(ErrorMessage = "Tên giải đấu là bắt buộc.")]
        public string TournamentName { get; set; }
        [Required(ErrorMessage = "Mô tả là bắt buộc.")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Thời gian bắt đầu là bắt buộc.")]
        public DateTime StartTime { get; set; }
        [Required(ErrorMessage = "Thời gian kết thúc là bắt buộc.")]
        public DateTime EndTime { get; set; }
        [Required(ErrorMessage = "Loại trò chơi là bắt buộc.")]
        public int GameTypeId { get; set; }
        [Required(ErrorMessage = "Loại giải đấu là bắt buộc.")]
        public int TournamentTypeId { get; set; }
        [Required(ErrorMessage = "Số lượng người chơi tối đa là bắt buộc.")]
        public int MaxPlayerNumber { get; set; }
        public int? KnockoutNumber { get; set; }
        public string RaceNumberString { get; set; } = string.Empty;
        public int? EntryFee { get; set; }
        public int? PrizeMoney { get; set; }
        [Required(ErrorMessage = "Thời hạn đăng ký là bắt buộc.")]
        public DateTime RegistrationDeadline { get; set; }
        [Required(ErrorMessage = "Phạm vi giải đấu là bắt buộc.")]
        public bool Access { get; set; }
        public byte? Status { get; set; }
    }
}
