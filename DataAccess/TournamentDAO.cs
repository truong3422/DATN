using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class TournamentDAO
    {
        private readonly poolcomvnContext _context;

        public TournamentDAO(poolcomvnContext poolComContext)
        {
            _context = poolComContext;
        }

        public Tournament GetTournament(int tourId)
        {
            try
            {
                var tournament = _context.Tournaments.Include(x => x.Club)
                    .FirstOrDefault(item => item.TourId == tourId);

                return tournament;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Tournament GetLastestTournament()
        {
            try
            {
                var tournament = _context.Tournaments.OrderByDescending(e => e.TourId).FirstOrDefault();

                return tournament;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public List<Tournament> GetAllTournament()
        {
            try
            {
                var tournaments = _context.Tournaments.Where(i => i.Status != Constant.CreateTourStepThree
                    && i.Status != Constant.CreateTourStepTwo 
                    && i.Status != Constant.CreateTourStepOne
                    && i.Access.Value).Include(t => t.Club).ToList();
                return tournaments;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public void UpdateTournament(Tournament tournament)
        {
            try
            {
                var updateTournament = tournament;
                _context.Tournaments.Update(updateTournament);
                _context.SaveChanges();
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public List<Tournament> GetTournamentBySearch(string searchQuery)
        {
            try
            {
                searchQuery = searchQuery.ToLower();
                var tournaments = _context.Tournaments
                    .Include(t => t.Club)
                    .Where(t =>
                        t.TourName.ToLower().Contains(searchQuery) ||
                        t.Club.ClubName.ToLower().Contains(searchQuery) ||
                        t.Club.Address.ToLower().Contains(searchQuery)
                    ).ToList();

                return tournaments;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tìm kiếm giải đấu: {ex.Message}");
                return null;
            }
        }
        public List<Tournament> GetTournamentsByFilters(string? gameTypeName, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                IQueryable<Tournament> query = _context.Tournaments
                    .Include(t => t.GameType)
                    .Include(t => t.Club);

                if (!string.IsNullOrEmpty(gameTypeName) && startDate.HasValue && endDate.HasValue)
                {
                    query = query.Where(t => t.GameType.TypeName == gameTypeName && (t.StartDate >= startDate && t.EndDate <= endDate));
                }
                else
                {
                    if (!string.IsNullOrEmpty(gameTypeName))
                    {
                        query = query.Where(t => t.GameType.TypeName == gameTypeName);
                    }

                    if (startDate.HasValue && endDate.HasValue)
                    {
                        query = query.Where(t => t.StartDate >= startDate && t.EndDate <= endDate);
                    }
                    else
                    {
                        if (startDate.HasValue)
                        {
                            query = query.Where(t => t.StartDate == startDate);
                        }

                        if (endDate.HasValue)
                        {
                            query = query.Where(t => t.EndDate == endDate);
                        }
                    }
                }
                return query.ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void CreateTournament(Tournament tournament)
        {
            try
            {
                var updateTournament = _context.Tournaments.FirstOrDefault(t => t.TourId == tournament.TourId);
                updateTournament = tournament;
                _context.Tournaments.Add(updateTournament);
                _context.SaveChanges();
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        //public bool IsTournamentExist(Tournament tournament)
        //{
        //    try
        //    {
        //        var updateTournament = _context.Tournaments.FirstOrDefault(t => t.TourId == tournament.TourId);
        //        updateTournament = tournament;
        //        _context.Tournaments.Update(updateTournament);
        //    }
        //    catch (Exception e)
        //    {

        //        throw e;
        //    }
        //}

        public int GetTourMaxNumberOfPlayer(int tourId)
        {
            try
            {
                int maxNumberOfPlayerInTour = _context.Tournaments.FirstOrDefault(t => t.TourId == tourId).MaxPlayerNumber;
                return maxNumberOfPlayerInTour;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public int? GetTourKnockoutNumber(int tourId)
        {
            try
            {
                int? maxNumberOfPlayerInTour = _context.Tournaments.FirstOrDefault(t => t.TourId == tourId).KnockoutPlayerNumber;
                return maxNumberOfPlayerInTour;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public List<Tournament> GetTournamentsByClubId(int clubId)
        {
            try
            {
                var tournaments = _context.Tournaments.Include(x => x.Club).Where(t => t.ClubId == clubId).ToList();
                return tournaments;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public int GetTourStatus(int tourId)
        {
            try
            {
                var tournaments = _context.Tournaments.FirstOrDefault(t => t.TourId == tourId).Status;
                return tournaments;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public void UpdateTourStatus(int tourId, int status)
        {
            try
            {
                var tournaments = _context.Tournaments.FirstOrDefault(t => t.TourId == tourId);
                tournaments.Status = (byte)status;
                _context.Tournaments.Update(tournaments);
                _context.SaveChanges();
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public List<Tournament> GetTournamentListByPlayerId(int playerId)
        {
            try
            {
                var tournaments = _context.Tournaments
                    .Include(x => x.Club)
                    .Where(t => _context.MatchOfTournaments
                        .Where(mt => _context.PlayerInMatches
                            .Where(pm => pm.PlayerId == playerId)
                            .Select(pm => pm.MatchId)
                            .Contains(mt.MatchId))
                        .Select(mt => mt.TourId)
                        .Contains(t.TourId))
                    .ToList();

                return tournaments;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public Tournament? GetCreatingTournament(int clubId)
        {
            try
            {
                var tour = _context.Tournaments.Where(t => t.ClubId == clubId
                    && t.Status != Constant.TournamentComplete && t.Status != Constant.TournamentInProgress
                        && t.Status != Constant.TournamentIncoming).FirstOrDefault();
                return tour;
            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
