using BusinessObject.Models;
using DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PoolComVnWebAPI.DTO;
using System.Numerics;
using System.Text.RegularExpressions;

namespace PoolComVnWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchOfTourController : ControllerBase
    {
        private readonly MatchDAO _matchDAO;
        private readonly ClubDAO _clubDAO;
        private readonly PlayerDAO _playerDAO;
        private readonly TournamentDAO _tournamentDAO;
        private readonly TableDAO _tableDAO;

        public MatchOfTourController(MatchDAO matchDAO, ClubDAO clubDAO, PlayerDAO playerDAO,
            TournamentDAO tournamentDAO, TableDAO tableDAO)
        {
            _matchDAO = matchDAO;
            _clubDAO = clubDAO;
            _playerDAO = playerDAO;
            _tournamentDAO = tournamentDAO;
            _tableDAO = tableDAO;
        }

        [HttpGet("GetMatchForBracket")]
        public IActionResult GetMatchForBracket(int tourId)
        {
            var lstMatch = _matchDAO.GetMatchOfTournaments(tourId);
            
            List<MatchOfTournamentOutputDTO> lstOutputMatch = new List<MatchOfTournamentOutputDTO>();
            foreach (var match in lstMatch)
            {
                var players = _playerDAO.GetPlayersByMatchTour(match.MatchId);
                PlayerInMatch? p1 = players.OrderBy(p => p.PlayerMatchId).FirstOrDefault();
                PlayerInMatch? p2 = null;
                if (players.Count() == 2)
                {
                    p2 = players.OrderBy(p => p.PlayerMatchId).LastOrDefault();
                }

                MatchOfTournamentOutputDTO matchOfTournament = new MatchOfTournamentOutputDTO()
                {
                    MatchId = match.MatchId,
                    MatchNumber = match.MatchNumber,
                    MatchCode = match.MatchCode,
                    LoseNextMatch = match.LoseToMatch,
                    WinNextMatch = match.WinToMatch,
                    P1Id = p1 != null ? p1.PlayerId : null,
                    P2Id = p2 != null ? p2.PlayerId : null,
                    P1Country = p1 != null ? p1.Player.Country.CountryImage : null,
                    P1Name = p1 != null ? p1.Player.PlayerName : null,
                    P1Score = p1 == null ? "_" : p1.Score.ToString(),
                    P2Country = p2 != null ? p2.Player.Country.CountryImage : null,
                    P2Name = p2 != null ? p2.Player.PlayerName : null,
                    P2Score = p2 == null ? "_" : p2.Score.ToString(),
                    RaceToNumber = match.RaceTo,
                    MatchStatus = match.Status,
                    TableName = match.Table != null ? match.Table.TagName : "Chưa có",
                    StartDate = match.StartTime,
                    EndDate = match.EndTime,
                };

                lstOutputMatch.Add(matchOfTournament);
            }
            return Ok(lstOutputMatch);
        }

        [HttpGet("RandomMatch")]
        public IActionResult RandomMatch(int tourId)
        {
            var playersInTour = _playerDAO.GetPlayersByTournament(tourId).ToList();
            var botPlayer = playersInTour.Where(p => p.PlayerName.Equals("BOT")).ToList();

            foreach (var player in botPlayer)
            {
                playersInTour.Remove(player);
            }

            List<MatchOfTournamentOutputDTO> lstMatch = new List<MatchOfTournamentOutputDTO>();
            int count = playersInTour.Count() + botPlayer.Count();
            for (int i = 0; i < (count / 2); i++)
            {
                MatchOfTournamentOutputDTO matchOfTournament = new MatchOfTournamentOutputDTO();
                matchOfTournament.MatchNumber = i + 1;
                // trường hợp người = bot => trận phân đều người và bot
                if (playersInTour.Count == botPlayer.Count)
                {
                    Random rand = new Random();
                    int randomId = rand.Next(0, playersInTour.Count - 1);

                    Player p1 = playersInTour[randomId];
                    matchOfTournament.P1Name = p1.PlayerName;
                    matchOfTournament.P1Id = p1.PlayerId;
                    matchOfTournament.P1Country = p1.Country.CountryImage;
                    playersInTour.Remove(p1);

                    Player p2 = botPlayer[0];
                    matchOfTournament.P2Name = p2.PlayerName;
                    matchOfTournament.P2Id = p2.PlayerId;
                    matchOfTournament.P2Country = p2.Country.CountryImage;
                    botPlayer.Remove(p2);
                    lstMatch.Add(matchOfTournament);
                }
                else
                {
                    // trường hợp còn lại
                    Random rand = new Random();
                    int randomId = rand.Next(0, playersInTour.Count - 1);

                    Player p1 = playersInTour[randomId];
                    matchOfTournament.P1Name = p1.PlayerName;
                    matchOfTournament.P1Id = p1.PlayerId;
                    matchOfTournament.P1Country = p1.Country.CountryImage;
                    playersInTour.Remove(p1);

                    if (botPlayer.Count() > 0 && (i % 2 == 0))
                    {
                        Player p2 = botPlayer[0];
                        matchOfTournament.P2Name = p2.PlayerName;
                        matchOfTournament.P2Id = p2.PlayerId;
                        matchOfTournament.P2Country = p2.Country.CountryImage;
                        botPlayer.Remove(p2);
                        lstMatch.Add(matchOfTournament);
                    }
                    else
                    {
                        randomId = rand.Next(0, playersInTour.Count - 1);
                        Player p2 = playersInTour[randomId];
                        matchOfTournament.P2Name = p2.PlayerName;
                        matchOfTournament.P2Id = p2.PlayerId;
                        matchOfTournament.P2Country = p2.Country.CountryImage;
                        playersInTour.Remove(p2);
                        lstMatch.Add(matchOfTournament);
                    }
                }
                
            }


            return Ok(lstMatch);
        }

        [HttpPost("SaveMatchesRandom/{tourId}")]
        public IActionResult SaveMatchesRandom(int tourId, [FromBody] List<MatchOfTournamentOutputDTO> lstMatch)
        {
            var tournament = _tournamentDAO.GetTournament(tourId);
            _tournamentDAO.UpdateTourStatus(tournament.TourId, Constant.TournamentInProgress);
            int count = 1;
            if (tournament.TournamentTypeId == Constant.SingleEliminate)
            {
                foreach (var match in lstMatch)
                {
                    string matchCode = GenerateMatchCodeSingle(count, tournament.MaxPlayerNumber);
                    MatchOfTournament matchOfTournament = new MatchOfTournament()
                    {
                        MatchCode = matchCode,
                        TourId = tourId,
                        MatchNumber = count,
                        Status = 0,
                        StartTime = DateTime.Now,
                        WinToMatch = NextMatchSingle(count, tournament.MaxPlayerNumber),
                        RaceTo = GetNumberRaceTo(tournament.RaceToString, matchCode),
                    };
                    count++;
                    _matchDAO.AddMatch(matchOfTournament);
                    _playerDAO.AddPlayerToMatch(matchOfTournament.MatchId, match.P1Id.Value);
                    _playerDAO.AddPlayerToMatch(matchOfTournament.MatchId, match.P2Id.Value);
                }
                GenerateAllMatchOfTourSingle(tourId);
                return Ok();
            }
            else
            {
                foreach (var match in lstMatch)
                {
                    string matchCode = GenerateMatchCode(count, tournament.MaxPlayerNumber, tournament.KnockoutPlayerNumber);
                    MatchOfTournament matchOfTournament = new MatchOfTournament()
                    {
                        MatchCode = matchCode,
                        TourId = tourId,
                        MatchNumber = count,
                        Status = 0,
                        StartTime = DateTime.Now,
                        LoseToMatch = LoseNextMatch(count, tournament.MaxPlayerNumber, tournament.KnockoutPlayerNumber.Value),
                        WinToMatch = WinNextMatch(count, tournament.MaxPlayerNumber, tournament.KnockoutPlayerNumber.Value),
                        RaceTo = GetNumberRaceTo(tournament.RaceToString, matchCode),
                    };
                    count++;
                    _matchDAO.AddMatch(matchOfTournament);
                    _playerDAO.AddPlayerToMatch(matchOfTournament.MatchId, match.P1Id.Value);
                    _playerDAO.AddPlayerToMatch(matchOfTournament.MatchId, match.P2Id.Value);
                }
                GenerateAllMatchOfTour(tourId);
                return Ok();
            }
        }
        
        [HttpGet("ScheduleMatch")]
        public IActionResult ScheduleMatch(ScheduleMatchDTO scheduleMatchDTO)
        {
            var match = _matchDAO.GetMatchByMatchId(scheduleMatchDTO.MatchId);
            match.TableId = scheduleMatchDTO.TableId;
            match.StartTime = scheduleMatchDTO.ScheduleTime;
            match.Status = Constant.MatchIncoming;
            _matchDAO.UpdateMatch(match);
            var table = _tableDAO.GetTableById(scheduleMatchDTO.TableId);
            table.IsScheduling = true;
            _tableDAO.UpdateTable(table);
            return Ok();
        }

        [HttpPost("StartMatch")]
        public IActionResult StartMatch(ScheduleMatchDTO scheduleMatchDTO)
        {
            // nếu mà có hai người thì check xem có người nào bỏ giải không
            // nếu có người bỏ giải thì end luôn
            var match = _matchDAO.GetMatchByMatchId(scheduleMatchDTO.MatchId);
            var schedulingTable = match.Table;
            if (schedulingTable != null)
            {
                schedulingTable.IsScheduling = false;
                _tableDAO.UpdateTable(schedulingTable);
            }
            match.TableId = scheduleMatchDTO.TableId;
            match.StartTime = DateTime.Now.Date;
            match.Status = Constant.MatchInprogress;
            _matchDAO.UpdateMatch(match);
            var table = _tableDAO.GetTableById(scheduleMatchDTO.TableId);
            table.Status = Constant.TableInUse;
            _tableDAO.UpdateTable(table);
            return Ok();
        }

        [HttpPost("SaveMatch")]
        public IActionResult SaveMatch(SaveMatchDTO saveMatchDTO)
        {

            var p1 = _playerDAO.GetPlayerInMatch(saveMatchDTO.P1Id, saveMatchDTO.MatchId);
            var p2 = _playerDAO.GetPlayerInMatch(saveMatchDTO.P2Id, saveMatchDTO.MatchId);

            if (saveMatchDTO.P1Status != Constant.NormalPlayer || saveMatchDTO.P2Status != Constant.NormalPlayer)
            {
                var match = _matchDAO.GetMatchByMatchId(saveMatchDTO.MatchId);
                var table = _tableDAO.GetTableById(match.TableId.Value);
                table.Status = Constant.TableNotUse;
                _tableDAO.UpdateTable(table);
                match.Status = Constant.MatchEnd;
                match.EndTime = DateTime.Now;
                _matchDAO.UpdateMatch(match);

                if (saveMatchDTO.P1Status == Constant.GiveUpPlayer && saveMatchDTO.P2Status == Constant.GiveUpPlayer)
                {
                    Player player1 = _playerDAO.GetPlayerById(p1.PlayerId);
                    Player player2 = _playerDAO.GetPlayerById(p2.PlayerId);
                    if ((player1.PlayerName != Constant.BotPlayerName && player2.PlayerName != Constant.BotPlayerName)
                        || (player1.PlayerName == Constant.BotPlayerName && player2.PlayerName == Constant.BotPlayerName))
                    {
                        p1.GameWin = string.Empty;
                        p1.Score = Constant.ScoreForSurrender;
                        p1.IsWinner = false;
                        _playerDAO.UpdatePlayerInMatch(p1);

                        p2.GameWin = Constant.ScoreForWinner.ToString();
                        p2.Score = Constant.ScoreForWinner;
                        p2.IsWinner = true;
                        _playerDAO.UpdatePlayerInMatch(p2);
                    }
                    else
                    {
                        if (player1.PlayerName == Constant.BotPlayerName)
                        {
                            p1.GameWin = string.Empty;
                            p1.Score = Constant.ScoreForSurrender;
                            p1.IsWinner = false;
                            _playerDAO.UpdatePlayerInMatch(p1);

                            p2.GameWin = Constant.ScoreForWinner.ToString();
                            p2.Score = Constant.ScoreForWinner;
                            p2.IsWinner = true;
                            _playerDAO.UpdatePlayerInMatch(p2);
                        }

                        if (player2.PlayerName == Constant.BotPlayerName)
                        {
                            p1.GameWin = Constant.ScoreForWinner.ToString();
                            p1.Score = Constant.ScoreForWinner;
                            p1.IsWinner = true;
                            _playerDAO.UpdatePlayerInMatch(p1);

                            p2.GameWin = string.Empty;
                            p2.Score = Constant.ScoreForSurrender;
                            p2.IsWinner = false;
                            _playerDAO.UpdatePlayerInMatch(p2);
                        }
                    }
                }
                else
                {
                    if (saveMatchDTO.P1Status == Constant.GiveUpPlayer)
                    {
                        p1.GameWin = string.Empty;
                        p1.Score = Constant.ScoreForSurrender;
                        p1.IsWinner = false;
                        _playerDAO.UpdatePlayerInMatch(p1);

                        p2.GameWin = Constant.ScoreForWinner.ToString();
                        p2.Score = Constant.ScoreForWinner;
                        p2.IsWinner = true;
                        _playerDAO.UpdatePlayerInMatch(p2);
                    }

                    if (saveMatchDTO.P2Status == Constant.GiveUpPlayer)
                    {
                        p1.GameWin = Constant.ScoreForWinner.ToString();
                        p1.Score = Constant.ScoreForWinner;
                        p1.IsWinner = true;
                        _playerDAO.UpdatePlayerInMatch(p1);

                        p2.GameWin = string.Empty;
                        p2.Score = Constant.ScoreForSurrender;
                        p2.IsWinner = false;
                        _playerDAO.UpdatePlayerInMatch(p2);
                    }
                }

                if (saveMatchDTO.P1Status == Constant.GiveUpPlayer)
                {
                    Player player1 = _playerDAO.GetPlayerById(p1.PlayerId);
                    player1.Status = Constant.GiveUpPlayer;
                    _playerDAO.UpdatePlayer(player1);
                }

                if (saveMatchDTO.P2Status == Constant.GiveUpPlayer)
                {
                    Player player2 = _playerDAO.GetPlayerById(p2.PlayerId);
                    player2.Status = Constant.GiveUpPlayer;
                    _playerDAO.UpdatePlayer(player2);
                }

                if (p1.IsWinner != null && p1.IsWinner.Value)
                {
                    PrepareNextMatch(saveMatchDTO.MatchId, p1.PlayerId, p2.PlayerId);
                }
                else
                {
                    PrepareNextMatch(saveMatchDTO.MatchId, p2.PlayerId, p1.PlayerId);
                }
            }
            else
            {
                p1.GameWin = saveMatchDTO.P1GameWin;
                p1.Score = saveMatchDTO.P1Score;
                _playerDAO.UpdatePlayerInMatch(p1);

                p2.GameWin = saveMatchDTO.P2GameWin;
                p2.Score = saveMatchDTO.P2Score;
                _playerDAO.UpdatePlayerInMatch(p2);
            }

            return Ok();
        }

        [HttpPost("CloseMatch")]
        public IActionResult CloseMatch(SaveMatchDTO saveMatchDTO)
        {
            var match = _matchDAO.GetMatchByMatchId(saveMatchDTO.MatchId);
            var table = _tableDAO.GetTableById(match.TableId.Value);
            table.Status = Constant.TableNotUse;
            _tableDAO.UpdateTable(table);
            match.Status = Constant.MatchEnd;
            match.EndTime = DateTime.Now;
            _matchDAO.UpdateMatch(match);

            var p1 = _playerDAO.GetPlayerInMatch(saveMatchDTO.P1Id, saveMatchDTO.MatchId);
            var p2 = _playerDAO.GetPlayerInMatch(saveMatchDTO.P2Id, saveMatchDTO.MatchId);
            if (saveMatchDTO.P1Status != Constant.NormalPlayer || saveMatchDTO.P2Status != Constant.NormalPlayer)
            {
                if (saveMatchDTO.P1Status == Constant.GiveUpPlayer)
                {
                    p1.GameWin = string.Empty;
                    p1.Score = Constant.ScoreForSurrender;
                    p1.IsWinner = false;
                    _playerDAO.UpdatePlayerInMatch(p1);

                    p2.GameWin = Constant.ScoreForWinner.ToString();
                    p2.Score = Constant.ScoreForWinner;
                    p2.IsWinner = true;
                    _playerDAO.UpdatePlayerInMatch(p2);
                }

                if (saveMatchDTO.P2Status == Constant.GiveUpPlayer)
                {
                    p1.GameWin = Constant.ScoreForWinner.ToString();
                    p1.Score = Constant.ScoreForWinner;
                    p1.IsWinner = true;
                    _playerDAO.UpdatePlayerInMatch(p1);

                    p2.GameWin = string.Empty;
                    p2.Score = Constant.ScoreForSurrender;
                    p2.IsWinner = false;
                    _playerDAO.UpdatePlayerInMatch(p2);
                }
            }
            else
            {
                p1.GameWin = saveMatchDTO.P1GameWin;
                p1.Score = saveMatchDTO.P1Score;
                p1.IsWinner = (p1.Score == match.RaceTo);
                _playerDAO.UpdatePlayerInMatch(p1);

                
                p2.GameWin = saveMatchDTO.P2GameWin;
                p2.Score = saveMatchDTO.P2Score;
                p2.IsWinner = (p2.Score == match.RaceTo);
                _playerDAO.UpdatePlayerInMatch(p2);
            }

            if (saveMatchDTO.P1Status == Constant.GiveUpPlayer)
            {
                Player player1 = _playerDAO.GetPlayerById(p1.PlayerId);
                player1.Status = Constant.GiveUpPlayer;
                _playerDAO.UpdatePlayer(player1);
            }

            if (saveMatchDTO.P2Status == Constant.GiveUpPlayer)
            {
                Player player2 = _playerDAO.GetPlayerById(p2.PlayerId);
                player2.Status = Constant.GiveUpPlayer;
                _playerDAO.UpdatePlayer(player2);
            }

            if (p1.IsWinner != null && p1.IsWinner.Value)
            {
                PrepareNextMatch(saveMatchDTO.MatchId, p1.PlayerId, p2.PlayerId);
            }
            else
            {
                PrepareNextMatch(saveMatchDTO.MatchId, p2.PlayerId, p1.PlayerId);
            }

            // check kết thúc giải
            if (match.MatchNumber == _matchDAO.GetMatchOfTournaments(match.TourId).Count)
            {
                _tournamentDAO.UpdateTourStatus(match.TourId, Constant.TournamentComplete);
                // free bàn 
                int clubId = _tournamentDAO.GetTournament(match.TourId).ClubId;
                var lstTable = _tableDAO.GetAllTablesForClub(clubId).Where(t => t.IsUseInTour == true);
                foreach(Table t in lstTable)
                {
                    t.Status = Constant.TableNotUse;
                    t.IsUseInTour = false;
                    _tableDAO.UpdateTable(t);
                }
            }

            return Ok();
        }

        private void PrepareNextMatch(int matchId, int winnerId, int loserId)
        {
            var match = _matchDAO.GetMatchByMatchId(matchId);

            // Xử lý người thua
            if (match.LoseToMatch != Constant.NoNextMatch)
            {
                var nextLoseMatch = _matchDAO.GetMatchByNumber(match.LoseToMatch, match.TourId);
                if (nextLoseMatch != null)
                {
                    _playerDAO.AddPlayerToMatch(nextLoseMatch.MatchId, loserId);
                    _matchDAO.ModifyMatch(nextLoseMatch.MatchId);
                }
            }
            else
            {
                var player = _playerDAO.GetPlayerById(loserId);
                if (player.Status == Constant.NormalPlayer)
                {
                    player.Status = Constant.EliminatedPlayer;
                }
                _playerDAO.UpdatePlayer(player);
            }

            // Xử lý người thắng
            if (match.WinToMatch != Constant.NoNextMatch)
            {
                var nextWinMatch = _matchDAO.GetMatchByNumber(match.WinToMatch, match.TourId);
                if (nextWinMatch != null)
                {
                    _playerDAO.AddPlayerToMatch(nextWinMatch.MatchId, winnerId);
                    _matchDAO.ModifyMatch(nextWinMatch.MatchId);
                }
            }        
        }

        private void GenerateAllMatchOfTour(int tourId)
        {
            // Cần xử lý thêm trường hợp single bracket
            Tournament tour = _tournamentDAO.GetTournament(tourId);
            int numberOfMatch = CalculateNumberOfMatch(tour.MaxPlayerNumber, tour.KnockoutPlayerNumber);
            for (int i = 1; i <= numberOfMatch; i++)
            {
                if (!_matchDAO.CheckExistMatch(tourId, i))
                {
                    string matchCode = GenerateMatchCode(i, tour.MaxPlayerNumber, tour.KnockoutPlayerNumber);
                    MatchOfTournament matchOfTournament = new MatchOfTournament()
                    {
                        TourId = tourId,
                        MatchCode = matchCode,
                        MatchNumber = i,
                        WinToMatch = WinNextMatch(i, tour.MaxPlayerNumber, tour.KnockoutPlayerNumber.Value),
                        LoseToMatch = LoseNextMatch(i, tour.MaxPlayerNumber, tour.KnockoutPlayerNumber.Value),
                        Status = Constant.MatchTBD,
                        RaceTo = GetNumberRaceTo(tour.RaceToString, matchCode),
                    };
                    if (i == numberOfMatch)
                    {
                        // set trận cuối cùng để check 
                        matchOfTournament.WinToMatch = Constant.NoNextMatch;
                    }
                    _matchDAO.AddMatch(matchOfTournament);
                }
            }
        }

        private void GenerateAllMatchOfTourSingle(int tourId)
        {
            Tournament tour = _tournamentDAO.GetTournament(tourId);
            int numberOfMatch = CalculateNumberOfMatch(tour.MaxPlayerNumber, null);
            for (int i = 1; i <= numberOfMatch; i++)
            {
                if (!_matchDAO.CheckExistMatch(tourId, i))
                {
                    string matchCode = GenerateMatchCodeSingle(i, tour.MaxPlayerNumber);
                    MatchOfTournament matchOfTournament = new MatchOfTournament()
                    {
                        TourId = tourId,
                        MatchCode = matchCode,
                        MatchNumber = i,
                        WinToMatch = NextMatchSingle(i, tour.MaxPlayerNumber),
                        Status = Constant.MatchTBD,
                        RaceTo = GetNumberRaceTo(tour.RaceToString, matchCode),
                    };
                    if (i == numberOfMatch)
                    {
                        // set trận cuối cùng để check 
                        matchOfTournament.WinToMatch = Constant.NoNextMatch;
                    }
                    _matchDAO.AddMatch(matchOfTournament);
                }
            }
        }

        int GetNumberRaceTo(string raceToString, string matchCode)
        {
            string pattern = $"{matchCode}-([0-9]+)";

            Match match = Regex.Match(raceToString, pattern);

            if (!match.Success)
            {
                return -1;
            }

            string numberStr = match.Groups[1].Value;
            return int.Parse(numberStr);
        }

        private int CalculateNumberOfMatch(int numberOfPlayer, int? knockOutPlayer)
        {
            if (knockOutPlayer != null)
            {
                int count = (int)Math.Log2(numberOfPlayer / knockOutPlayer.Value);
                int numberOfMatch = numberOfPlayer / 2;
                for (int i = 1; i <= count; i++)
                {
                    numberOfMatch += (int)(numberOfPlayer / Math.Pow(2, i));
                    numberOfMatch += (int)(numberOfPlayer / Math.Pow(2, i + 1));
                }
                count = (int)Math.Log2(knockOutPlayer.Value);
                for (int i = 1; i <= count; i++)
                {
                    numberOfMatch += (int)(knockOutPlayer / Math.Pow(2, i));
                }
                return numberOfMatch;
            }
            else
            {
                int numberOfMatch = numberOfPlayer - 1;
                return numberOfMatch;
            }
        }

        [HttpGet("GetMatchForControl")]
        public IActionResult GetMatchForControl(int matchId)
        {
            var match = _matchDAO.GetMatchForControl(matchId);
            var players = _playerDAO.GetPlayersByMatchTour(match.MatchId);
            var p1 = players.OrderBy(p => p.PlayerMatchId).FirstOrDefault();
            var p2 = players.OrderBy(p => p.PlayerMatchId).LastOrDefault();

            MatchControlDTO matchControlDTO = new MatchControlDTO()
            {
                MatchId = match.MatchId,
                MatchNumber = match.MatchNumber,
                MatchCode = match.MatchCode,
                P1Id = p1 != null ? p1.PlayerId : null,
                P2Id = p2 != null ? p2.PlayerId : null,
                P1Country = p1 != null ? p1.Player.Country.CountryImage : null,
                P1Name = p1 != null ? p1.Player.PlayerName : null,
                P1Score = p1 == null ? "_" : p1.Score.ToString(),
                P2Country = p2 != null ? p2.Player.Country.CountryImage : null,
                P2Name = p2 != null ? p2.Player.PlayerName : null,
                P2Score = p2 == null ? "_" : p2.Score.ToString(),
                TableId = match.TableId,
                TableName = match.Table != null ? match.Table.TagName : null,
                StartTime = match.StartTime,
                EndTime = match.EndTime,
                P1GameWin = p1 != null ? p1.GameWin : null,
                P2GameWin = p2 != null ? p2.GameWin : null,
                P1Status = p1 != null ? p1.Player.Status : null,
                P2Status = p2 != null ? p2.Player.Status : null,
                RaceTo = match.RaceTo,
                Status = match.Status,
                LoseToMatch = match.LoseToMatch,
                WinToMatch = match.WinToMatch,
            };
            return Ok(matchControlDTO);
        }

        [HttpGet("GetMatchForSchedule")]
        public IActionResult GetMatchForSchedule(int matchId)
        {
            var match = _matchDAO.GetMatchForSchedule(matchId);
            Table? table = match.Table;

            var players = _playerDAO.GetPlayersByMatchTour(match.MatchId);
            var p1 = players.OrderBy(p => p.PlayerMatchId).FirstOrDefault();
            var p2 = players.OrderBy(p => p.PlayerMatchId).LastOrDefault();

            MatchScheduleDTO matchSchedule = new MatchScheduleDTO()
            {
                MatchId = match.MatchId,
                MatchNumber = match.MatchNumber,
                MatchCode = match.MatchCode,
                P1Country = p1 != null ? p1.Player.Country.CountryImage : null,
                P1Name = p1 != null ? p1.Player.PlayerName : null,
                P2Country = p2 != null ? p2.Player.Country.CountryImage : null,
                P2Name = p2 != null ? p2.Player.PlayerName : null,
                TableId = match.TableId,
                TableName = match.Table != null ? match.Table.TableName : null,
                StartTime = match.StartTime,
                EndTime = match.EndTime,
                RaceTo = match.RaceTo,
                LoseToMatch = match.LoseToMatch,
                WinToMatch = match.WinToMatch,
            };
            // chưa xếp lịch
            if (table == null)
            {
                matchSchedule.MatchNumber = match.MatchNumber;
                matchSchedule.MatchId = match.MatchId;
                matchSchedule.ScheduleStatus = 0;
                matchSchedule.StartTime = match.StartTime;
                matchSchedule.EndTime = match.EndTime;
            }

            // xếp lịch chưa đấu
            if (table != null && table.IsScheduling.Value == true && match.Status == Constant.MatchIncoming)
            {
                matchSchedule.ScheduleStatus = 1;
            }

            // đang đấu và đã đấu 
            if (table != null && match.Status != Constant.MatchIncoming)
            {
                matchSchedule.ScheduleStatus = 2;
            }

            return Ok(matchSchedule);
        }


        private string GenerateStringGameWin(int raceTo)
        {
            string result = "";
            for (int i = 1; i <= raceTo; i++)
            {
                result = result + "," + i;
            }
            return result.Trim(',');
        }

        private int WinNextMatch(int m, int n, int d)
        {
            int a = Convert.ToInt32(Math.Log2(n));
            int w = Convert.ToInt32(Math.Log2(d));
            int x = 0;

            for (int i = (a - 1); i >= w; i--)
            {
                x = x + Convert.ToInt32(Math.Pow(2, i));
            }

            int y = 2 * x;
            if (m % 2 == 1 && m <= x)
            {
                return (n / 2 + (m + 1) / 2);
            }
            else if (m % 2 == 0 && m <= x)
            {
                return n / 2 + m / 2;
            }
            else if (m > x && m <= (x + Convert.ToInt32(Math.Pow(2, (w - 1)))))
            {
                return m * 2 + d / 2 - (m - x);
            }
            else if (m % 2 == 1 && m > (y + Convert.ToInt32(Math.Pow(2, (w - 1)))))
            {
                return Convert.ToInt32(0.5 * y + 0.75 * d + (m + 1) / 2);
            }
            else if (m % 2 == 0 && m > (y + Convert.ToInt32(Math.Pow(2, (w - 1)))))
            {
                return Convert.ToInt32(0.5 * y + 0.75 * d + m / 2);
            }
            else if (m > y && m <= (y + d / 2))
            {
                if (m > (y + d / 4))
                {
                    if (w == 1)
                    {
                        return m + d / 4 + 1;
                    }
                    return m + d / 4;
                }
                else
                {
                    return m + d - d / 4;
                }
            }
            else
            {
                int from = x + d / 2 + 1;
                int to = x + d / 2 + Convert.ToInt32(Math.Pow(2, a - 2));
                for (int i = (a - 2); i >= (w - 1); i--)
                {
                    if (m >= from && m <= to)
                    {
                        return m + Convert.ToInt32(Math.Pow(2, i));
                    }
                    else if (m > to && m <= (to + Convert.ToInt32(Math.Pow(2, i))))
                    {
                        if ((m % 2 == 0 && d == 2) || m % 2 == 1 && d != 2)
                        {
                            return Convert.ToInt32((m + 1 - to) / 2 + to + Math.Pow(2, i));
                        }
                        else
                        {
                            return Convert.ToInt32((m - to) / 2 + to + Math.Pow(2, i));
                        }
                    }
                    from += Convert.ToInt32(2 * Math.Pow(2, i));
                    to += Convert.ToInt32(1.5 * Math.Pow(2, i));
                }
            }
            return 0;
        }

        private int LoseNextMatch(int m, int n, int d)
        {
            int a = Convert.ToInt32(Math.Log2(n));
            int w = Convert.ToInt32(Math.Log2(d));
            int x = 0;

            for (int i = (a - 1); i >= w; i--)
            {
                x = x + Convert.ToInt32(Math.Pow(2, i));
            }

            int y = 2 * x;
            if (m % 2 == 1 && m <= n / 2)
            {
                return ((x + d / 2) + (m + 1) / 2);
            }
            else if (m % 2 == 0 && m <= n / 2)
            {
                return (x + d / 2) + m / 2;
            }
            else if (m > n / 2 && m <= 0.75 * n)
            {
                return (x + d / 2 + n + (1 - m));
            }
            else if (m > 0.75 * n && m <= x)
            {
                for (int i = a - 3; i >= w; i--)
                {
                    int count1 = 0;
                    for (int j = a - 1; j >= i + 1; j--)
                    {
                        count1 += Convert.ToInt32(Math.Pow(2, j));
                    }

                    int count2 = count1 + Convert.ToInt32(Math.Pow(2, i));

                    if (m > count1 && m <= count2)
                    {
                        if (m % 2 == 1)
                        {
                            return x + d / 2 + count1 - (count2 - m) + 1;
                        }
                        else if (m % 2 == 0)
                        {
                            return x + d / 2 + count1 - (count2 - m) - 1;
                        }
                    }
                }

            }
            else if (m % 2 == 1 && m > x && m <= (x + Convert.ToInt32(Math.Pow(2, (w - 1)))))
            {
                if (w == 1)
                {
                    return y + m - x;
                }
                return y + (m - x + 1);
            }
            else if (m % 2 == 0 && m > x && m <= (x + Convert.ToInt32(Math.Pow(2, (w - 1)))))
            {
                return y + (m - x - 1);
            }
            return 0;
        }

        private string GenerateMatchCode(int matchNum, int playerNumber, int? finalSinglePlayer)
        {
            if (finalSinglePlayer != null)
            {
                int roundNumber = Convert.ToInt32(Math.Log2(playerNumber / 2));
                int finalSingleRound = roundNumber - Convert.ToInt32(Math.Log2(finalSinglePlayer.Value)) + 1;
                int numberMatchEachRound;
                int count = 0;
                int winRound = 0;
                int loseRound = 0;

                for (int i = 0; i <= finalSingleRound; i++)
                {
                    winRound++;
                    numberMatchEachRound = Convert.ToInt32(Math.Pow(2, roundNumber - i));
                    for (int j = 0; j < numberMatchEachRound; j++)
                    {
                        if (i == 0)
                        {
                            if (++count == matchNum)
                            {
                                return "W" + winRound;
                            }
                        }
                        else
                        {
                            if (++count == matchNum)
                            {
                                return "W" + winRound;
                            }
                        }
                    }
                }
                for (int i = 1; i <= finalSingleRound; i++)
                {
                    loseRound++;
                    numberMatchEachRound = Convert.ToInt32(Math.Pow(2, roundNumber - i));
                    for (int j = 0; j < numberMatchEachRound; j++)
                    {
                        if (++count == matchNum)
                        {
                            return "L" + loseRound;
                        }
                    }
                    loseRound++;
                    for (int j = 0; j < numberMatchEachRound; j++)
                    {
                        if (++count == matchNum)
                        {
                            return "L" + loseRound;
                        }
                    }
                }

                for (int i = finalSingleRound; i <= roundNumber; i++)
                {
                    winRound++;
                    numberMatchEachRound = Convert.ToInt32(Math.Pow(2, roundNumber - i));
                    if (i == finalSingleRound)
                    {
                        for (int j = 0; j < numberMatchEachRound; j++)
                        {
                            if (++count == matchNum)
                            {
                                return "W" + winRound;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < numberMatchEachRound; j++)
                        {
                            if (++count == matchNum)
                            {
                                return "W" + winRound;
                            }
                        }
                    }

                }
            }
            else
            {
                int roundNumber = Convert.ToInt32(Math.Log2(playerNumber / 2));
                int finalSingleRound = roundNumber;
                int numberMatchEachRound;
                int count = 0;
                int winRound = 0;
                for (int i = 0; i <= finalSingleRound; i++)
                {
                    winRound++;
                    numberMatchEachRound = Convert.ToInt32(Math.Pow(2, roundNumber - i));
                    for (int j = 0; j < numberMatchEachRound; j++)
                    {
                        if (i == 0)
                        {
                            if (++count == matchNum)
                            {
                                return "W" + winRound;
                            }
                        }
                        else
                        {
                            if (++count == matchNum)
                            {
                                return "W" + winRound;
                            }
                        }
                    }
                }
            }
            return string.Empty;
        }

        private string GenerateMatchCodeSingle(int currentMatch, int bracketSize)
        {
            int logForCal = (int)Math.Log2(bracketSize);
            int temp = 0;
            for (int i = 1; i <= logForCal; i++)
            {
                temp += bracketSize / (int)Math.Pow(2, i);
                if (temp >= currentMatch)
                {
                    return "W" + i;
                }
            }
            return string.Empty;
        }

        private int NextMatchSingle(int currentMatch, int bracketSize)
        {
            int nextMatch;
            if (currentMatch % 2 == 0)
            {
                nextMatch = currentMatch / 2 + bracketSize / 2;
            }
            else
            {
                nextMatch = currentMatch / 2 + 1 + bracketSize / 2;
            }
            return nextMatch;
        }
    }
}
