using AutoMapper;
using BusinessObject.Models;
using DataAccess;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using PoolComVnWebAPI.DTO;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Numerics;

namespace PoolComVnWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : ControllerBase
    {
        private readonly PlayerDAO _playerDAO;
        private readonly TournamentDAO _tournamentDAO;
        private readonly AccountDAO _accountDAO;
        private readonly MatchDAO _matchDAO;
        private readonly IMapper _mapper;
        private static List<PlayerDTO> selectedPlayers = new List<PlayerDTO>();
        public PlayerController(PlayerDAO playerDAO, IMapper mapper, TournamentDAO tournamentDAO, AccountDAO accountDAO, MatchDAO matchDAO)
        {
            _playerDAO = playerDAO;
            _mapper = mapper;
            _tournamentDAO = tournamentDAO;
            _accountDAO = accountDAO;
            _matchDAO = matchDAO;

        }


        [HttpGet]
        public ActionResult<IEnumerable<PlayerDTO>> Get()
        {
            var players = _playerDAO.GetAllPlayers();


            foreach (var playerDto in players)
            {
                if (playerDto.User != null && playerDto.User.Account != null)
                {
                    playerDto.PhoneNumber = playerDto.User.Account.PhoneNumber;
                }
            }

            return Ok(players.Select(player => new PlayerDTO
            {
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                CountryName = player.Country?.CountryName,
                PhoneNumber = player.PhoneNumber,
                Level = player.Level,
            }));
        }



        [HttpGet("GetByName/{name}")]
        public ActionResult<PlayerDTO> GetByName(string name)
        {
            var player = _playerDAO.GetPlayerByName(name);

            if (player == null)
            {
                return NotFound();
            }

            var playerDto = _mapper.Map<PlayerDTO>(player);
            return Ok(playerDto);
        }


        [HttpPost("AddPlayer")]
        public IActionResult AddPlayer([FromBody] PlayerDTO playerDto)
        {
            try
            {
                var account = _accountDAO.GetAccountByEmail(playerDto.Email);
                var user = _accountDAO.GetUserByAccountById(account.AccountId);
                var tour = _tournamentDAO.GetTournament(playerDto.TourId ?? 0);
                var country = _playerDAO.GetCountryByID(playerDto.CountryId);
                if (playerDto == null)
                {
                    return BadRequest("Dữ liệu người chơi không hợp lệ.");
                }

                var player = new Player
                {
                    PlayerName = playerDto.PlayerName,
                    CountryId = playerDto.CountryId,
                    Level = playerDto.Level ?? 0,
                    UserId = user.UserId,
                    TourId = playerDto.TourId,
                    PhoneNumber = playerDto.PhoneNumber,
                    Email = playerDto.Email,
                    IsPayed = playerDto.IsPayed,
                    User = user,
                    Tour = tour,
                    Country = country

                };

                _playerDAO.AddPlayer(player);

                return Ok();
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Đã xảy ra lỗi khi thêm người chơi: {ex.Message}");
            }
        }

        [HttpPost("AddPlayerToSoloMatch")]
        public IActionResult AddPlayerToSoloMatch([FromBody] PlayerInSoloMatchDTO playerInSoloMatchDTO)
        {
            try
            {
                var playerInSoloMatch = new PlayerInSoloMatch
                {
                    SoloMatchId = playerInSoloMatchDTO.SoloMatchId,
                    PlayerId = playerInSoloMatchDTO.PlayerId,
                    Score = playerInSoloMatchDTO.Score,
                    GameWin = playerInSoloMatchDTO.GameWin,
                    IsWinner = playerInSoloMatchDTO.IsWinner
                };
                _playerDAO.AddPlayerToSoloMatch(playerInSoloMatch);
                return Ok("Người chơi đã được thêm vào trận đấu đơn thành công.");
            }
            catch (Exception e)
            {

                return StatusCode(500, $"Lỗi server: {e.Message}");
            }
        }

        [HttpPost("AddPlayerToTournament")]
        public IActionResult AddPlayerToTournament([FromBody] List<PlayerDTO> playerDtos)
        {
            try
            {

                if (playerDtos == null || playerDtos.Count == 0)
                {
                    return BadRequest("No players provided.");
                }


                foreach (var player in playerDtos)
                {
                    var player2 = _mapper.Map<Player>(player);
                    player2.PlayerId = 0;
                    player2.CountryId = Constant.NationVietNamId;
                    player2.TourId = player.TourId;
                    player2.Status = Constant.NormalPlayer;
                    _playerDAO.AddPlayer(player2);
                }


                return Ok("Players added to the tournament successfully.");
            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpGet("Update/{id}")]
        public IActionResult Put(int id, [FromBody] PlayerDTO updatedPlayerDto)
        {
            if (updatedPlayerDto == null)
            {
                return BadRequest("Invalid request data");
            }

            var existingPlayer = _playerDAO.GetPlayerById(id);

            if (existingPlayer == null)
            {
                return NotFound();
            }


            existingPlayer.PlayerName = updatedPlayerDto.PlayerName;
            existingPlayer.Level = updatedPlayerDto.Level.Value;


            existingPlayer.User.Account.PhoneNumber = updatedPlayerDto.PhoneNumber;


            _playerDAO.UpdatePlayer(existingPlayer);

            return NoContent();
        }

        [HttpGet("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            var player = _playerDAO.GetPlayerById(id);

            if (player == null)
            {
                return NotFound();
            }

            _playerDAO.DeletePlayer(id);

            return NoContent();
        }

        [HttpGet("GetPlayerByTourId")]
        public IActionResult GetPlayerByTourId(int tourId)
        {
            var players = _playerDAO.GetPlayersByTournament(tourId);
            List<PlayerDTO> lstPlayer = new List<PlayerDTO>();
            foreach (Player p in players)
            {
                PlayerDTO playerDTO = new PlayerDTO
                {
                    PlayerId = p.PlayerId,
                    PlayerName = p.PlayerName,
                    CountryName = p.Country.CountryImage,
                };
                lstPlayer.Add(playerDTO);
            }
            return Ok(lstPlayer);
        }

        [HttpGet("GetPlayerRequestJoinByTourId")]
        public IActionResult GetPlayerRequestJoinByTourId(int tourId)
        {
            var players = _playerDAO.GetPlayerRequestJoinByTourId(tourId);
            List<PlayerDTO> lstPlayer = new List<PlayerDTO>();
            foreach (Player p in players)
            {
                PlayerDTO playerDTO = new PlayerDTO
                {
                    PlayerId = p.PlayerId,
                    PlayerName = p.PlayerName,
                    CountryImage = p.Country.CountryImage,
                    CountryName = p.Country.CountryName,
                    Email = p.Email,
                    PhoneNumber = p.PhoneNumber,
                };
                lstPlayer.Add(playerDTO);
            }
            return Ok(lstPlayer);
        }

        [HttpGet("GetPlayerCanPlay")]
        public IActionResult GetPlayerCanPlay(int tourId)
        {
            var players = _playerDAO.GetPlayersByTournament(tourId);
            List<PlayerDTO> lstPlayer = new List<PlayerDTO>();
            foreach (Player p in players)
            {
                PlayerDTO playerDTO = new PlayerDTO
                {
                    PlayerId = p.PlayerId,
                    PlayerName = p.PlayerName,
                    CountryImage = p.Country.CountryImage,
                    CountryName = p.Country.CountryName,
                    Email = p.Email,
                    PhoneNumber = p.PhoneNumber,
                    Level = p.Level,
                };
                lstPlayer.Add(playerDTO);
            }
            return Ok(lstPlayer);
        }

        [HttpGet("GetPlayerExBotByTourId")]
        public IActionResult GetPlayerExBotByTourId(int tourId)
        {


            var players = _playerDAO.GetPlayersByTournament(tourId);
            var tours = _tournamentDAO.GetTournament(tourId);
            int totalPlayers = tours.MaxPlayerNumber;
            var newList = new List<dynamic>();
            if (tours.TournamentTypeId == Constant.DoubleEliminate)
            {
                int startCountWin = Convert.ToInt32(Math.Log2(totalPlayers) - Math.Log2(tours.KnockoutPlayerNumber ?? 0) + 2);
                var playerLose = _matchDAO.GetPlayerLose(tourId);

                foreach (var player in playerLose)
                {

                    if (player.MatchCode[0] == 'L' || (player.MatchCode[0] == 'W' && Convert.ToInt32(player.MatchCode[1] - '0') >= startCountWin))
                    {
                        newList.Add(player);
                    }
                }
            }
            else
            {
                var playerLose = _matchDAO.GetPlayerLose(tourId);
                foreach (var player in playerLose)
                {
                    newList.Add(player);
                }
            }

            List<PlayerDTO> lstPlayer = new List<PlayerDTO>();

            foreach (Player p in players)
            {
                int countMatchWin = 0;
                int countMatchLose = 0;
                int countGameLose = 0;
                int countGameWin = 0;
                string streakMatch = null;
                var matches = _matchDAO.GetMatchesByPlayerId(p.PlayerId);
                var matchesCountStreak = _matchDAO.GetLatestMatchesByPlayerId(p.PlayerId);
                if (matches != null)
                {
                    foreach (PlayerInMatch playerInMatch in matches)
                    {

                        var match = _matchDAO.GetPlayerInMatchByMatchId(playerInMatch.MatchId, playerInMatch.PlayerId);
                        var matchAgainst = _matchDAO.GetMatchByMatchIdAndPlayerId(playerInMatch.MatchId, playerInMatch.PlayerId);
                        if (playerInMatch.IsWinner == true)
                        {
                            countMatchWin++;
                        }
                        else if (playerInMatch.IsWinner == false)
                        {
                            countMatchLose++;
                        }
                        countGameLose += matchAgainst == null ? 0 : (matchAgainst.Score == null ? 0 : matchAgainst.Score.Value);
                        countGameWin += match == null ? 0 : (match.Score == null ? 0 : match.Score.Value);

                    }
                }
                if (countMatchLose == 2)
                {
                    var matchLast = _matchDAO.GetLatestMatchPlayerPlay(p.PlayerId);
                    var matchLastData = _matchDAO.GetMatchByMatchId(matchLast.MatchId);

                }
                if (matchesCountStreak != null)
                {

                    foreach (PlayerInMatch playerInMatch in matchesCountStreak)
                    {


                        if (playerInMatch.IsWinner == true)
                        {
                            streakMatch = streakMatch + "Win";
                        }
                        else if (playerInMatch.IsWinner == false)
                        {
                            streakMatch = streakMatch + "Lose";
                        }

                    }
                }
                string reversedStreakMatch = null;
                if (streakMatch != null)
                {
                    reversedStreakMatch = new string(streakMatch.Reverse().ToArray());
                }

                PlayerDTO playerDTO = new PlayerDTO
                {
                    PlayerId = p.PlayerId,
                    PlayerName = p.PlayerName,
                    CountryName = p.Country.CountryImage,
                    CountryName2 = p.Country.CountryName,
                    Match = $"{countMatchWin}-{countMatchLose}",
                    Game = $"{countGameWin}-{countGameLose}",
                    StreakMatch = reversedStreakMatch

                };
                if (!playerDTO.PlayerName.Equals("BOT"))
                {
                    lstPlayer.Add(playerDTO);
                }
            }

            int startCount = totalPlayers;
            var rankDictionary = new Dictionary<int, string>();
            string rank = null;
            int currentIndex = 0;
            if (newList != null && newList.Count != 0)
            {
                string MatchCodeFirst = newList[0].MatchCode;
                foreach (var player in newList.Skip(1))
                {

                    int rankIndex = startCount - currentIndex;

                    if (MatchCodeFirst == player.MatchCode)
                    {
                        currentIndex++;
                        continue;
                    }
                    if (MatchCodeFirst != player.MatchCode)
                    {

                        rank = $"{rankIndex}-{startCount}";
                        foreach (var p in newList.Where(p => p.MatchCode == MatchCodeFirst))
                        {
                            rankDictionary.Add(p.PlayerId, rank);
                        }
                        startCount = rankIndex - 1;
                        MatchCodeFirst = player.MatchCode;
                        currentIndex = 0;

                    }
                }
            }

            var TheLastMatchInTournament = _matchDAO.GetTheLastMatchOfTournament(tourId);
            if (TheLastMatchInTournament != null)
            {
                if (TheLastMatchInTournament.Status == 2)
                {
                    var playerInFinalMatch = _matchDAO.GetPlayerInLastMatchByMatchId(TheLastMatchInTournament.MatchId);
                    foreach (var playerFinal in playerInFinalMatch)
                    {
                        if (playerFinal.IsWinner == true)
                        {
                            rank = "1";
                            rankDictionary.Add(playerFinal.PlayerId, rank);

                        }
                        if (playerFinal.IsWinner == false)
                        {
                            rank = "2";
                            rankDictionary.Add(playerFinal.PlayerId, rank);

                        }
                    }
                }
            }

            foreach (var player in lstPlayer)
            {
                foreach (var key in rankDictionary.Keys)
                {
                    if (key == player.PlayerId)
                    {
                        player.Ranking = rankDictionary[key];
                        break;
                    }
                }
            }
            if (tours.Status == 2)
            {
                var sortedPlayers = lstPlayer
        .Where(p => p.Ranking != null)
        .OrderBy(p => ConvertRankingToComparable(p.Ranking))
        .ToList();
                return Ok(sortedPlayers);
            }
            else
            {
                return Ok(lstPlayer);
            }
        }

        [HttpGet("GetNumberPlayerByTourId")]
        public IActionResult GetNumberPlayerByTourId(int tourId)
        {
            int numberPlayers = _playerDAO.GetNumberPlayerByTourId(tourId);
            return Ok(numberPlayers);
        }
        private static int ConvertRankingToComparable(string ranking)
        {
            char firstChar = ranking.FirstOrDefault();
            int rankValue = int.Parse(firstChar.ToString());

            return rankValue;
        }

        [HttpGet("GenerateBotInTour")]
        public IActionResult GenerateBotInTour(int tourId)
        {
            int numberHumanPlayers = _playerDAO.GetNumberPlayerByTourId(tourId);
            int numberPlayersOfTour = _tournamentDAO.GetTournament(tourId).MaxPlayerNumber;
            int numberBotPlayer = numberPlayersOfTour - numberHumanPlayers;

            for (int i = 0; i < numberBotPlayer; i++)
            {
                Player botPlayer = new Player()
                {
                    PlayerName = "BOT",
                    CountryId = Constant.NationVietNamId,
                    TourId = tourId,
                    Status = Constant.GiveUpPlayer,
                };
                _playerDAO.AddPlayer(botPlayer);
            }

            return Ok();
        }

        [HttpGet("GetNumberPlayerExBotByTourId")]
        public IActionResult GetNumberPlayerExBotByTourId(int tourId)
        {
            int numberPlayers = _playerDAO.GetNumberPlayerExBotByTourId(tourId);
            return Ok(numberPlayers);
        }

        [HttpGet("PlayerJoinTournament")]
        public IActionResult PlayerJoinTournament(string userEmail, string playerName, int tourId)
        {
            try
            {
                var account = _accountDAO.GetAccountByEmail(userEmail);
                var user = _accountDAO.GetUserByAccountById(account.AccountId);
                var tour = _tournamentDAO.GetTournament(tourId);
                var firstPlayerOfUser = _playerDAO.GetPlayerByUserId(user.UserId).FirstOrDefault();
                var countryId = firstPlayerOfUser.CountryId != null ? firstPlayerOfUser.CountryId : 1;
                var country = _playerDAO.GetCountryByID(countryId);

                var player = new Player
                {
                    PlayerName = playerName,
                    CountryId = countryId,
                    Level = 0,
                    UserId = user.UserId,
                    TourId = tourId,
                    PhoneNumber = firstPlayerOfUser.PhoneNumber != null ? firstPlayerOfUser.PhoneNumber : "",
                    Email = userEmail,
                    IsPayed = false,
                    User = user,
                    Tour = tour,
                    Country = country
                };

                _playerDAO.AddPlayer(player);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi khi tham gia giải đấu: {ex.Message}");
            }
        }

        [HttpGet("PlayerOutTournament")]
        public IActionResult PlayerOutTournament(string userEmail, int tourId)
        {
            try
            {
                var account = _accountDAO.GetAccountByEmail(userEmail);
                var user = _accountDAO.GetUserByAccountById(account.AccountId);

                string playerName = _playerDAO.RemovePlayerByUserIdAndTourId(user.UserId, tourId);

                return Ok(playerName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi khi rời khỏi giải đấu: {ex.Message}");
            }
        }



        [HttpGet("GetPlayerInTournament")]
        public IActionResult GetPlayerInTournament(string userEmail, int tourId)
        {
            try
            {
                var account = _accountDAO.GetAccountByEmail(userEmail);
                var user = _accountDAO.GetUserByAccountById(account.AccountId);

                Player player = _playerDAO.GetPlayerByUserIdAndTourId(user.UserId, tourId);
                if (player != null)
                {
                    PlayerDTO playerDTO = new PlayerDTO()
                    {
                        PlayerId = player.PlayerId,
                        Status = player.Status
                    };
                    return Ok(playerDTO);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi tìm người chơi trong giải: {ex.Message}");
            }
        }

        [HttpGet("RejectPlayer")]
        public IActionResult RejectPlayer(int playerId)
        {
            try
            {
                _playerDAO.RejectPlayer(playerId);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi khi tham gia giải đấu: {ex.Message}");
            }
        }

        [HttpGet("AcceptPlayer")]
        public IActionResult AcceptPlayer(int playerId)
        {
            try
            {
                _playerDAO.AcceptPlayer(playerId);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Đã xảy ra lỗi khi tham gia giải đấu: {ex.Message}");
            }
        }
    }
}
