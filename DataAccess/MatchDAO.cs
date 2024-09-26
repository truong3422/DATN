using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class MatchDAO
    {
        private readonly poolcomvnContext _context;

        public MatchDAO(poolcomvnContext poolComContext)
        {
            _context = poolComContext;
        }

        public List<MatchOfTournament> GetMatchOfTournaments(int tourId)
        {
            try
            {
                var lstMatchOfTournament = _context.MatchOfTournaments.Where(item => item.TourId == tourId)
                    .Include(m => m.Table);
                return lstMatchOfTournament.ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void AddMatch(MatchOfTournament match)
        {
            try
            {
                var lstMatchOfTournament = _context.MatchOfTournaments.Add(match);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool CheckExistMatch(int tourId, int matchNumber)
        {
            try
            {
                var match = _context.MatchOfTournaments.FirstOrDefault(m => m.TourId == tourId 
                                                            && m.MatchNumber == matchNumber);
                if (match != null)
                {
                    return true;
                }
                else return false;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public int GetLastest(int tourId, int matchNumber)
        {
            try
            {
                var match = _context.MatchOfTournaments.FirstOrDefault(m => m.TourId == tourId
                                                            && m.MatchNumber == matchNumber);
                return match.MatchId;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public MatchOfTournament GetMatchOfTournamentsByNumber(int tourId, int matchNumber)
        {
            try
            {
                var match = _context.MatchOfTournaments.FirstOrDefault(m => m.TourId == tourId
                                                            && m.MatchNumber == matchNumber);
                return match;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void UpdateMatch(MatchOfTournament matchOfTournament)
        {
            try
            {
                var match = _context.MatchOfTournaments.FirstOrDefault(m => m.MatchId == matchOfTournament.MatchId);
                match.Status = matchOfTournament.Status;
                match.WinToMatch = matchOfTournament.WinToMatch;
                match.LoseToMatch = matchOfTournament.LoseToMatch;
                match.TableId = matchOfTournament.TableId;
                match.StartTime = matchOfTournament.StartTime;
                match.EndTime = matchOfTournament.EndTime;
                _context.Update(match);
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public MatchOfTournament GetMatchForControl(int matchId)
        {
            try
            {
                var match = _context.MatchOfTournaments.Include(m => m.Table)
                                    .Include(m => m.PlayerInMatches).ThenInclude(p => p.Player)
                                    .FirstOrDefault(m => m.MatchId == matchId);
                return match;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public MatchOfTournament GetMatchForSchedule(int matchId)
        {
            try
            {
                var match = _context.MatchOfTournaments.Include(m => m.Table)
                                    .FirstOrDefault(m => m.MatchId == matchId);
                return match;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public MatchOfTournament GetMatchByMatchId(int matchId)
        {
            try
            {
                var match = _context.MatchOfTournaments.Include(m => m.Table)
                    .FirstOrDefault(item => item.MatchId == matchId);
                return match;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public MatchOfTournament? GetMatchByNumber(int? matchNumber, int tourId)
        {
            return _context.MatchOfTournaments.FirstOrDefault(item => item.MatchNumber == matchNumber
                                                    && item.TourId == tourId);
        }

        public List<PlayerInMatch> GetMatchesByPlayerId(int playerId)
        {
            try
            {
                return _context.PlayerInMatches.Where(m => m.PlayerId == playerId).ToList();
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public List<PlayerInMatch> GetLatestMatchesByPlayerId(int playerId)
        {
            try
            {
                return _context.PlayerInMatches
                    .Where(m => m.PlayerId == playerId)
                    .OrderByDescending(m => m.MatchId) 
                    .Take(5) 
                    .ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public  PlayerInMatch GetMatchByMatchIdAndPlayerId(int matchId, int playerId)
        {
            try
            {
                return _context.PlayerInMatches.FirstOrDefault(m => m.MatchId == matchId && m.PlayerId != playerId);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public PlayerInMatch GetPlayerInMatchByMatchId(int matchId,int playerId)
        {
            try
            {
                return _context.PlayerInMatches.FirstOrDefault(m => m.MatchId == matchId && m.PlayerId == playerId);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public PlayerInMatch GetLatestMatchPlayerPlay(int playerId)
        {
            try
            {
                return _context.PlayerInMatches
                    .Where(m => m.PlayerId == playerId)
                    .OrderByDescending(m => m.MatchId)
                    .FirstOrDefault(); 
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public List<PlayerViewStatDTO> GetPlayerLose(int tourId)
        {
            try
            {
                var playerInMatches = _context.PlayerInMatches.Where(x => x.Match.TourId == tourId && x.IsWinner == false)
               .Select(x => new PlayerViewStatDTO
               {
                   MatchId = x.MatchId,
                   PlayerId = x.PlayerId,
                   IsWinner = x.IsWinner,
                   MatchCode = x.Match.MatchCode
               }).OrderBy(x => x.MatchCode).ToList();
                return playerInMatches;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public MatchOfTournament GetTheLastMatchOfTournament(int tourId)
        {
            try
            {
               
                var lastMatch = _context.MatchOfTournaments
                    .Where(m => m.TourId == tourId)
                    .OrderByDescending(m => m.MatchId)
                    .FirstOrDefault();

                if (lastMatch != null)
                {
                    
                    
                    return lastMatch;
                }
                else
                {
                 
                    return null;
                }
            }
            catch (Exception)
            {
               
                throw;
            }
        }

        public List<PlayerInMatch> GetPlayerInLastMatchByMatchId(int matchId)
        {
            try
            {
                return _context.PlayerInMatches.Where(p => p.MatchId == matchId).ToList();
            }
            catch (Exception e)
            {

                throw e;
            }
        }
        public void ModifyMatch(int matchId)
        {
            int numberOfPlayerInMatch = _context.PlayerInMatches.Where(item => item.MatchId == matchId).Count();
            if (numberOfPlayerInMatch == Constant.NumberPlayerOfMatch)
            {
                var match = GetMatchByMatchId(matchId);
                if (match != null)
                {
                    match.Status = Constant.MatchIncoming;
                    UpdateMatch(match);
                }
            }
        }

        public class PlayerViewStatDTO
        {
            public int? PlayerId { get; set; }
            public int? MatchId { get; set; }
            public bool? IsWinner { get; set; }
            public string? MatchCode { get; set; }
        }

        public int GetNumberOfMatchEnd(int tourId)
        {
            try
            {
                int number = _context.MatchOfTournaments.Where(m => m.TourId == tourId &&
                    m.Status == Constant.MatchEnd).Count();
                return number;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public int GetNumberOfMatchPlaying(int tourId)
        {
            try
            {
                int number = _context.MatchOfTournaments.Where(m => m.TourId == tourId &&
                    m.Status == Constant.MatchInprogress).Count();
                return number;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public int GetNumberOfMatchRemain(int tourId)
        {
            try
            {
                int number = _context.MatchOfTournaments.Where(m => m.TourId == tourId &&
                    (m.Status == Constant.MatchTBD || m.Status == Constant.MatchIncoming)).Count();
                return number;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
}
