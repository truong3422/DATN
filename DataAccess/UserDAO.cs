using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class UserDAO
    {
        private readonly poolcomvnContext _context;

        public UserDAO(poolcomvnContext poolComContext)
        {
            _context = poolComContext;
        }

        public List<User> GetAllUsers()
        {
            try
            {
                var users = _context.Users.Include(u => u.Account).ToList();
                return users;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to retrieve accounts: {e.Message}", e);
            }
        }


        public User GetUserById(int userId)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(x => x.UserId == userId);
                if (user != null)
                {
                    return user;
                }
                return null;
            }
            catch (Exception e)
            {
                throw new Exception($"Failed to retrieve accounts: {e.Message}", e);
            }
        }

        public void ChangeUserStatus(int id)
        {
            var usernormalToUpdate = _context.Users.FirstOrDefault(u => u.UserId == id);
            if (usernormalToUpdate != null)
            {
                var accountToUpdate = _context.Accounts.FirstOrDefault(a => a.AccountId == usernormalToUpdate.AccountId);
                if (accountToUpdate != null)
                {
                    accountToUpdate.Status = !accountToUpdate.Status;
                    _context.SaveChanges();
                }
            }
        }

        public int WinMatchNumberById(int playerId)
        {
            var winMatchNum = _context.PlayerInMatches
                .Count(x => x.PlayerId == playerId && x.IsWinner == true);
            return winMatchNum;
        }

        public int LoseMatchNumberById(int playerId)
        {
            var winMatchNum = _context.PlayerInMatches
                .Count(x => x.PlayerId == playerId && x.IsWinner == false);
            return winMatchNum;
        }

        public List<Award> GetTopForPlayer(int playerId)
        {
            var queryTop1_2 = (from list1 in _context.PlayerInMatches
                               where list1.PlayerId == playerId
                               join list2 in
                                   (from mt in _context.MatchOfTournaments
                                    group mt by mt.TourId into grouped
                                    select new
                                    {
                                        MaxMatch = grouped.Max(x => x.MatchId),
                                        TourID = grouped.Key
                                    }) on list1.MatchId equals list2.MaxMatch
                               join tourList in _context.Tournaments
                               on list2.TourID equals tourList.TourId
                               select new
                               {
                                   TourId = list2.TourID,
                                   TourName = tourList.TourName,
                                   IsWinner = list1.IsWinner
                               }).OrderByDescending(x => x.TourId).ToList();

            List<Award> awards = new List<Award>();

            foreach (var item in queryTop1_2)
            {
                Award award = new Award()
                {
                    TourId = item.TourId,
                    TourName = item.TourName,
                    TourTop = item.IsWinner == true ? 1 : 2
                };
                awards.Add(award);
            }

            var queryTop3_4 = from list1 in (
                from p in _context.PlayerInMatches
                join m in (
                    from m1 in (
                        from mt in _context.MatchOfTournaments
                        group mt by mt.TourId into g
                        select new { TourID = g.Key, MaxMatch = g.Max(mt => mt.MatchId) - 1 }
                    ).Concat(
                        from m2 in (
                            from mt in _context.MatchOfTournaments
                            join t in _context.Tournaments on mt.TourId equals t.TourId
                            where t.KnockoutPlayerNumber > 2
                            group mt by mt.TourId into g
                            select new { TourID = g.Key, MaxMatch = g.Max(mt => mt.MatchId) - 2 }
                        )
                        select m2
                    )
                    select m1
                ) on p.MatchId equals m.MaxMatch
                where p.PlayerId == playerId && p.IsWinner == false
                select new { p.MatchId, p.IsWinner }
            )
                              join list2 in _context.MatchOfTournaments on list1.MatchId equals list2.MatchId
                              join list3 in _context.Tournaments on list2.TourId equals list3.TourId
                              select new {list3.TourId, list3.TourName, list1.IsWinner };

            foreach (var item in queryTop3_4)
            {
                Award award = new Award()
                {
                    TourId = item.TourId,
                    TourName = item.TourName,
                    TourTop = 3
                };
                awards.Add(award);
            }
            return awards;
        }

        public List<User> GetUserList()
        {
            List<User> list = _context.Users
                .Include(x=> x.Account)
                .Include(x=> x.Players)
                .ThenInclude(x=> x.PlayerInMatches)
                .ToList();
            return list;
        }

        public int GetTournamentUserJoined(int playerId)
        {
            int result = _context.Tournaments
                .Where(x => x.MatchOfTournaments
                .Any(match => match.PlayerInMatches.Any(pim => pim.PlayerId == playerId)))
                .Count();
            return result;
        }

        public string GetProvinceByUserId(int userId)
        {
            return  _context.Users.Where(x=> x.UserId == userId)
                .Select(x=> x.WardCodeNavigation.DistrictCodeNavigation.ProvinceCodeNavigation.Name)
                .FirstOrDefault();
        }

        public void UpdateUser(User user)
        {
            try
            {
                _context.Users.Update(user);
                _context.SaveChanges();
            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
