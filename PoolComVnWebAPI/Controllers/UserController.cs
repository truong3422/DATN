using BusinessObject.Models;
using DataAccess;
using Firebase.Auth;
using Microsoft.AspNetCore.Mvc;
using PoolComVnWebAPI.DTO;
using System.Numerics;

namespace PoolComVnWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserDAO _userDAO;
        private readonly PlayerDAO _playerDAO;
        private readonly AccountDAO _accountDAO;
        private readonly AddressDAO _addressDAO;
        private readonly TournamentDAO _tournamentDAO;
        public UserController(UserDAO userDAO, AccountDAO accountDAO, AddressDAO addressDAO, PlayerDAO playerDAO, TournamentDAO tournamentDAO)
        {
            _userDAO = userDAO;
            _accountDAO = accountDAO;
            _addressDAO = addressDAO;
            _playerDAO = playerDAO;
            _tournamentDAO = tournamentDAO;
        }

        [HttpGet("GetAllUser")]
        public ActionResult<IEnumerable<UserDTO>> GetAllUsers()
        {
            try
            {
                var users = _userDAO.GetAllUsers();

                var userDTOs = users.Select(user => new UserDTO
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Address = user.Address,
                    Avatar = user.Avatar,
                    Dob = user.Dob,
                    CreatedDate = user.CreatedDate,
                    UpdatedDate = user.UpdatedDate,
                    AccountId = user.AccountId,
                    WardCode = user.WardCode
                });

                return Ok(userDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetUserById")]
        public ActionResult<IEnumerable<UserDTO>> GetUserById(int userId)
        {
            try
            {
                var user = _userDAO.GetUserById(userId);

                var userDTO = new UserDTO
                {
                    UserId = user.UserId,
                    FullName = user.FullName,
                    Address = user.Address,
                    Avatar = user.Avatar,
                    Dob = user.Dob,
                    CreatedDate = user.CreatedDate,
                    UpdatedDate = user.UpdatedDate,
                    AccountId = user.AccountId,
                    WardCode = user.WardCode
                };

                return Ok(userDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetTourJoinedForUser")]
        public IActionResult GetTourJoinedForUser(int userId)
        {
            try
            {
                var players = _playerDAO.GetPlayerByUserId(userId);
                if (players != null)
                {
                    List<Tournament> tournaments = new List<Tournament>();
                    foreach (var player in players)
                    {
                        var list = _tournamentDAO.GetTournamentListByPlayerId(player.PlayerId);
                        tournaments.AddRange(list);
                    }


                    List<TournamentDetailDTO> tournamentDetails = new List<TournamentDetailDTO>();
                    foreach (var tour in tournaments)
                    {
                        TournamentDetailDTO tournamentDetailDTO = new TournamentDetailDTO()
                        {
                            Address = tour.Club.Address,
                            ClubName = tour.Club.ClubName,
                            ClubId = tour.Club.ClubId,
                            ClubAvatar = tour.Club.Avatar,
                            TournamentId = tour.TourId,
                            TournamentName = tour.TourName,
                            Description = tour.Description,
                            StartTime = tour.StartDate ?? DateTime.Now,
                            EndTime = tour.EndDate,
                            Flyer = tour.Flyer,
                            GameType = tour.GameTypeId == Constant.Game8Ball ? Constant.String8Ball
                                            : (tour.GameTypeId == Constant.Game9Ball ? Constant.String9Ball : Constant.String10Ball),
                            Status = tour.Status
                        };

                        tournamentDetails.Add(tournamentDetailDTO);
                    }

                    if (tournamentDetails != null)
                    {
                        return Ok(tournamentDetails);
                    }
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("ChangeUserStatus/{id}")]
        public IActionResult ChangeUserStatus(int id)
        {
            try
            {
                _userDAO.ChangeUserStatus(id);
                return Ok("User status changed successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");

            }
        }

        [HttpGet("GetWinMatchNumber/{userId}")]
        public IActionResult GetWinMatchNumber(int userId)
        {
            try
            {
                var players = _playerDAO.GetPlayerByUserId(userId);
                if (players != null)
                {
                    int winMatchNum = 0;
                    foreach (var player in players)
                    {
                        int num = _userDAO.WinMatchNumberById(player.PlayerId);
                        winMatchNum += num;
                    }
                    return Ok(winMatchNum);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");

            }
        }

        [HttpGet("GetLoseMatchNumber/{userId}")]
        public IActionResult GetLoseMatchNumber(int userId)
        {
            try
            {
                var players = _playerDAO.GetPlayerByUserId(userId);
                if (players != null)
                {
                    int loseMatchNum = 0;
                    foreach(var player in players)
                    {
                        int num = _userDAO.LoseMatchNumberById(player.PlayerId);
                        loseMatchNum += num;
                    }
                    return Ok(loseMatchNum);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");

            }
        }

        [HttpGet("GetTopForPlayer/{userId}")]
        public IActionResult GetTopForPlayer(int userId)
        {
            try
            {
                var players = _playerDAO.GetPlayerByUserId(userId);
                if (players != null)
                {
                    List<Award> awards = new List<Award>();
                    foreach (var player in players)
                    {
                        var list = _userDAO.GetTopForPlayer(player.PlayerId);
                        awards.AddRange(list);
                    }
                    var sortedAwards = awards.OrderByDescending(a => a.TourId).ToList();
                    return Ok(sortedAwards);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");

            }
        }

        [HttpGet("GetUserlist")]
        public IActionResult GetUserlist()
        {
            try
            {
                var userList = _userDAO.GetUserList();
                if (userList != null)
                {
                    var userStaticList = userList.Where(p => p.Players != null).Select(
                        user => new UserIncludStatisticDTO
                        {
                            UserId = user.UserId,
                            FullName = user.FullName,
                            Country = _playerDAO.GetCountryByPlayerId(user.Players.Select(x => x.PlayerId).FirstOrDefault())?.CountryName ?? string.Empty,
                            CountryImage = _playerDAO.GetCountryByPlayerId(user.Players.Select(x => x.PlayerId).FirstOrDefault())?.CountryImage ?? string.Empty,
                            Province = _userDAO.GetProvinceByUserId(user.UserId),
                            Email = user.Account.Email,
                            TournamentJoined = _userDAO.GetTournamentUserJoined(user.Players.Select(x => x.PlayerId).FirstOrDefault()),
                            MatchWin = user.Players.Sum(x => x.PlayerInMatches.Where(x => x.IsWinner == true).Count()),
                            MatchLose = user.Players.Sum(x => x.PlayerInMatches.Where(x => x.IsWinner == false).Count()),
                        });
                    return Ok(userStaticList);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");

            }
        }

        [HttpPost("UpdateUser")]
        public IActionResult UpdateUser(UserDTO userDTO)
        {
            try
            {
                var user = _userDAO.GetUserById(userDTO.UserId);
                var account = _accountDAO.GetAccountById(userDTO.AccountId);
                account.PhoneNumber = userDTO.PhoneNumber;
                if (user == null)
                {
                    return NotFound();
                }
                user.UpdatedDate = userDTO.UpdatedDate;
                user.Avatar = userDTO.Avatar;
                user.Address = userDTO.Address;
                user.FullName = userDTO.FullName;
                user.Dob = userDTO.Dob;
                user.WardCode = userDTO.WardCode;
                var ward = _addressDAO.GetWardsByWardCode(userDTO.WardCode);
                user.WardCodeNavigation = ward;
                _userDAO.UpdateUser(user);
                _accountDAO.UpdateAccount(account);
                return Ok("User updated successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");

            }
        }
    }
}
