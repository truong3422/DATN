using BusinessObject.Models;
using DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PoolComVnWebAPI.DTO;
using System.IdentityModel.Tokens.Jwt;

namespace PoolComVnWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreateTournamentController : ControllerBase
    {
        private readonly ClubDAO _clubDAO;
        private readonly TournamentDAO _tournamentDAO;

        public CreateTournamentController(ClubDAO clubDAO, TournamentDAO tournamentDAO)
        {
            _clubDAO = clubDAO;
            _tournamentDAO = tournamentDAO;
        }

        [HttpPost("CreateTourStOne")]
        [Authorize]
        public IActionResult CreateTourStOne([FromBody] CreateTourStepOneDTO inputDto)
        {
            // Lấy giá trị token từ header
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            // Giải mã token để lấy các claims
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

            // Xử lý logic của bạn với các claims
            var roleClaim = jsonToken?.Claims.FirstOrDefault(claim => claim.Type.Equals("Role"));
            var account = jsonToken?.Claims.FirstOrDefault(claim => claim.Type.Equals("Account"));
            if (!Constant.BusinessRole.ToString().Equals(roleClaim.Value))
            {
                return BadRequest("Unauthorize");
            }
            var club = _clubDAO.GetClubByAccountId(Int32.Parse(account.Value));
            int clubId = club.ClubId;

            try
            {
                Tournament tour = new Tournament()
                {
                    TourName = inputDto.TournamentName,
                    Access = inputDto.Access,
                    ClubId = clubId,
                    Description = inputDto.Description,
                    StartDate = inputDto.StartTime,
                    EndDate = inputDto.EndTime,
                    EntryFee = inputDto.EntryFee.Value,
                    KnockoutPlayerNumber = inputDto.TournamentTypeId == Constant.DoubleEliminate ? inputDto.KnockoutNumber : null,
                    GameTypeId = inputDto.GameTypeId,
                    TotalPrize = inputDto.PrizeMoney,
                    TournamentTypeId = inputDto.TournamentTypeId,
                    MaxPlayerNumber = inputDto.MaxPlayerNumber,
                    RegistrationDeadline = inputDto.RegistrationDeadline,
                    RaceToString = inputDto.RaceNumberString,
                    Status = inputDto.Status.Value,
                };
                _tournamentDAO.CreateTournament(tour);
                return Ok(_tournamentDAO.GetLastestTournament().TourId);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        [HttpPost("UpdateTournament")]
        public IActionResult UpdateTournament([FromBody]TournamentDTO tournamentDTO)
        {
            var tournament = _tournamentDAO.GetTournament(tournamentDTO.TourId);
            tournament.TourName = tournamentDTO.TourName;
            tournament.MaxPlayerNumber = tournamentDTO.MaxPlayerNumber;
            tournament.Description = tournamentDTO.Description;
            tournament.StartDate = tournamentDTO.StartDate;
            tournament.EndDate = tournamentDTO.EndDate;
            tournament.GameTypeId = tournamentDTO.GameTypeId;
            tournament.TournamentTypeId = tournamentDTO.TournamentTypeId;
            tournament.KnockoutPlayerNumber = tournamentDTO.KnockoutPlayerNumber;
            tournament.RaceToString = tournamentDTO.RaceToString;
            tournament.EntryFee = tournament.EntryFee;
            tournament.TotalPrize = tournament.TotalPrize;
            tournament.RegistrationDeadline = tournament.RegistrationDeadline;
            tournament.Access = tournament.Access;
            _tournamentDAO.UpdateTournament(tournament);
            return Ok();
        }

        [HttpPost("CreateTourStTwo")]
        // [Authorize]
        public IActionResult CreateTourStTwo([FromBody] CreateTourStepTwoDTO BannerDTO)
        {
            try
            {
                Tournament tour = _tournamentDAO.GetTournament(BannerDTO.TourID);
                tour.Flyer = BannerDTO.Flyer;
                _tournamentDAO.UpdateTournament(tour);
                return Ok(BannerDTO.TourID);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        [HttpPost("UpdateBracketStepSix")]
        // [Authorize]
        public IActionResult UpdateBracketStepSix([FromBody]UpdateStepSixTournamentDTO updateStepSixTournamentDTO)
        {
            try
            {
                Tournament tournament = _tournamentDAO.GetTournament(updateStepSixTournamentDTO.TourId);
                tournament.MaxPlayerNumber = updateStepSixTournamentDTO.MaxPlayerNumber;
                tournament.KnockoutPlayerNumber = updateStepSixTournamentDTO.KnockOutNumberPlayer;
                tournament.RaceToString = updateStepSixTournamentDTO.RaceToString;
                _tournamentDAO.UpdateTournament(tournament);
                return Ok();
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        [HttpGet("CheckStep")]
        public IActionResult CheckStep(int tourId)
        {
            try
            {
                var tournament = _tournamentDAO.GetTournament(tourId);
                if (tournament != null)
                {
                    switch (tournament.Status)
                    {
                        case Constant.CreateTourStepOne:
                            return Ok(Constant.CreateTourStepOne);
                        case Constant.CreateTourStepTwo:
                            return Ok(Constant.CreateTourStepTwo);
                        case Constant.CreateTourStepThree:
                            return Ok(Constant.CreateTourStepThree);
                        case Constant.CreateTourStepFour:
                            return Ok(Constant.CreateTourStepFour);
                        case Constant.CreateTourStepFive:
                            return Ok(Constant.CreateTourStepFive);
                        case Constant.CreateTourStepSix:
                            return Ok(Constant.CreateTourStepSix);
                        default:
                            return Ok(Constant.CreateTourStepSix);
                    }
                }
                return Ok(Constant.CreateTourStepOne);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpGet("UpdateStep")]
        public IActionResult UpdateStep(int tourId, byte step)
        {
            try
            {
                var tournament = _tournamentDAO.GetTournament(tourId);
                if (tournament != null)
                {
                    if (step > tournament.Status)
                    {
                        tournament.Status = step;
                        _tournamentDAO.UpdateTournament(tournament);
                    }
                }
                return Ok();
            }
            catch (Exception e)
            {

                throw e;
            }
        }

    }
}
