using BusinessObject.Models;
using DataAccess;
using Microsoft.AspNetCore.Mvc;
using PoolComVnWebAPI.DTO;
using System.IdentityModel.Tokens.Jwt;

namespace PoolComVnWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TournamentController : ControllerBase
    {
        private readonly TournamentDAO _tournamentDAO;
        private readonly ClubDAO _clubDAO;
        private readonly MatchDAO _matchDAO;
        private readonly PlayerDAO _playerDAO;
        private readonly TableDAO _tableDAO;

        public TournamentController(TournamentDAO tournamentDAO, ClubDAO clubDAO, 
            TableDAO tableDAO, PlayerDAO playerDAO, MatchDAO matchDAO)
        {
            _tournamentDAO = tournamentDAO;
            _clubDAO = clubDAO;
            _tableDAO = tableDAO;
            _matchDAO = matchDAO;
            _playerDAO = playerDAO;
        }

        [HttpGet("GetAllTournament")]
        //[Authorize]
        public IActionResult Index()
        {
            List<Tournament> tournaments = _tournamentDAO.GetAllTournament().ToList();
            tournaments.Reverse();
            return Ok(tournaments);
        }

        [HttpGet("GetTournament")]
        public IActionResult GetTournament(int tourId)
        {
            try
            {
                Tournament tour = _tournamentDAO.GetTournament(tourId);
                TournamentDetailDTO tournamentDetailDTO = new TournamentDetailDTO()
                {
                    Address = tour.Club.Address,
                    ClubName = tour.Club.ClubName,
                    TournamentId = tour.TourId,
                    TournamentName = tour.TourName,
                    Description = tour.Description,
                    StartTime = tour.StartDate ?? DateTime.Now,
                    EndTime = tour.EndDate,
                    Flyer = tour.Flyer,
                    GameType = tour.GameTypeId == Constant.Game8Ball ? Constant.String8Ball
                                    : (tour.GameTypeId == Constant.Game9Ball ? Constant.String9Ball : Constant.String10Ball),
                    Status = tour.Status,
                    TourTypeId = tour.TournamentTypeId,
                    RaceWin = GetRaceWinNumbers(tour.RaceToString),
                    RaceLose = GetRaceLoseNumbers(tour.RaceToString),
                    RegisterDate = tour.RegistrationDeadline,
                    MaxPlayer = tour.MaxPlayerNumber,
                    Access = tour.Access == null ? true : tour.Access.Value,
                    EntryFee = tour.EntryFee,
                    TotalPrize = tour.TotalPrize,
                };
                return Ok(tournamentDetailDTO);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        [HttpGet("GetTournamentForStOne")]
        public IActionResult GetTournamentForStOne(int tourId)
        {
            try
            {
                Tournament tour = _tournamentDAO.GetTournament(tourId);
                TournamentDTO tournamentDetailDTO = new TournamentDTO()
                {
                    TourId = tour.TourId,
                    TourName = tour.TourName,
                    Description = tour.Description,
                    StartDate = tour.StartDate ?? DateTime.Now,
                    EndDate = tour.EndDate,
                    GameTypeId = tour.GameTypeId,
                    Status = tour.Status,
                    TournamentTypeId = tour.TournamentTypeId,
                    RegistrationDeadline = tour.RegistrationDeadline,
                    MaxPlayerNumber = tour.MaxPlayerNumber,
                    Access = tour.Access == null ? true : tour.Access.Value,
                    EntryFee = tour.EntryFee,
                    TotalPrize = tour.TotalPrize,
                    KnockoutPlayerNumber = tour.KnockoutPlayerNumber,
                };
                return Ok(tournamentDetailDTO);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet("GetFlyer")]
        public IActionResult GetFlyer(int tourId)
        {
            try
            {
                Tournament tour = _tournamentDAO.GetTournament(tourId);
                return Ok(tour.Flyer);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet("GetTournamentsByClubId")]
        public IActionResult GetTournamentsByClubId(int clubId)
        {
            try
            {
                List<TournamentOutputDTO> allTourDto = new List<TournamentOutputDTO>();
                var allTourLst = _tournamentDAO.GetTournamentsByClubId(clubId);
                foreach (var item in allTourLst)
                {
                    TournamentOutputDTO tour = new TournamentOutputDTO()
                    {
                        TournamentId = item.TourId,
                        TournamentName = item.TourName,
                        StartTime = item.StartDate ?? DateTime.Now,
                        EndTime = item.EndDate,
                        Address = item.Club.Address,
                        ClubName = item.Club.ClubName,
                        Description = item.Description,
                        GameType = item.GameTypeId == Constant.Game8Ball ? Constant.String8Ball
                                    : (item.GameTypeId == Constant.Game9Ball ? Constant.String9Ball : Constant.String10Ball),
                        Status = item.Status,
                        Flyer = item.Flyer,
                    };
                    allTourDto.Add(tour);
                }
                allTourDto.Reverse();
                return Ok(allTourDto);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        private List<RaceNumber> GetRaceWinNumbers(string raceString)
        {
            List<RaceNumber> raceNumbers = new List<RaceNumber>();

            string[] parts = raceString.Split(',');

            foreach (var part in parts)
            {
                string[] subParts = part.Split('-');

                if (subParts.Length == 2 && int.TryParse(subParts[1], out int gameToWin) && subParts[0].StartsWith("W"))
                {
                    string round = "R" + subParts[0].Substring(1);
                    RaceNumber raceNumber = new RaceNumber
                    {
                        Round = subParts[0],
                        GameToWin = gameToWin
                    };

                    raceNumbers.Add(raceNumber);
                }
            }

            raceNumbers.Last().Round = "CK";
            return raceNumbers;
        }

        private List<RaceNumber> GetRaceLoseNumbers(string raceString)
        {
            List<RaceNumber> raceNumbers = new List<RaceNumber>();

            string[] parts = raceString.Split(',');

            foreach (var part in parts)
            {
                string[] subParts = part.Split('-');

                if (subParts.Length == 2 && int.TryParse(subParts[1], out int gameToWin) && subParts[0].StartsWith("L"))
                {
                    string round = "R" + subParts[0].Substring(1);
                    RaceNumber raceNumber = new RaceNumber
                    {
                        Round = subParts[0],
                        GameToWin = gameToWin
                    };

                    raceNumbers.Add(raceNumber);
                }
            }

            return raceNumbers;
        }

        [HttpGet("SearchTournament")]
        public ActionResult<IEnumerable<TournamentOutputDTO>> SearchTournament(string searchQuery)
        {
            try
            {
                var tournaments = _tournamentDAO.GetTournamentBySearch(searchQuery);

                if (tournaments == null || tournaments.Count == 0)
                {
                    return NotFound("Không tìm thấy giải đấu nào phù hợp.");
                }

                var tournamentDTOs = new List<TournamentOutputDTO>();
                foreach (var tour in tournaments)
                {
                    var tourDTO = new TournamentOutputDTO
                    {
                        TournamentId = tour.TourId,
                        TournamentName = tour.TourName,
                        StartTime = tour.StartDate ?? DateTime.Now,
                        EndTime = tour.EndDate,
                        Address = tour.Club.Address,
                        ClubName = tour.Club.ClubName,
                        Description = tour.Description,
                        GameType = tour.GameTypeId == Constant.Game8Ball ? Constant.String8Ball :
                                   (tour.GameTypeId == Constant.Game9Ball ? Constant.String9Ball : Constant.String10Ball),
                        Status = tour.Status,
                        Flyer = tour.Flyer
                    };
                    tournamentDTOs.Add(tourDTO);
                }
                return Ok(tournamentDTOs);
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có lỗi xảy ra
                return StatusCode(500, $"Lỗi khi tìm kiếm giải đấu: {ex.Message}");
            }
        }

        [HttpGet("FilterTournaments")]
        public IActionResult FilterTournaments(string? gameTypeName, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                var tournaments = _tournamentDAO.GetTournamentsByFilters(gameTypeName, startDate, endDate);

                // Chuyển đổi danh sách giải đấu thành danh sách DTO
                List<TournamentOutputDTO> tournamentDTOs = new List<TournamentOutputDTO>();
                foreach (var tournament in tournaments)
                {
                    var tournamentDTO = new TournamentOutputDTO
                    {
                        TournamentId = tournament.TourId,
                        TournamentName = tournament.TourName,
                        StartTime = tournament.StartDate ?? DateTime.Now,
                        EndTime = tournament.EndDate,
                        Address = tournament.Club.Address,
                        ClubName = tournament.Club.ClubName,
                        Description = tournament.Description,
                        GameType = tournament.GameTypeId == Constant.Game8Ball ? Constant.String8Ball
                                    : (tournament.GameTypeId == Constant.Game9Ball ? Constant.String9Ball : Constant.String10Ball),
                        Status = tournament.Status,
                        Flyer = tournament.Flyer
                    };
                    tournamentDTOs.Add(tournamentDTO);
                }

                // Trả về danh sách giải đấu
                return Ok(tournamentDTOs);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }


        [HttpGet("GetAllTour")]
        public IActionResult GetAllTour()
        {
            try
            {
                List<TournamentOutputDTO> allTourDto = new List<TournamentOutputDTO>();
                var allTourLst = _tournamentDAO.GetAllTournament();
                foreach (var item in allTourLst)
                {
                    TournamentOutputDTO tour = new TournamentOutputDTO()
                    {
                        TournamentId = item.TourId,
                        TournamentName = item.TourName,
                        StartTime = item.StartDate ?? DateTime.Now,
                        EndTime = item.EndDate,
                        Address = item.Club.Address,
                        ClubName = item.Club.ClubName,
                        ClubAvt = item.Club.Avatar,
                        Description = item.Description,
                        GameType = item.GameTypeId == Constant.Game8Ball ? Constant.String8Ball
                                    : (item.GameTypeId == Constant.Game9Ball ? Constant.String9Ball : Constant.String10Ball),
                        Status = item.Status,
                        Flyer = item.Flyer,
                    };
                    allTourDto.Add(tour);
                }
                allTourDto.Reverse();
                return Ok(allTourDto);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        [HttpGet("GetTourInfo")]
        public IActionResult GetTourInfo(int tourId)
        {
            try
            {
                Tournament tour = _tournamentDAO.GetTournament(tourId);
                return Ok(new
                {
                    playerNumber = tour.PlayerNumber,
                    finalSinglePlayer = tour.KnockoutPlayerNumber
                });
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        [HttpGet("GetTourMaxNumberOfPlayer")]
        public IActionResult GetTourMaxNumberOfPlayer(int tourId)
        {
            try
            {
                int maxNumberPlayer = _tournamentDAO.GetTourMaxNumberOfPlayer(tourId);
                return Ok(maxNumberPlayer);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet("GetTourKnockoutNumber")]
        public IActionResult GetTourKnockoutNumber(int tourId)
        {
            try
            {
                int? knockOutNumber = _tournamentDAO.GetTourKnockoutNumber(tourId);
                return Ok(knockOutNumber);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet("GetTourStatus")]
        public IActionResult GetTourStatus(int tourId)
        {
            try
            {
                int tourStatus = _tournamentDAO.GetTourStatus(tourId);
                return Ok(tourStatus);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet("UpdateTourStatus")]
        public IActionResult UpdateTourStatus(int tourId, int status)
        {
            try
            {
                _tournamentDAO.UpdateTourStatus(tourId, status);
                return Ok();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet("GetCreatingTour")]
        public IActionResult GetCreatingTour()
        {
            try
            {
                // Lấy giá trị token từ header
                var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

                // Giải mã token để lấy các claims
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                // Xử lý logic của bạn với các claims
                var roleClaim = jsonToken?.Claims.FirstOrDefault(claim => claim.Type.Equals("Role"));
                var account = jsonToken?.Claims.FirstOrDefault(claim => claim.Type.Equals("Account"));
                if (!Constant.BusinessRole.ToString().Equals(roleClaim?.Value))
                {
                    return BadRequest("Unauthorized");
                }

                if (!Int32.TryParse(account?.Value, out int accountId))
                {
                    return BadRequest("Invalid AccountId claim");
                }
                var club = _clubDAO.GetClubByAccountId(accountId);

                int clubId = club.ClubId;
                var tour = _tournamentDAO.GetCreatingTournament(clubId);
                CreatingTourDTO creatingTourDTO = new CreatingTourDTO();
                if (tour == null)
                {
                    creatingTourDTO.HaveCreatingTour = false;
                }
                else
                {
                    creatingTourDTO.HaveCreatingTour = true;
                    creatingTourDTO.TourId = tour.TourId;
                    creatingTourDTO.StepOnGoing = tour.Status;
                }
                return Ok(creatingTourDTO);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet("GetTourDetailForManager")]
        public IActionResult GetTourDetailForMananger(int tourId)
        {
            TourDetailForManagerDTO tourDetail = new TourDetailForManagerDTO();
            tourDetail.MatchEnd = _matchDAO.GetNumberOfMatchEnd(tourId);
            tourDetail.MatchPlaying = _matchDAO.GetNumberOfMatchPlaying(tourId);
            tourDetail.MatchIncoming = _matchDAO.GetNumberOfMatchRemain(tourId);
            tourDetail.NormalPlayer = _playerDAO.GetNumberOfPlayerPlaying(tourId);
            tourDetail.GiveUpPlayer = _playerDAO.GetNumberOfPlayerGiveUp(tourId);
            tourDetail.EliminatedPlayer = _playerDAO.GetNumberOfPlayerEliminated(tourId);
            int clubId = _tournamentDAO.GetTournament(tourId).ClubId;
            tourDetail.EmptyTable = _tableDAO.GetNumberOfTableEmpty(clubId);
            tourDetail.PlayingTable = _tableDAO.GetNumberOfTablePlaying(clubId);
            tourDetail.UseInTourTable = _tableDAO.GetNumberOfTableUseInTour(clubId);
            return Ok(tourDetail);
        }
    }
}
